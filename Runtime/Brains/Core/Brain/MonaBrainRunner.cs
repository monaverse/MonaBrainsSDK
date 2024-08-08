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
using Mona.SDK.Brains.Core.Control;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Mona.SDK.Core.Utils;
using Unity.Profiling;

namespace Mona.SDK.Brains.Core.Brain
{
    public struct WaitFrameQueueItem
    {
        public int Index;
        public Action<InstructionEvent> Callback;
        public InstructionEvent Evt;
        public int Frame;

        public WaitFrameQueueItem(int index)
        {
            Index = index;
            Callback = null;
            Evt = default;
            Frame = 0;
        }

        public WaitFrameQueueItem(int index, Action<InstructionEvent> callback, InstructionEvent evt, int frame)
        {
            Index = index;
            Callback = callback;
            Evt = evt;
            Frame = frame;
        }

        public bool ShouldExecute()
        {
            return Time.frameCount - Frame > 0;
        }
    }

    public partial class MonaBrainRunner : MonoBehaviour, IMonaBrainRunner, IMonaTagged
    {
        public event Action<IMonaBrainRunner> OnBegin;
        public event Action<string> OnMessage;

        [SerializeField]
        private List<MonaBrainGraph> _brainGraphs = new List<MonaBrainGraph>();
        public List<MonaBrainGraph> BrainGraphs => _brainGraphs;

        [SerializeField]
        private List<string> _brainUrls = new List<string>();
        public List<string> BrainUrls => _brainUrls;

        private List<MonaBrainGraph> _brainGraphsToRemove = new List<MonaBrainGraph>();
        private List<IMonaBrain> _brainInstancesToRemove = new List<IMonaBrain>();

        public IMonaBody Body {
            get
            {
                if (_body == null)
                    _body = transform.GetComponent<IMonaBody>();
                return _body;
            }
        }

        public void SetBrainGraphs(List<MonaBrainGraph> graphs)
        {
            _brainGraphs = graphs;
            PreloadBrains();
        }
        
        public void LoadBrainGraph(string url, Action<List<IMonaBrain>> callback = null)
        {
            StartCoroutine(LoadBrainGraphCoroutine(new List<string>() { url }, callback));
        }

        public void LoadBrainGraphs(List<string> urls, Action<List<IMonaBrain>> callback = null)
        {
            StartCoroutine(LoadBrainGraphCoroutine(urls, callback));
        }

        private IEnumerator LoadBrainGraphCoroutine(List<string> urls, Action<List<IMonaBrain>> callback = null)
        {
            List<IMonaBrain> brains = new List<IMonaBrain>();
            var gateway = GameObject.FindObjectOfType<MonaGlobalBrainRunner>();
            var prefix = gateway != null ? gateway.DefaultIPFSGateway : "";

            for (var i = 0; i < urls.Count; i++)
            {
                var url = urls[i];
                var request = UnityWebRequest.Get(url.IndexOf("http") == -1 ? (prefix+url) : url);
                string data;

                yield return request.SendWebRequest();

                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{nameof(MonaBrainRunner)}.{nameof(LoadBrainGraphCoroutine)} - Request error: {request.error}");
                        data = null;
                        break;
                    case UnityWebRequest.Result.Success:
                        data = request.downloadHandler.text;
                        break;
                    default:
                        Debug.LogError($"{nameof(MonaBrainRunner)}.{nameof(LoadBrainGraphCoroutine)} - Request error: {request.error}");
                        data = null;
                        break;
                }

                if (data != null)
                {
                    var brain = MonaBrainGraph.CreateFromJson(data);
                    brain.SourceUrl = url;
                    brains.Add(brain);
                    if(BrainGraphs.Find(x => x != null && x.SourceUrl == url) == null)
                        AddBrainGraph(brain);
                }
            }

            if (callback != null)
                callback(brains);
            else
                StartBrains(force: true);
        }

        public void AddBrainGraph(MonaBrainGraph graph)
        {
            if (!_brainGraphs.Contains(graph))
                _brainGraphs.Add(graph);
        }

        public void RemoveBrainGraph(MonaBrainGraph graph)
        {
            if (_brainGraphs.Contains(graph))
                _brainGraphs.Remove(graph);

            RemoveBrainGraph(graph.Name);
        }

