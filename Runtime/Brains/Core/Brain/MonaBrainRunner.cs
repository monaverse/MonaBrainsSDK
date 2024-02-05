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

        private List<IMonaBrain> _brainInstances = new List<IMonaBrain>();
        public List<IMonaBrain> BrainInstances => _brainInstances;

        private IMonaBody _body;

        private List<ResetTransform> _transformDefaults = new List<ResetTransform>();

        private bool _began;
        public bool Began => _began;

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

        private void Awake()
        {
            EnsureGlobalRunnerExists();
            CacheComponents();
            AddDelegates();
            AddHotReloadDelegates();
            PreloadBrains();
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

            _transformDefaults.Clear();
            _transformDefaults.Add(new ResetTransform(_body));
        }

        private void AddDelegates()
        {
            OnStateAuthorityChanged = HandleStateAuthorityChanged;
            EventBus.Register(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _body), OnStateAuthorityChanged);
        }

        private Dictionary<Type, Coroutine> _coroutine = new Dictionary<Type, Coroutine>();
        
        public struct WaitFrameQueueItem
        {
            public Action<IInstructionEvent> Callback;
            public IInstructionEvent Evt;
            public Type Type;
        }

        private List<WaitFrameQueueItem> _list = new List<WaitFrameQueueItem>();

        public void WaitFrame(Action<IInstructionEvent> callback, IInstructionEvent evt, Type type)
        {
            if (!_coroutine.ContainsKey(type))
                _coroutine.Add(type, null);

            if (_coroutine[type] != null) return;
            if (gameObject.activeInHierarchy)
            {
                if(evt is MonaTriggerEvent)
                    StartCoroutine(DoWaitFrame(callback, evt, type));
                else
                    _coroutine[type] = StartCoroutine(DoWaitFrame(callback, evt, type));
            }
            else
                _list.Add(new WaitFrameQueueItem() { Callback = callback, Evt = evt, Type = type });
        }

        private void ClearWaitFrameQueue()
        {
            while(_list.Count > 0)
            {
                var item = _list[0];
                Debug.Log($"{nameof(ClearWaitFrameQueue)} {item.Type}");
                _coroutine[item.Type] = StartCoroutine(DoWaitFrame(item.Callback, item.Evt, item.Type));
                _list.RemoveAt(0);
            }
        }

        private IEnumerator DoWaitFrame(Action<IInstructionEvent> callback, IInstructionEvent evt, Type type)
        { 
            yield return null;
            callback(evt);
            _coroutine[type] = null;
        }

        private void AddHotReloadDelegates()
        {
            OnHotReload = HandleHotReload;
            for (var i = 0; i < _brainGraphs.Count; i++)
                EventBus.Register<MonaBrainReloadEvent>(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brainGraphs[i].Guid), OnHotReload);
        }

        private void RemoveHotReloadDelegates()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
                EventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brainGraphs[i].Guid), OnHotReload);
        }

        private void PreloadBrains()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] == null) continue;
                var instance = (IMonaBrain)Instantiate(_brainGraphs[i]);
                if (instance != null)
                {
                    instance.Guid = _brainGraphs[i].Guid;
                    instance.LoggingEnabled = _brainGraphs[i].LoggingEnabled;
                    instance.Preload(gameObject, this);
                    _brainInstances.Add(instance);
                    EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), new MonaBrainSpawnedEvent(instance));
                }
            }
        }

        private void HandleHotReload(MonaBrainReloadEvent evt)
        {
            RestartBrains();
        }

        private void HandleStateAuthorityChanged(MonaStateAuthorityChangedEvent evt)
        {
            if (evt.HasControl)
                RestartBrains();
        }

        public void StartBrains()
        {
            HandleStarted();
        }

        private void HandleStarted()
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(BeginBrains());
        }

        private void HandleResumed()
        {
            if (_began)
            {
                //Debug.Log($"{nameof(HandleResumed)} Resume Brains");
                OnBegin?.Invoke(this);
                for (var i = 0; i < _brainInstances.Count; i++)
                    _brainInstances[i].Resume();
                ClearWaitFrameQueue();
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
            yield return null;
            if (!_began && _body.HasControl())
            {
                _began = true;
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
            yield return null;
            BeginBrains();
        }

        private void ResetTransforms()
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
            RemoveDelegates();
            UnloadBrains();
        }

        private void RemoveDelegates()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _body), OnStateAuthorityChanged);
        }

        private void UnloadBrains()
        {
            for (var i = 0; i < _brainInstances.Count; i++)
            {
                var instance = _brainInstances[i];
                EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), new MonaBrainDestroyedEvent(instance));
                instance.Unload();
            }

            var states = gameObject.GetComponents<MonaBrainValues>();
            for(var i  = 1;i < states.Length; i++)
            {
                Destroy(states[i]);
            }
        }
    }
}
