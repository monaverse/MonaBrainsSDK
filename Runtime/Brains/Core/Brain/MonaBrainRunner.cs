using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core;
using System.Collections;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Core.Brain.Structs;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.Body.Enums;

namespace Mona.SDK.Brains.Core.Brain
{
    public struct WaitFrameQueueItem
    {
        public int Index;
        public Action<IInstructionEvent> Callback;
        public IInstructionEvent Evt;
        public Type Type;
        public int Frame;

        public WaitFrameQueueItem(int index)
        {
            Index = index;
            Callback = null;
            Evt = null;
            Type = null;
            Frame = 0;
        }

        public WaitFrameQueueItem(int index, Action<IInstructionEvent> callback, IInstructionEvent evt, Type type, int frame)
        {
            Index = index;
            Callback = callback;
            Evt = evt;
            Type = type;
            Frame = frame;
        }

        public bool ShouldExecute()
        {
            return Time.frameCount - Frame > 0;
        }
    }

    [RequireComponent(typeof(MonaBody))]
    public partial class MonaBrainRunner : MonoBehaviour, IMonaBrainRunner, IMonaTagged
    {
        public event Action<IMonaBrainRunner> OnBegin;

        [SerializeField]
        private List<MonaBrainGraph> _brainGraphs = new List<MonaBrainGraph>();
        public List<MonaBrainGraph> BrainGraphs => _brainGraphs;

        public void SetBrainGraphs(List<MonaBrainGraph> graphs)
        {
            _brainGraphs = graphs;
            PreloadBrains();
        }

        public void AddBrainGraph(MonaBrainGraph graph)
        {
            if (!_brainGraphs.Contains(graph))
                _brainGraphs.Add(graph);
        }

        private List<IMonaBrain> _brainInstances = new List<IMonaBrain>();
        public List<IMonaBrain> BrainInstances => _brainInstances;

        private IMonaBody _body;

        private List<ResetTransform> _transformDefaults = new List<ResetTransform>();

        private bool _began;
        public bool Began => _began;

