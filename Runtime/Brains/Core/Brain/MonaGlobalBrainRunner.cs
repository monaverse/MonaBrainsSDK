using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Brain
{
    public interface IMonaBrainPlayer
    {
        public IMonaBody PlayerBody { get; }
        public int PlayerId { get; }
    }

    public partial class MonaGlobalBrainRunner : MonoBehaviour, IMonaBrainPlayer
    {
        private List<IMonaBrain> _brains = new List<IMonaBrain>();

        [SerializeField]
        private int _brainsPerTick = -1;

        private int _currentBrainIndex = 0;
        private bool _tickLateUpdate;

        private Action<MonaBrainSpawnedEvent> OnBrainSpawned;
        private Action<MonaBrainDestroyedEvent> OnBrainDestroyed;
        private Action<MonaPlayerJoinedEvent> OnMonaPlayerJoined;

        private bool _playerJoined;

        private IMonaBody _playerBody;
        private int _playerId;

        public IMonaBody PlayerBody => _playerBody;
        public int PlayerId => _playerId;

        private PlayerInput _playerInput;
        public PlayerInput PlayerInput { get => _playerInput; set => _playerInput = value; }

        private static MonaGlobalBrainRunner _instance;
        public static MonaGlobalBrainRunner Instance {
            get
            {
                Init();
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public static void Init()
        {
            if (_instance == null)
            {
                var go = new GameObject();
                _instance = go.AddComponent<MonaGlobalBrainRunner>();
                go.name = nameof(MonaGlobalBrainRunner);
                go.transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
            }
        }

        public PlayerInput GetPlayerInput()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput == null)
                _playerInput = gameObject.AddComponent<PlayerInput>();
            return _playerInput;
        }

        private void Awake()
        {
            Instance = this;

            OnBrainSpawned = HandleBrainSpawned;
            OnBrainDestroyed = HandleBrainDestroyed;
            OnMonaPlayerJoined = HandleMonaPlayerJoined;

            EventBus.Register<MonaBrainSpawnedEvent>(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), OnBrainSpawned);
            EventBus.Register<MonaBrainDestroyedEvent>(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), OnBrainDestroyed);
            EventBus.Register<MonaPlayerJoinedEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_JOINED_EVENT), OnMonaPlayerJoined);
        }

        private void Start()
        {
#if UNITY_EDITOR && !OLYMPIA
            IMonaNetworkSpawner mockSpawner = null;
            EventBus.Trigger(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_STARTED_EVENT), new NetworkSpawnerStartedEvent(mockSpawner));
#endif
        }

        private void OnDestroy()
        {
            EventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), OnBrainSpawned);
            EventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), OnBrainDestroyed);
            EventBus.Unregister(new EventHook(MonaCoreConstants.ON_PLAYER_JOINED_EVENT), OnMonaPlayerJoined);
        }

        private void HandleBrainSpawned(MonaBrainSpawnedEvent evt)
        {
            Debug.Log($"{nameof(HandleBrainSpawned)} {evt.Brain.LocalId}");
            if (!_brains.Contains(evt.Brain))
                _brains.Add(evt.Brain);

            evt.Brain.SetMonaBrainPlayer(this);
        }

        private void HandleBrainDestroyed(MonaBrainDestroyedEvent evt)
        {
            Debug.Log($"{nameof(HandleBrainDestroyed)} {evt.Brain.LocalId}");
            if (_brains.Contains(evt.Brain))
                _brains.Remove(evt.Brain);
        }

        private void HandleMonaPlayerJoined(MonaPlayerJoinedEvent evt)
        {
            if (!evt.IsLocal) return;
            _playerBody = evt.PlayerBody;
            _playerId = evt.PlayerId;
        }

        private void Update()
        {
            TriggerTileTick();
        }

        private void TriggerTileTick()
        {
            EventBus.Trigger<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), new MonaTileTickEvent(Time.deltaTime));
        }
    }
}