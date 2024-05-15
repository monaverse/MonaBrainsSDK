using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.Brain.Structs;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.Network.Enums;
using Mona.SDK.Core.Network.Interfaces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Brains.EasyUI;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Core.Brain
{
    public partial class MonaGlobalBrainRunner : MonoBehaviour, IMonaBrainPlayer
    {
        public Action OnStarted = delegate { };
        public Action OnBrainsChanged = delegate { };

        public MonaNetworkSettings _NetworkSettings = new MonaNetworkSettings();

        public string MockWalletAddress;

        public string DefaultIPFSGateway;

        public IMonaNetworkSettings NetworkSettings => _NetworkSettings;

        private List<IMonaBrain> _brains = new List<IMonaBrain>();
        public List<IMonaBrain> Brains => _brains;

        private int _currentBrainIndex = 0;
        private bool _tickLateUpdate;

        private Action<MonaBodyInstantiatedEvent> OnMonaBodyInstantiated;
        private Action<MonaBrainSpawnedEvent> OnBrainSpawned;
        private Action<MonaBrainDestroyedEvent> OnBrainDestroyed;
        private Action<MonaPlayerJoinedEvent> OnMonaPlayerJoined;

        private bool _playerJoined;

        private IMonaBrainBlockchain _blockchain;
        public IMonaBrainBlockchain Blockchain => _blockchain;

        private IMonaBody _playerBody;
        private IMonaBody _playerCameraBody;
        private Camera _playerCamera;
        private int _playerId;

        public Camera SceneCamera
        {
            get
            {
                if (_playerCamera != null) return _playerCamera;
                if (Camera.main != null) return Camera.main;
                return FindObjectOfType<Camera>();
            }
        }

        private List<MonaRemotePlayer> _otherPlayers = new List<MonaRemotePlayer>();

        public IMonaBody PlayerBody => _playerBody;
        public IMonaBody PlayerCameraBody => _playerCameraBody;
        public Camera PlayerCamera => _playerCamera;
        public List<MonaRemotePlayer> OtherPlayers => _otherPlayers;
        public int PlayerId => _playerId;

        public int GetPlayerIdByBody(IMonaBody body)
        {
            if (body == PlayerBody) return PlayerId;
            for(var i = 0;i < _otherPlayers.Count; i++)
            {
                var otherPlayer = _otherPlayers[i];
                if (otherPlayer.Body == body)
                    return otherPlayer.PlayerId;
            }
            return -1;
        }

        private PlayerInput _playerInput;
        public PlayerInput PlayerInput { get => _playerInput; set => _playerInput = value; }

        [SerializeField]
        private List<MonaBrainGraph> _playerBrainGraphs = new List<MonaBrainGraph>();
        public List<MonaBrainGraph> PlayerBrainGraphs => _playerBrainGraphs;

        private MonaBrainInput _brainInput;
        private EasyUIGlobalRunner _easyUIRunner;

        bool IsAndroidOrIOS => Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.tvOS ||
            Application.platform == RuntimePlatform.Android;

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
            if (IsAndroidOrIOS)
                Application.targetFrameRate = 60;

            if (_instance == null)
            {
                Instance = this;

                FindBlockchainAPI();

                OnBrainSpawned = HandleBrainSpawned;
                OnBrainDestroyed = HandleBrainDestroyed;
                OnMonaPlayerJoined = HandleMonaPlayerJoined;
                OnMonaBodyInstantiated = HandleMonaBodyInstantiated;

                MonaEventBus.Register<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), OnMonaBodyInstantiated);
                MonaEventBus.Register<MonaBrainSpawnedEvent>(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), OnBrainSpawned);
                MonaEventBus.Register<MonaBrainDestroyedEvent>(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), OnBrainDestroyed);
                MonaEventBus.Register<MonaPlayerJoinedEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_JOINED_EVENT), OnMonaPlayerJoined);

                MonaEventBus.Trigger<MonaRegisterNetworkSettingsEvent>(new EventHook(MonaCoreConstants.REGISTER_NETWORK_SETTINGS_EVENT), new MonaRegisterNetworkSettingsEvent(NetworkSettings));
            }
        }

        private void FindBlockchainAPI()
        {
            var blockchains = GameObject.FindObjectsOfType<MonaBrainBlockchain>();
            for (var i = 0; i < blockchains.Length; i++)
            {
                if (blockchains[i].enabled)
                {
                    _blockchain = blockchains[i];
                    break;
                }
            }
        }

        private void Start()
        {
            SetupEasyUIGlobalRunner();
            CacheCamera();
        }

        private void CacheCamera()
        {

            if (_playerCamera == null)
                _playerCamera = Camera.main;

            if (_playerCamera == null)
                _playerCamera = FindObjectOfType<Camera>();

        }

        private void OnDestroy()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), OnMonaBodyInstantiated);
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), OnBrainSpawned);
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_DESTROYED_EVENT), OnBrainDestroyed);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.ON_PLAYER_JOINED_EVENT), OnMonaPlayerJoined);
        }
        
        private void HandleMonaBodyInstantiated(MonaBodyInstantiatedEvent evt)
        {
#if (!OLYMPIA)
            IMonaNetworkSpawner mockSpawner = null;
            MonaEventBus.Trigger(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_STARTED_EVENT, evt.Body), new NetworkSpawnerStartedEvent(mockSpawner));