        public bool LegacyMonaPlatforms {
            get
            {
                for(var i = 0;i < _brainInstances.Count; i++)
                {
                    var instance = _brainInstances[i];
                    if(instance.LegacyMonaPlatforms)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private Action<MonaBrainReloadEvent> OnHotReload;

        public bool HasMonaTag(string tag) {
            for(var i = 0;i < _brainInstances.Count;i++)
            {
                if (_brainInstances[i].HasMonaTag(tag))
                    return true;
            }
            return false;
        }

        private Action<MonaStateAuthorityChangedEvent> OnStateAuthorityChanged;

        public bool RequiresAnimator
        {
            get
            {
                for(var i = 0; i < _brainGraphs.Count; i++)
                {
                    if (_brainGraphs[i] == null) continue;
                    if (_brainGraphs[i].HasAnimationTiles())
                        return true;
                }
                return false;
            }
        }

        private void Awake()
        {
            EnsureGlobalRunnerExists();
            CacheComponents();
            AddHotReloadDelegates();
        }

        private void OnEnable()
        {
            if (_began)
                RestartBrains();
        }

        private void OnDisable()
        {
            if(_began)
                UnloadBrains();
        }

        private void EnsureGlobalRunnerExists()
        {
            MonaGlobalBrainRunner.Init();
        }

        private void CacheComponents()
        {
            _body = GetComponent<IMonaBody>();
            if (_body == null)
                _body = gameObject.AddComponent<MonaBody>();
            _body.OnStarted += HandleStarted;
            _body.OnResumed += HandleResumed;
            _body.OnPaused += HandlePaused;
            CacheTransforms();
        }

        public void CacheTransforms()
        {
            _transformDefaults.Clear();
            _transformDefaults.Add(new ResetTransform(_body));
        }

        private List<WaitFrameQueueItem> _waitQueue = new List<WaitFrameQueueItem>();
        private List<Dictionary<Type, WaitFrameQueueItem>> _wait = new List<Dictionary<Type, WaitFrameQueueItem>>();
        private List<int> _layers = new List<int>();
        private List<Type> _types = new List<Type>();
        
        private List<List<WaitFrameQueueItem>> _waitInactiveQueue = new List<List<WaitFrameQueueItem>>();

        public void WaitFrame(Action callback)
        {
            StartCoroutine(WaitFrameCallback(callback));
        }

        private IEnumerator WaitFrameCallback(Action callback)
        {
            yield return null;
            callback?.Invoke();
        }

        private Action<MonaTickEvent> OnMonaTick;
        public void WaitFrame(int index, Action<IInstructionEvent> callback, IInstructionEvent evt, Type type)
        {
            //Debug.Log($"{nameof(WaitFrame)} WAIT WaitFrame {index}, type {type}, evt: {evt} {Time.frameCount}");
            if (!_layers.Contains(index))
                _layers.Add(index);

            if (!_types.Contains(type))
                _types.Add(type);

            if (!_wait[index].ContainsKey(type))
                _wait[index].Add(type, new WaitFrameQueueItem(-1));

            if (_wait[index][type].Index > -1) return;
            if (gameObject.activeInHierarchy)
            {
                if (evt is MonaTriggerEvent || evt is MonaBrainTickEvent)
                    _waitQueue.Add(new WaitFrameQueueItem(index, callback, evt, type, Time.frameCount));
                else
                    _wait[index][type] = new WaitFrameQueueItem(index, callback, evt, type, Time.frameCount);

                OnMonaTick = HandleMonaTick;
                EventBus.Register<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), OnMonaTick);
            }
            else
                _waitInactiveQueue[index].Add(new WaitFrameQueueItem(index, callback, evt, type, Time.frameCount));
        }

        private void ClearWaitFrameQueue(int index)
        {
            while(_waitInactiveQueue[index].Count > 0)
            {
                var item = _waitInactiveQueue[index][0];
                Debug.Log($"{nameof(ClearWaitFrameQueue)} {item.Type}");
                _wait[index][item.Type] = item;
                _waitInactiveQueue[index].RemoveAt(0);
            }
        }

        private void HandleMonaTick(MonaTickEvent evt)
        {
            var queueCleared = true;

            for (var i = 0; i < _layers.Count; i++)
            {
                for (var j = 0; j < _types.Count; j++)
                {
                    var item = _wait[_layers[i]][_types[j]];
                    if (item.Index >= 0)
                    {
                        if (item.ShouldExecute())
                        {
                            //Debug.Log($"{nameof(MonaBrainRunner)} WAIT TICK LATER {item.Index}, type {item.Type}, evt: {((MonaBrainTickEvent)item.Evt).Instruction.InstructionTiles[0]} {Time.frameCount}");
                            item.Index = -1;
                            _wait[_layers[i]][_types[j]] = item;
                            item.Callback(item.Evt);
                        }
                        else
                            queueCleared = false;
                    }
                }
            }

            for(var i = _waitQueue.Count-1; i >= 0; i--)
            {
                var item = _waitQueue[i];
                if (item.ShouldExecute())
                {
                    //Debug.Log($"{nameof(MonaBrainRunner)} WAIT QUEUE TICK LATER {item.Index}, type {item.Type}, evt: {item.Evt} {Time.frameCount}");
                    item.Callback(item.Evt);
                    _waitQueue.Remove(item);
                }
                else
                {
                    queueCleared = false;
                }
            }

            if (queueCleared)
            {
                //Debug.Log($"STOP LISTENIGN FOR TICK");
                //_waitQueue.Clear();
                EventBus.Unregister(new EventHook(MonaCoreConstants.TICK_EVENT), OnMonaTick);
            }

        }

        private void AddHotReloadDelegates()
        {
            OnHotReload = HandleHotReload;
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if(_brainGraphs[i] != null)
                    EventBus.Register<MonaBrainReloadEvent>(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brainGraphs[i].Guid), OnHotReload);
            }
        }

