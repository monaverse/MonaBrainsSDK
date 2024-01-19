using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Events;

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

        private void Awake()
        {
            PreloadBrains();
        }

        private void PreloadBrains()
        {
            for (var i = 0; i < _brainGraphs.Count; i++)
            {
                if (_brainGraphs[i] == null) continue;
                var instance = (IMonaBrain)Instantiate(_brainGraphs[i]);
                if (instance != null)
                {
                    instance.Preload(gameObject);
                    _brainInstances.Add(instance);
                    EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT, new MonaBrainSpawnedEvent(instance)));
                }
            }
        }

        private void Start()
        {
            BeginBrains();
        }

        private void BeginBrains()
        {
            _began = true;
            if(_brainInstances.Count > 0)
                Debug.Log($"{nameof(BeginBrains)}");
            OnBegin?.Invoke(this);
            for (var i = 0; i < _brainInstances.Count; i++)
                _brainInstances[i].Begin();
        }

        private void OnDestroy()
        {
            UnloadBrains();
        }

        private void UnloadBrains()
        {
            for (var i = 0; i < _brainInstances.Count; i++)
            {
                var instance = _brainInstances[i];
                EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT, new MonaBrainDestroyedEvent(instance)));
                instance.Unload();
            }
        }
    }
}