#endif
        }

        public void HandleWalletConnected(string address)
        {
            Debug.Log($"{nameof(HandleWalletConnected)} a wallet has been connected");
            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), new MonaWalletConnectedEvent(address));
        }

        public void HandleWalletDisconnected(string address)
        {
            Debug.Log($"{nameof(HandleWalletDisconnected)} a wallet has been disconnected");
            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.WALLET_DISCONNECTED_EVENT), new MonaWalletConnectedEvent(address));
        }

        private void HandleBrainSpawned(MonaBrainSpawnedEvent evt)
        {
            if (evt.Brain.LoggingEnabled)
                Debug.Log($"{nameof(HandleBrainSpawned)} {transform.name} {evt.Brain.LocalId}");
            if (!_brains.Contains(evt.Brain))
                _brains.Add(evt.Brain);

            evt.Brain.SetMonaBrainPlayer(this);

            OnBrainsChanged?.Invoke();
        }

        private void HandleBrainDestroyed(MonaBrainDestroyedEvent evt)
        {
            if(evt.Brain.LoggingEnabled)
                Debug.Log($"{nameof(HandleBrainDestroyed)} {transform.name} {evt.Brain.LocalId}");
            if (_brains.Contains(evt.Brain))
                _brains.Remove(evt.Brain);

            OnBrainsChanged?.Invoke();
        }

        private void HandleMonaPlayerJoined(MonaPlayerJoinedEvent evt)
        {
            var allGlobalRunners = new List<MonaGlobalBrainRunner>(GameObject.FindObjectsByType<MonaGlobalBrainRunner>(FindObjectsSortMode.None));
            allGlobalRunners.Remove(MonaGlobalBrainRunner.Instance);
            
            /* if there are multiple instances of mona global brain, make sure all the player graphs are added */
            if(allGlobalRunners.Count > 0)
            {
                for (var i = 0; i < allGlobalRunners.Count; i++)
                {
                    var otherRunner = allGlobalRunners[i];
                    for(var j = 0; j < otherRunner.PlayerBrainGraphs.Count; j++)
                    {
                        var brain = otherRunner.PlayerBrainGraphs[j];
                        if (!MonaGlobalBrainRunner.Instance.PlayerBrainGraphs.Contains(brain))
                            MonaGlobalBrainRunner.Instance.PlayerBrainGraphs.Add(brain);
                    }
                }
            }

            if (evt.IsLocal)
            {
                _playerBody = evt.PlayerBody;
                _playerCameraBody = _playerBody.FindChildByTag(MonaCoreConstants.MONA_TAG_PLAYER_CAMERA);
                _playerId = evt.PlayerId;

                if (_playerCameraBody != null)
                {
                    _playerCamera = _playerCameraBody.Transform.GetComponent<Camera>();
                    Debug.Log($"{nameof(HandleMonaPlayerJoined)} {_playerCamera}", _playerCamera.gameObject);
                }

                if (_playerCamera == null)
                    _playerCamera = Camera.main;

                if (_playerCamera == null)
                    _playerCamera = FindObjectOfType<Camera>();

                AttachBrainsToPlayer(_playerBody);

                for (var i = 0; i < _brains.Count; i++)
                    _brains[i].SetMonaBrainPlayer(this);

                OnStarted?.Invoke();
            }
            else
            {
                var remotePlayer = new MonaRemotePlayer() { Body = evt.PlayerBody, PlayerId = evt.PlayerId };
                if (!_otherPlayers.Contains(remotePlayer))
                    _otherPlayers.Add(remotePlayer);

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
                Debug.Log($"ATTACHING {PlayerBrainGraphs.Count} BRAINS TO PLAYER");
                for (var i = 0;i < PlayerBrainGraphs.Count; i++)
                    runner.AddBrainGraph(PlayerBrainGraphs[i]);

                runner.StartBrains(force:true);
            }
            else
            {
                Debug.Log($"NO BRAINS TO ATTACH TO PLAYER");
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
            MonaEventBus.Trigger<MonaFixedTickEvent>(new EventHook(MonaCoreConstants.FIXED_TICK_EVENT), new MonaFixedTickEvent(Time.fixedDeltaTime));
        }

        private void TriggerTick()
        {
            MonaEventBus.Trigger<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), new MonaTickEvent(Time.deltaTime));
        }

        private void TriggerLateTick()
        {
            MonaEventBus.Trigger<MonaLateTickEvent>(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), new MonaLateTickEvent());
        }

        private void SetupEasyUIGlobalRunner()
        {
            _easyUIRunner = GetComponent<EasyUIGlobalRunner>();

            if (_easyUIRunner)
                return;

            _easyUIRunner = gameObject.AddComponent(typeof(EasyUIGlobalRunner)) as EasyUIGlobalRunner;
        }
    }
}