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
    public partial class MonaBrainRunner : MonoBehaviour, IMonaBrainRunner, IMonaTagged
    {
        public event Action<IMonaBrainRunner> OnBegin;

        [SerializeField]
        private List<MonaBrainGraph> _brainGraphs = new List<MonaBrainGraph>();
        public List<MonaBrainGraph> BrainGraphs => _brainGraphs;

        private List<IMonaBrain> _brainInstances = new List<IMonaBrain>();
        public List<IMonaBrain> BrainInstances => _brainInstances;

        private IMonaBody _body;

        private bool _began;
        public bool Began => _began;

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
        }

        private void AddDelegates()
        {
            OnStateAuthorityChanged = HandleStateAuthorityChanged;
            EventBus.Register(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, _body), OnStateAuthorityChanged);
        }

        public void WaitFrame(Action<IBrainMessageEvent> callback, IBrainMessageEvent evt)
        {
            StartCoroutine(DoWaitFrame(callback, evt));
        }

        private IEnumerator DoWaitFrame(Action<IBrainMessageEvent> callback, IBrainMessageEvent evt)
        { 
            yield return null;
            callback(evt);
        }

        private void PreloadBrains()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] == null) continue;
                var instance = (IMonaBrain)Instantiate(_brainGraphs[i]);
                if (instance != null)
                {
                    instance.Preload(gameObject, this);
                    _brainInstances.Add(instance);
                    EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), new MonaBrainSpawnedEvent(instance));
                }
            }
        }

        private void HandleStateAuthorityChanged(MonaStateAuthorityChangedEvent evt)
        {
            if (evt.HasControl)
                BeginBrains();
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

        private void OnDestroy()
        {
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
