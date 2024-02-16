using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.Brain.Structs;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.Network.Enums;
using Mona.SDK.Core.Network.Interfaces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Brain
{
    public partial class MonaGlobalBrainRunner : MonoBehaviour, IMonaBrainPlayer
    {
        public MonaNetworkSettings _NetworkSettings = new MonaNetworkSettings();

        public IMonaNetworkSettings NetworkSettings => _NetworkSettings;

        private List<IMonaBrain> _brains = new List<IMonaBrain>();
        public List<IMonaBrain> Brains => _brains;

        private int _currentBrainIndex = 0;
        private bool _tickLateUpdate;

        private Action<MonaBrainSpawnedEvent> OnBrainSpawned;
        private Action<MonaBrainDestroyedEvent> OnBrainDestroyed;
        private Action<MonaPlayerJoinedEvent> OnMonaPlayerJoined;

        private bool _playerJoined;

        private IMonaBody _playerBody;
        private IMonaBody _playerCamera;
        private int _playerId;

        private List<MonaRemotePlayer> _otherPlayers = new List<MonaRemotePlayer>();

        public IMonaBody PlayerBody => _playerBody;
        public IMonaBody PlayerCamera => _playerCamera;
        public List<MonaRemotePlayer> OtherPlayers => _otherPlayers;
        public int PlayerId => _playerId;

        private PlayerInput _playerInput;
        public PlayerInput PlayerInput { get => _playerInput; set => _playerInput = value; }

        [SerializeField]
        private List<MonaBrainGraph> _playerBrainGraphs = new List<MonaBrainGraph>();
        public List<MonaBrainGraph> PlayerBrainGraphs => _playerBrainGraphs;

        private MonaBrainInput _brainInput;

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
                var existing = GameObject.FindObjectOfType<MonaGlobalBrainRunner>();
                if (existing != null)
                {
                    existing.Awake();
                }
                else
                {
                    var go = new GameObject();
                    var runner = go.AddComponent<MonaGlobalBrainRunner>();
                    go.name = nameof(MonaGlobalBrainRunner);
                    go.transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
                    runner.Awake();
                }
            }
        }

        public MonaBrainInput GetBrainInput()
        {
            if (_brainInput == null)
            {
                _brainInput = GetComponent<MonaBrainInput>();
                if (_brainInput == null)
                    _brainInput = gameObject.AddComponent<MonaBrainInput>();
                _brainInput.SetPlayer(this);
            }
            return _brainInput;
        }

        public void Awake()
        {
            if (_instance == null)
            {
                Instance = this;

                OnBrainSpawned = HandleBrainSpawned;
                OnBrainDestroyed = HandleBrainDestroyed;
                OnMonaPlayerJoined = HandleMonaPlayerJoined;

                EventBus.Register<MonaBrainSpawnedEvent>(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), OnBrainSpawned);
                EventBus.Register<MonaBrainDestroyedEvent>(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), OnBrainDestroyed);
                EventBus.Register<MonaPlayerJoinedEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_JOINED_EVENT), OnMonaPlayerJoined);

                EventBus.Trigger<MonaRegisterNetworkSettingsEvent>(new EventHook(MonaCoreConstants.REGISTER_NETWORK_SETTINGS_EVENT), new MonaRegisterNetworkSettingsEvent(NetworkSettings));
            }
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
            if (evt.Brain.LoggingEnabled)
                Debug.Log($"{nameof(HandleBrainSpawned)} {evt.Brain.LocalId}");
            if (!_brains.Contains(evt.Brain))
                _brains.Add(evt.Brain);

            evt.Brain.SetMonaBrainPlayer(this);
        }

        private void HandleBrainDestroyed(MonaBrainDestroyedEvent evt)
        {
            if(evt.Brain.LoggingEnabled)
                Debug.Log($"{nameof(HandleBrainDestroyed)} {evt.Brain.LocalId}");
            if (_brains.Contains(evt.Brain))
                _brains.Remove(evt.Brain);
        }

        private void HandleMonaPlayerJoined(MonaPlayerJoinedEvent evt)
        {
            if (evt.IsLocal)
            {
                _playerBody = evt.PlayerBody;
                _playerCamera = _playerBody.FindChildByTag(MonaCoreConstants.MONA_TAG_PLAYER_CAMERA);
                _playerId = evt.PlayerId;
                Debug.Log($"{nameof(HandleMonaPlayerJoined)} {_playerCamera.ActiveTransform}", _playerCamera.ActiveTransform.gameObject);

                AttachBrainsToPlayer(_playerBody);

                for (var i = 0; i < _brains.Count; i++)
                    _brains[i].SetMonaBrainPlayer(this);
            }
            else
            {
                var remotePlayer = new MonaRemotePlayer() { Body = evt.PlayerBody, PlayerId = evt.PlayerId };
                if (!_otherPlayers.Contains(remotePlayer))
                    _otherPlayers.Contains(remotePlayer);

                AttachBrainsToPlayer(remotePlayer.Body);

                for (var i = 0; i < _brains.Count; i++)
                    _brains[i].SetMonaBrainPlayer(this);
            }
        }

        private void AttachBrainsToPlayer(IMonaBody body)
        {
            var runner = body.Transform.GetComponent<IMonaBrainRunner>();
            if (runner == null)
                runner = body.Transform.AddComponent<MonaBrainRunner>();

            if (PlayerBrainGraphs.Count > 0)
            {
                for(var i = 0;i < PlayerBrainGraphs.Count; i++)
                    runner.AddBrainGraph(PlayerBrainGraphs[i]);
                runner.PreloadBrains();
                runner.StartBrains();
            }
        }

        private void Update()
        {
            TriggerTick();
        }

        private void FixedUpdate()
        {
            TriggerFixedTick();
        }

        private void LateUpdate()
        {
            TriggerLateTick();
        }

        private void TriggerFixedTick()
        {
            EventBus.Trigger<MonaFixedTickEvent>(new EventHook(MonaCoreConstants.FIXED_TICK_EVENT), new MonaFixedTickEvent(Time.fixedDeltaTime));
        }

        private void TriggerTick()
        {
            EventBus.Trigger<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), new MonaTickEvent(Time.deltaTime));
        }

        private void TriggerLateTick()
        {
            EventBus.Trigger<MonaLateTickEvent>(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), new MonaLateTickEvent());
        }
    }
}