        private void RemoveHotReloadDelegates()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] != null)
                    EventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brainGraphs[i].Guid), OnHotReload);
            }
        }

        public void PreloadBrains()
        {
            _body.InitializeTags();

            _wait.Clear();
            _waitQueue.Clear();
            _layers.Clear();
            _waitInactiveQueue.Clear();
            _brainInstances.Clear();
            _brainGraphs.RemoveAll(x => x == null);

            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] == null) continue;
                var instance = (IMonaBrain)Instantiate(_brainGraphs[i]);
                if (instance != null)
                {
                    _wait.Add(new Dictionary<Type, WaitFrameQueueItem>());
                    instance.Guid = _brainGraphs[i].Guid;
                    instance.LoggingEnabled = _brainGraphs[i].LoggingEnabled;
                    instance.Preload(gameObject, this, i);
                    _waitInactiveQueue.Add(new List<WaitFrameQueueItem>());
                    _brainInstances.Add(instance);
                }
            }
        }

        private void HandleHotReload(MonaBrainReloadEvent evt)
        {
            RestartBrains();
        }

        public void StartBrains(bool force = false)
        {
            if (force) _began = false;
            Debug.Log($"{nameof(MonaBrainRunner)}.{nameof(StartBrains)} start brains from external source", _body.Transform.gameObject);
            HandleStarted();
        }

        private void HandleStarted()
        {
            if (_began) return;
            Debug.Log($"{nameof(MonaBrainRunner)}.{nameof(HandleStarted)} start brains {_body.Transform.name}", _body.Transform.gameObject);
            if (!gameObject.activeInHierarchy) return;
            
            PreloadBrains();

            StopCoroutine("BeginBrains");
            StartCoroutine("BeginBrains");
        }

        private void HandleResumed()
        {
            if (_began)
            {
                //Debug.Log($"{nameof(HandleResumed)} Resume Brains");
                OnBegin?.Invoke(this);
                for (var i = 0; i < _brainInstances.Count; i++)
                {
                    _brainInstances[i].Resume();
                    ClearWaitFrameQueue(i);
                    //EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brainInstances[i]), new MonaBrainTickEvent(InstructionEventTypes.Tick));
                }
            }
            else
            {
                HandleStarted();
            }
        }

        private void HandlePaused()
        {
            if(_began)
            {
                OnBegin?.Invoke(this);
                for (var i = 0; i < _brainInstances.Count; i++)
                    _brainInstances[i].Pause();
            }
        }

        private IEnumerator BeginBrains()
        {
            if (!_began)
            {
                _began = true;
                yield return null;
                OnBegin?.Invoke(this);
                for (var i = 0; i < _brainInstances.Count; i++)
                    _brainInstances[i].Begin();
            }
        }

        private void RestartBrains()
        {
            _began = false;
            ResetTransforms();
            UnloadBrains();
            PreloadBrains();
            if(gameObject.activeInHierarchy)
                StartCoroutine(BeginBrainsAgain());
        }

        private IEnumerator BeginBrainsAgain()
        {
            yield return BeginBrains();
        }            

        public void ResetTransforms()
        {
            for (var i = 0; i < _transformDefaults.Count; i++)
            {
                var d = _transformDefaults[i];
                d.Body.ActiveTransform.SetParent(d.Parent);
                d.Body.ActiveTransform.position = d.Position;
                d.Body.ActiveTransform.rotation = d.Rotation;
            }
        }

        private void OnDestroy()
        {
            RemoveHotReloadDelegates();
            UnloadBrains();
        }

        private void UnloadBrains()
        {
            for (var i = 0; i < _brainInstances.Count; i++)
            {
                var instance = _brainInstances[i];
                EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), new MonaBrainDestroyedEvent(instance));
                instance.Unload();
            }

            var variableBehaviours = gameObject.GetComponents<MonaBrainVariablesBehaviour>();
            for(var i  = 1;i < variableBehaviours.Length; i++)
            {
                Destroy(variableBehaviours[i]);
            }
        }
    }
}