        public void RemoveBrainGraph(string nameString, bool includeGraphsContainingNameString = false)
        {
            _brainGraphsToRemove.Clear();
            _brainInstancesToRemove.Clear();

            for (int i = 0; i < _brainGraphs.Count; i++)
            {
                if ((includeGraphsContainingNameString && _brainGraphs[i].Name.Contains(nameString)) || (!includeGraphsContainingNameString && _brainGraphs[i].Name == nameString))
                    _brainGraphsToRemove.Add(_brainGraphs[i]);    
            }

            for (int i = 0; i < _brainInstances.Count; i++)
            {
                if ((includeGraphsContainingNameString && _brainInstances[i].Name.Contains(nameString)) || (!includeGraphsContainingNameString && _brainInstances[i].Name == nameString))
                    _brainInstancesToRemove.Add(_brainInstances[i]);
            }

            for (int i = 0; i < _brainGraphsToRemove.Count; i++)
                _brainGraphs.Remove(_brainGraphsToRemove[i]);

            for (int i = 0; i < _brainInstancesToRemove.Count; i++)
            {
                var instance = _brainInstancesToRemove[i];
                MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), new MonaBrainDestroyedEvent(instance));
                instance.Unload(true);
                _brainInstances.Remove(_brainInstancesToRemove[i]);
            }
        }

        public void RemoveAllBrainGraphs()
        {
            UnloadBrains(true);
            _brainGraphs.Clear();
            _brainInstances.Clear();
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
        private Action<NetworkSpawnerInitializedEvent> OnNetworkSpawnerInitialized;

        private List<string> _tags = new List<string>();
        public List<string> MonaTags
        {
            get
            {
                _tags.Clear();
                for (var i = 0; i < _brainGraphs.Count; i++)
                {
                    if(_brainGraphs[i] != null)
                        _tags.AddRange(_brainGraphs[i].MonaTags);
                }
                return _tags;
            }
        }

        public bool HasMonaTag(string tag) {
            for(var i = 0;i < _brainInstances.Count;i++)
            {
                if (_brainInstances[i].HasMonaTag(tag))
                    return true;
            }
            return false;
        }

        public bool HasRigidbodyTiles()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] != null && _brainGraphs[i].HasRigidbodyTiles())
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
            _brainInstances.Clear();
            EnsureGlobalRunnerExists();
            CacheComponents();
            InitVariableCache();
            AddHotReloadDelegates();
            DetectRigidbody();
            LoadUrl();
            AddNetworkSpawnerDelegate();
            //Debug.Log($"{nameof(MonaBrainRunner)} {nameof(Awake)} {gameObject.name}", gameObject);
        }

        private void AddNetworkSpawnerDelegate()
        {
            OnNetworkSpawnerInitialized = HandleNetworkSpawnerInitialized;
            MonaEventBus.Register(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_INITIALIZED_EVENT), OnNetworkSpawnerInitialized);
        }

        private void RemoveNetworkSpawnerDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_INITIALIZED_EVENT), OnNetworkSpawnerInitialized);
        }

        private void HandleNetworkSpawnerInitialized(NetworkSpawnerInitializedEvent evt)
        {
            AddMonaAssetsToNetwork();
        }

        private void AddMonaAssetsToNetwork()
        {
            for(var i = 0;i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] == null) continue;
                _brainGraphs[i].AddMonaAssetsToNetwork();
            }
        }

        private void Start()
        {
            CacheTransforms();
        }

        private void LoadUrl()
        {
            if(_brainUrls.Count > 0)
                LoadBrainGraphs(_brainUrls);
        }

        private void DetectRigidbody()
        {
            if(HasRigidbodyTiles())
            {
                Rigidbody parentRB = _body.Transform.GetComponentInParent<Rigidbody>();
                IMonaBody parentMB = _body.Transform.GetComponentInParent<IMonaBody>();
                IMonaBrainRunner parentRunner = parentMB != null ? parentMB.Transform.GetComponent<IMonaBrainRunner>() : null;

                if (parentRB == null || (parentRunner != null && !(parentMB.SyncType == MonaBodyNetworkSyncType.NetworkRigidbody || parentRunner.HasRigidbodyTiles())))
                    _body.AddRigidbody();
            }
        }

        //static readonly ProfilerMarker _restartBrains = new ProfilerMarker("MonaBrains.RestartBrains");
        private void HandleEnabled()
        {
            //_restartBrains.Begin();
            if (_began)
                RestartBrains();
            //_restartBrains.End();
        }

        private void HandleDisabled()
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
                _body = MonaBodyFactory.Create(gameObject);
            _body.OnDisableOnLoad += HandleDisableOnLoad;
            _body.OnStarted += HandleStarted;
            _body.OnEnabled += HandleEnabled;
            _body.OnDisabled += HandleDisabled;
            _body.OnResumed += HandleResumed;
            _body.OnPaused += HandlePaused;
        }

        public void CacheTransforms()
        {
            if (_body == null) return;
            _transformDefaults.Clear();
            _transformDefaults.Add(new ResetTransform(_body));
        }

        private List<WaitFrameQueueItem> _waitQueue = new List<WaitFrameQueueItem>();
        private List<Dictionary<InstructionEventTypes, WaitFrameQueueItem>> _wait = new List<Dictionary<InstructionEventTypes, WaitFrameQueueItem>>(20);
        private List<Dictionary<IInstruction, WaitFrameQueueItem>> _waitForNextBrainTick = new List<Dictionary<IInstruction, WaitFrameQueueItem>>();

        private HashSet<int> _layersSet = new HashSet<int>(20);
        private List<int> _layers = new List<int>(20);

        private HashSet<InstructionEventTypes> _typesSet = new HashSet<InstructionEventTypes>(100);
        private List<InstructionEventTypes> _types = new List<InstructionEventTypes>(100);

        private HashSet<IInstruction> _instructionsSet = new HashSet<IInstruction>(100);
        private List<IInstruction> _instructions = new List<IInstruction>(100);
        
        private List<List<WaitFrameQueueItem>> _waitInactiveQueue = new List<List<WaitFrameQueueItem>>(20);

        private IEnumerator WaitFrameCallback(Action callback)
        {
            yield return null;
            callback?.Invoke();
        }

        private Action<MonaTickEvent> OnMonaTick;
        private bool _waitFrameRequested;
        private bool _debug;

        /*static readonly ProfilerMarker _processWaitFrame = new ProfilerMarker("MonaBrains.WaitFrame");
        static readonly ProfilerMarker _processWaitFrameQueue = new ProfilerMarker("MonaBrains.WaitFrame.queue");
        static readonly ProfilerMarker _processWaitFrameQueueLayer = new ProfilerMarker("MonaBrains.WaitFrame.queue.layer");
        static readonly ProfilerMarker _processWaitFrameQueueTick = new ProfilerMarker("MonaBrains.WaitFrame.queue.tick");
        static readonly ProfilerMarker _processWaitFrameQueueWait = new ProfilerMarker("MonaBrains.WaitFrame.queue.wait");*/

        public void WaitFrame(int index, Action<InstructionEvent> callback, InstructionEvent evt, bool debug = false)
        {
            //_processWaitFrame.Begin();
            //_processWaitFrameQueue.Begin();
                _debug = debug;

                var instruction = evt.Instruction;
                var type = evt.Type;

            //Debug.Log($"{nameof(WaitFrame)} WAIT WaitFrame {index}, evt: {evt} {Time.frameCount}");
            //_processWaitFrameQueueLayer.Begin();
                if (!_layersSet.Contains(index))
                {
                    _layersSet.Add(index);
                    _layers.Add(index);
                }

                if (!_typesSet.Contains(type))
                {
                    _typesSet.Add(type);
                    _types.Add(type);
                }
            //_processWaitFrameQueueLayer.End();

                var wait = _wait[index];
                var waitForNextBrainTick = _waitForNextBrainTick[index];

                if (type == InstructionEventTypes.Tick)
                {
                //_processWaitFrameQueueTick.Begin();
                    if (!_instructionsSet.Contains(instruction))
                    {
                        _instructionsSet.Add(instruction);
                        _instructions.Add(instruction);
                    }

                    if (!waitForNextBrainTick.ContainsKey(instruction))
                        waitForNextBrainTick.Add(instruction, new WaitFrameQueueItem(-1));
                //_processWaitFrameQueueTick.End();
                }
            //_processWaitFrameQueueWait.Begin();
                if (!wait.ContainsKey(type))
                    wait.Add(type, new WaitFrameQueueItem(-1));
            //_processWaitFrameQueueWait.End();
            //_processWaitFrameQueue.End();

            if (gameObject.activeInHierarchy)
            {
                if (type == InstructionEventTypes.Trigger || type == InstructionEventTypes.Message)
                {
                    _waitQueue.Add(new WaitFrameQueueItem(index, callback, evt, Time.frameCount));
                    _waitFrameRequested = true;
                    //Debug.Log($"add to queue {_waitQueue.Count}");
                }
                else if (type == InstructionEventTypes.Tick)
                {
                    //Debug.Log($"add to tick queue {index} {Time.frameCount} {evt.Type}");
                    var waitForNextBrainTickInstruction = waitForNextBrainTick[instruction];
                    if (waitForNextBrainTickInstruction.Index == -1)
                        _waitForNextBrainTick[index][instruction] = new WaitFrameQueueItem(index, callback, evt, Time.frameCount);
                    _waitFrameRequested = true;
                }
                else
                {
                    if (wait[type].Index == -1)
                        wait[type] = new WaitFrameQueueItem(index, callback, evt, Time.frameCount);
                    _waitFrameRequested = true;
                }
            }
            else
            {
                //Debug.Log($"add to inactive queue {index} {Time.frameCount} {evt.Type}");
                _waitInactiveQueue[index].Add(new WaitFrameQueueItem(index, callback, evt, Time.frameCount));
            }
            //_processWaitFrame.End();
        }

        private void ClearWaitFrameQueue(int index)
        {
            while(_waitInactiveQueue[index].Count > 0)
            {
                var item = _waitInactiveQueue[index][0];
                //Debug.Log($"{nameof(ClearWaitFrameQueue)} {index} {Time.frameCount}");
                if (item.Evt.Type == InstructionEventTypes.Tick)
                {
                    _waitForNextBrainTick[index][item.Evt.Instruction] = item;
                }
                else
                {
                    _wait[index][item.Evt.Type] = item;
                }
                _waitInactiveQueue[index].RemoveAt(0);
            }
        }

        private void BeginBrains()
        {
            //Debug.Log($"{nameof(BeginBrains)} {_body.Transform.name}", _body.Transform.gameObject);
            OnBegin?.Invoke(this);
            for (var i = 0; i < _brainInstances.Count; i++)
                _brainInstances[i].Begin();
        }

        private void HandleMonaTick(MonaTickEvent evt)
        {
            /*if (_beginBrainsAfterFrame > 0 && Time.frameCount - _beginBrainsAfterFrame > 0)
            {
                _beginBrainsAfterFrame = 0;
                BeginBrains();             
                return;
            }*/

            if (!_began) return;

            _waitFrameRequested = false; //if this gets flipped to true during the following callbacks, we know an instruction tile added another frame request to the queue recursively.

            var queueCleared = true;

            for (var i = 0; i < _layers.Count; i++)
            {
                for (var j = 0; j < _types.Count; j++)
                {
                    if(_wait.Count <= _layers[i] || !_wait[_layers[i]].ContainsKey(_types[j]))
                    {
                        //Debug.Log($"can't find event type {_layers[i]} {_types[j]}");
                        continue;
                    }
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

                for (var j = 0; j < _instructions.Count; j++)
                {
                    if (_waitForNextBrainTick.Count <= _layers[i] || !_waitForNextBrainTick[_layers[i]].ContainsKey(_instructions[j]))
                    {
                        //Debug.Log($"can't find event type {_layers[i]} {_types[j]} {j} {Time.frameCount}");
                        continue;
                    }
                    var item = _waitForNextBrainTick[_layers[i]][_instructions[j]];
                   // if (_debug)
                        //Debug.Log($"{nameof(MonaBrainRunner)} WAIT INSTRUCTION TICK LATER {item.Index}, evt: {(item.Evt).Instruction.InstructionTiles[0]} {Time.frameCount} {item.ShouldExecute()}");

                    if (item.Index >= 0)
                    {
                        if (item.ShouldExecute())
                        {
                            item.Index = -1;
                            _waitForNextBrainTick[_layers[i]][_instructions[j]] = item;
                            item.Callback(item.Evt);
                        }
                        else
                            queueCleared = false;
                    }
                }
            }

            var count = _waitQueue.Count - 1;
            //Debug.Log($"clear queue {_waitQueue.Count} fr:{Time.frameCount}");
            for (var i = count; i >= 0; i--)
            {
                var item = _waitQueue[i];
                if (item.ShouldExecute())
                {
                    //Debug.Log($"{nameof(MonaBrainRunner)} WAIT QUEUE TICK LATER {i} idx, {item.Index}, type {item.Type}, evt: {item.Evt} {((MonaBrainTickEvent)item.Evt).Type} {Time.frameCount}");
                    _waitQueue.Remove(item);
                    item.Callback(item.Evt);
                    if (_waitQueue.Count >= i)
                    {
                        //Debug.Log($"the Callback resulted in another queue item recursively {i} {_waitQueue.Count} {Time.frameCount}");
                        //queueCleared = false;
                    }
                }
                else
                {
                    queueCleared = false;
                }
            }

            if (queueCleared && _waitQueue.Count == 0 && !_waitFrameRequested)
            {
                //if (_debug)
                 //   Debug.Log($"STOP LISTENIGN FOR TICK");
                //_waitQueue.Clear();
                //MonaEventBus.Unregister(new EventHook(MonaCoreConstants.TICK_EVENT), OnMonaTick);
                //OnMonaTick = null;
            }
        }

        private void InitVariableCache()
        {
            for(var i = 0;i < BrainGraphs.Count; i++)
                BrainGraphs[i]?.DefaultVariables.CacheVariableNames();
        }

        private void AddHotReloadDelegates()
        {
            OnHotReload = HandleHotReload;
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if(_brainGraphs[i] != null)
                    MonaEventBus.Register<MonaBrainReloadEvent>(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brainGraphs[i].Guid), OnHotReload);
            }
        }

        private void RemoveHotReloadDelegates()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] != null)
                    MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brainGraphs[i].Guid), OnHotReload);
            }
        }

        public void PreloadBrains()
        {
            //Debug.Log($"{nameof(PreloadBrains)} {_body.Transform.name} instances; {_brainInstances.Count}", _body.Transform.gameObject);
            _body.InitializeTags();

            for (var i = 0; i < _wait.Count; i++)
                _wait[i].Clear();

            for (var i = 0; i < _waitInactiveQueue.Count; i++)
                _waitInactiveQueue[i].Clear();

            for (var i = 0; i < _waitForNextBrainTick.Count; i++)
                _waitForNextBrainTick[i].Clear();

            _waitFrameRequested = false;

            if (_brainInstances.Count == 0)
            {
                _brainGraphs.RemoveAll(x => x == null);

                for (var i = 0; i < _brainGraphs.Count; i++)
                {
                    if (_brainGraphs[i] == null) continue;
                    var instance = (IMonaBrain)Instantiate(_brainGraphs[i]);
                    if (instance != null)
                    {
                        _wait.Add(new Dictionary<InstructionEventTypes, WaitFrameQueueItem>(10));
                        _waitForNextBrainTick.Add(new Dictionary<IInstruction, WaitFrameQueueItem>(20));
                        instance.Guid = _brainGraphs[i].Guid;
                        instance.ListenGuid = Guid.NewGuid().ToString();
                        instance.LoggingEnabled = _brainGraphs[i].LoggingEnabled;
                        instance.Preload(gameObject, this, i);
                        _waitInactiveQueue.Add(new List<WaitFrameQueueItem>(10));
                        _brainInstances.Add(instance);
                    }
                }
            }
            else
            {
                for (var i = 0; i < _brainInstances.Count; i++)
                {
                    if (_brainInstances[i] == null) continue;
                    var instance = _brainInstances[i];
                    instance.Preload(gameObject, this, i);
                }
            }

            if (OnMonaTick == null)
            {
                OnMonaTick = HandleMonaTick;
                MonaEventBus.Register<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), OnMonaTick);
            }

        }

        private void HandleHotReload(MonaBrainReloadEvent evt)
        {
            RestartBrains();
        }

        public void StartBrains(bool force = false)
        {
            if (force) _began = false;
            //Debug.Log($"{nameof(MonaBrainRunner)}.{nameof(StartBrains)} start brains {gameObject.name}", gameObject);

            HandleStarted();
        }

        private void HandleDisableOnLoad()
        {
            //Debug.Log($"{nameof(HandleDisableOnLoad)} {gameObject.name}", gameObject);
            PreloadBrains();
        }

        private void HandleStarted()
        {
            if (_began) return;
            //Debug.Log($"{nameof(MonaBrainRunner)}.{nameof(HandleStarted)} start brains {_body.Transform.name} active? {gameObject.activeInHierarchy}", _body.Transform.gameObject);

            PreloadBrains();

            _began = true;
            //_beginBrainsAfterFrame = Time.frameCount;
            BeginBrains();
        }

        private void HandleResumed()
        {
            if (_began)
            {
                Debug.Log($"{nameof(HandleResumed)} Resume Brains");
                OnBegin?.Invoke(this);
                for (var i = 0; i < _brainInstances.Count; i++)
                {
                    _brainInstances[i].Resume();
                    ClearWaitFrameQueue(i);
                    //MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brainInstances[i]), new MonaBrainTickEvent(InstructionEventTypes.Tick));
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

        public void RestartBrains(bool clearInstances = false)
        {
            //Debug.Log($"Restart Brains {_body.Transform.name}", _body.Transform.gameObject);
            _began = false;
            UnloadBrains();
            if(clearInstances)
                _brainInstances.Clear();
            PreloadBrains();
            if (gameObject.activeInHierarchy)
                BeginBrainsAgain();
        }

        private void BeginBrainsAgain()
        {
            _began = true;
            //_beginBrainsAfterFrame = Time.frameCount;
            BeginBrains();
        }            

        public void ResetTransforms()
        {
            if (!_began) return;
            for (var i = 0; i < _transformDefaults.Count; i++)
            {
                var d = _transformDefaults[i];
                if (d.Body.ActiveTransform == null || d.Body.ActiveRigidbody == null || d.Body.ActiveTransform.gameObject == null) continue;

                try
                {
                    d.Body.ActiveTransform.SetParent(d.Parent);
                }
                catch(Exception e)
                {
                    Debug.LogWarning($"{nameof(MonaBrainRunner)}.{nameof(ResetTransforms)} {e.Message}");
                }

                if (d.Parent != null)
                {
                    //Debug.Log($"{nameof(ResetTransform)} localPosition: {d.LocalPosition}", d.Body.Transform.gameObject);
                    d.Body.ActiveTransform.localPosition = d.LocalPosition;
                    d.Body.ActiveTransform.localRotation = d.LocalRotation;
                }
                else
                {
                    //Debug.Log($"{nameof(ResetTransform)} Position: {d.Position}", d.Body.Transform.gameObject);
                    d.Body.ActiveTransform.position = d.Position;
                    d.Body.ActiveTransform.rotation = d.Rotation;
                }
            }
        }

        private void OnDestroy()
        {
            RemoveHotReloadDelegates();
            RemoveNetworkSpawnerDelegate();
            UnloadBrains(destroy:true);
        }

        private void UnloadBrains(bool destroy = false)
        {
            if(!destroy)
                ResetTransforms();
            //_began = false;
            //if(_body.Transform != null)
            //    Debug.Log($"{nameof(UnloadBrains)} {_body.Transform.name} instances {_brainInstances.Count}", _body.Transform.gameObject);
            for (var i = 0; i < _brainInstances.Count; i++)
            {
                var instance = _brainInstances[i];
                if(destroy) MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), new MonaBrainDestroyedEvent(instance));
                instance.Unload(destroy);
            }
            /*
            var variableBehaviours = gameObject.GetComponents<MonaBrainVariablesBehaviour>();
            for(var i  = 1;i < variableBehaviours.Length; i++)
            {
                Destroy(variableBehaviours[i]);
            }*/
        }

        public void TriggerMessage(string message)
        {
            OnMessage?.Invoke(message);
        }

        public void SendMessageToTags(string message)
        {
            for (var i = 0; i < _brainInstances.Count; i++)
            {
                var tags = _brainInstances[i].MonaTags;
                for (var j = 0; j < tags.Count; j++)
                {
                    var bodies = MonaBodyFactory.FindByTag(tags[j]);
                    for (var b = 0; b < bodies.Count; b++)
                        MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, bodies[b]), new InstructionEvent(message, null, Time.frameCount));
                }
            }
        }

        public void SendMessageToBody(string message)
        {
            Debug.Log($"{nameof(SendMessageToBody)} {message}");
            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), new InstructionEvent(message, null, Time.frameCount));
        }

        private Keyboard _keyboard;
        public void SimulateKeyPress(string keyName)
        {
            if (_keyboard == null)
            {
                _keyboard = InputSystem.AddDevice<Keyboard>("Mobile");
                InputSystem.EnableDevice(_keyboard);
            }
            Key key = (Key)Enum.Parse(typeof(Key), keyName);
            KeyboardState stateA = new KeyboardState();
            stateA.Press(key);
            InputSystem.QueueStateEvent(_keyboard, stateA);
        }

        public void SimulateKeyRelease(string keyName)
        {
            if (_keyboard == null)
            {
                _keyboard = InputSystem.AddDevice<Keyboard>("Mobile");
                InputSystem.EnableDevice(_keyboard);
            }
            Key key = (Key)Enum.Parse(typeof(Key), keyName);
            Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
            KeyboardState stateA = new KeyboardState();
            stateA.Release(key);
            InputSystem.QueueStateEvent(_keyboard, stateA);
        }
    }
}
