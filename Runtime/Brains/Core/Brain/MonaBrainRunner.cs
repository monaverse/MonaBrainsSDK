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

namespace Mona.SDK.Brains.Core.Brain
{
    [RequireComponent(typeof(MonaBody))]
    public partial class MonaBrainRunner : MonoBehaviour, IMonaBrainRunner, IMonaTagged
    {
        [Serializable]
        public struct ResetTransform
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Transform Parent;
            public IMonaBody Body;

            public ResetTransform(IMonaBody body)
            {
                Body = body;
                Parent = body.ActiveTransform.parent;
                Position = body.ActiveTransform.position;
                Rotation = body.ActiveTransform.rotation;
            }
        }

        public event Action<IMonaBrainRunner> OnBegin;

        [SerializeField]
        private List<MonaBrainGraph> _brainGraphs = new List<MonaBrainGraph>();
        public List<MonaBrainGraph> BrainGraphs => _brainGraphs;

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

            _transformDefaults.Clear();
            _transformDefaults.Add(new ResetTransform(_body));
        }

        private void AddDelegates()
        {
            OnStateAuthorityChanged = HandleStateAuthorityChanged;
            EventBus.Register(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _body), OnStateAuthorityChanged);
        }

        private Dictionary<Type, Coroutine> _coroutine = new Dictionary<Type, Coroutine>();
        public void WaitFrame(Action<IInstructionEvent> callback, IInstructionEvent evt, Type type)
        {
            if (!_coroutine.ContainsKey(type))
                _coroutine.Add(type, null);

            if (_coroutine[type] != null) return;
            _coroutine[type] = StartCoroutine(DoWaitFrame(callback, evt, type));
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

        private void HandleStarted()
        {
            BeginBrains();
        }

        private void BeginBrains()
        {
            if (!_began && _body.HasControl())
            {
                _began = true;
                if (_brainInstances.Count > 0)
                    Debug.Log($"{nameof(BeginBrains)}");
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
        }
    }
}
