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

namespace Mona.SDK.Brains.Core.Brain
{
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

        private List<Dictionary<Type, Coroutine>> _coroutine = new List<Dictionary<Type, Coroutine>>();
        
        public struct WaitFrameQueueItem
        {
            public int Index;
            public Action<IInstructionEvent> Callback;
            public IInstructionEvent Evt;
            public Type Type;
        }

        private List<List<WaitFrameQueueItem>> _list = new List<List<WaitFrameQueueItem>>();

        public void WaitFrame(Action callback)
        {
            StartCoroutine(WaitFrameCallback(callback));
        }

        private IEnumerator WaitFrameCallback(Action callback)
        {
            yield return null;
            callback?.Invoke();
        }

        public void WaitFrame(int index, Action<IInstructionEvent> callback, IInstructionEvent evt, Type type)
        {
            if (!_coroutine[index].ContainsKey(type))
                _coroutine[index].Add(type, null);

            if (_coroutine[index][type] != null) return;
            if (gameObject.activeInHierarchy)
            {
                if(evt is MonaTriggerEvent)
                    StartCoroutine(DoWaitFrame(index, callback, evt, type));
                else
                    _coroutine[index][type] = StartCoroutine(DoWaitFrame(index, callback, evt, type));
            }
            else
                _list[index].Add(new WaitFrameQueueItem() { Index = index, Callback = callback, Evt = evt, Type = type });
        }

        private void ClearWaitFrameQueue(int index)
        {
            while(_list[index].Count > 0)
            {
                var item = _list[index][0];
                Debug.Log($"{nameof(ClearWaitFrameQueue)} {item.Type}");
                _coroutine[index][item.Type] = StartCoroutine(DoWaitFrame(index, item.Callback, item.Evt, item.Type));
                _list[index].RemoveAt(0);
            }
        }

        private IEnumerator DoWaitFrame(int index, Action<IInstructionEvent> callback, IInstructionEvent evt, Type type)
        { 
            yield return null;
            _coroutine[index][type] = null;
            callback(evt);
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

            _coroutine.Clear();
            _list.Clear();
            _brainInstances.Clear();

            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] == null) continue;
                var instance = (IMonaBrain)Instantiate(_brainGraphs[i]);
                if (instance != null)
                {
                    _coroutine.Add(new Dictionary<Type, Coroutine>());
                    instance.Guid = _brainGraphs[i].Guid;
                    instance.LoggingEnabled = _brainGraphs[i].LoggingEnabled;
                    instance.Preload(gameObject, this, i);
                    _list.Add(new List<WaitFrameQueueItem>());
                    _brainInstances.Add(instance);
                }
            }
        }

        private void HandleHotReload(MonaBrainReloadEvent evt)
        {
            RestartBrains();
        }

        public void StartBrains()
        {
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
