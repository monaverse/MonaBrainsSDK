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
using Mona.SDK.Brains.EasyUI;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Core.Brain
{
    public partial class MonaGlobalBrainRunner : MonoBehaviour, IMonaBrainPlayer
    {
        public Action OnStarted = delegate { };
        public Action<IMonaBrain> OnBrainAddUIEvent = delegate { };
        public Action<IMonaBrain> OnBrainRemoveUIEvent = delegate { };

        public MonaNetworkSettings _NetworkSettings = new MonaNetworkSettings();

        public string MockWalletAddress;

        public string DefaultIPFSGateway;

        public bool NetworkingEnabled;

        public IMonaNetworkSettings NetworkSettings => _NetworkSettings;

        private List<IMonaBrain> _brains = new List<IMonaBrain>();
        public List<IMonaBrain> Brains => _brains;

        private bool _tickLateUpdate;

        private Action<MonaBodyInstantiatedEvent> OnMonaBodyInstantiated;
        private Action<MonaBrainSpawnedEvent> OnBrainSpawned;
        private Action<MonaBrainDestroyedEvent> OnBrainDestroyed;
        private Action<MonaPlayerJoinedEvent> OnMonaPlayerJoined;
        private Action<MonaBrainAddUIEvent> OnBrainAddUI;
        private Action<MonaBrainRemoveUIEvent> OnBrainRemoveUI;

        private bool _playerJoined;

        private IMonaBrainBlockchain _blockchain;
        public IMonaBrainBlockchain Blockchain => _blockchain;

        private IMonaNetworkSpawner _networkSpawner;
        public IMonaNetworkSpawner NetworkSpawner { get => _networkSpawner; set => _networkSpawner = value; }

        private IMonaBody _playerBody;
        private IMonaBody _playerCameraBody;
        private Camera _playerCamera;
        private int _playerId;
        private bool _isHost;

        private List<IMonaTags> _monaTags = new List<IMonaTags>();
        public List<IMonaTags> MonaTags => _monaTags;

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
        public List<MonaRemotePlayer> OtherPlayers => _otherPlayers;

        public IMonaBody PlayerBody { get => _playerBody; set => _playerBody = value; } 
        public IMonaBody PlayerCameraBody { get => _playerCameraBody; set => _playerCameraBody = value; }
        public Camera PlayerCamera { get => _playerCamera; set => _playerCamera = value; }
        public int PlayerId { get => _playerId; set => _playerId = value; }
        public bool IsHost { get => _isHost; set => _isHost = value; }

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

        private IBrainSocialPlatformUser _brainSocialUser;
        public IBrainSocialPlatformUser BrainSocialUser { get => _brainSocialUser; set => _brainSocialUser = value; }

        private IBrainLeaderboard _brainLeaderboards;
        public IBrainLeaderboard BrainLeaderboards { get => _brainLeaderboards; set => _brainLeaderboards = value; }

        private IBrainStorage _localStorage;
        public IBrainStorage LocalStorage { get => _localStorage; set => _localStorage = value; }

        private IBrainStorage _cloudStorage;
        public IBrainStorage ClousStorage { get => _cloudStorage; set => _cloudStorage = value; }



        public void EnablePlayerInput() => GetBrainInput().EnableInput();
        public void DisablePlayerInput() => GetBrainInput().DisableInput();

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
                    try
                    {
                        go.transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
                    }
                    catch
                    {
                        Debug.Log($"{nameof(MonaGlobalBrainRunner)} init no space tag present");
                    }
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
#if OLYMPIA
            NetworkSettings.NetworkType = MonaNetworkType.Shared;
#endif
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
                OnBrainAddUI = HandleBrainAddUI;
                OnBrainRemoveUI = HandleBrainRemoveUI;

                MonaEventBus.Register<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), OnMonaBodyInstantiated);
                MonaEventBus.Register<MonaBrainSpawnedEvent>(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), OnBrainSpawned);
                MonaEventBus.Register<MonaBrainAddUIEvent>(new EventHook(MonaBrainConstants.BRAIN_ADD_UI), OnBrainAddUI);
                MonaEventBus.Register<MonaBrainRemoveUIEvent>(new EventHook(MonaBrainConstants.BRAIN_REMOVE_UI), OnBrainRemoveUI);
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
            SetupStorageSolutions();
            SetupSocialUser();
            SetupLeaderboards();
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
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_ADD_UI), OnBrainAddUI);
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_REMOVE_UI), OnBrainRemoveUI);
        }

        private void HandleBrainAddUI(MonaBrainAddUIEvent evt)
        {
            OnBrainAddUIEvent?.Invoke(evt.Brain);
        }

        private void HandleBrainRemoveUI(MonaBrainRemoveUIEvent evt)
        {
            OnBrainRemoveUIEvent?.Invoke(evt.Brain);
        }

        private void HandleMonaBodyInstantiated(MonaBodyInstantiatedEvent evt)
        {
#if (!OLYMPIA)
            if(NetworkSettings.NetworkType == MonaNetworkType.None)
                MonaEventBus.Trigger(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_STARTED_EVENT, evt.Body), new NetworkSpawnerStartedEvent(null));
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
            //if (evt.Brain.LoggingEnabled)
            //    Debug.Log($"{nameof(HandleBrainSpawned)} {transform.name} {evt.Brain.Name}");
            if (!_brains.Contains(evt.Brain))
                _brains.Add(evt.Brain);

            if (evt.Brain.MonaTagSource != null && !_monaTags.Contains(evt.Brain.MonaTagSource))
                _monaTags.Add(evt.Brain.MonaTagSource);

            evt.Brain.SetMonaBrainPlayer(this);

        }

        public IMonaTagItem GetTag(string tag)
        {
            for(var i = 0;i < _monaTags.Count;i++)
            {
                if (_monaTags[i].HasTag(tag))
                    return _monaTags[i].GetTag(tag);
            }
            return null;
        }

        private void HandleBrainDestroyed(MonaBrainDestroyedEvent evt)
        {
            //if(evt.Brain.LoggingEnabled)
            //    Debug.Log($"{nameof(HandleBrainDestroyed)} {transform.name} {evt.Brain.Name}");
            if (_brains.Contains(evt.Brain))
                _brains.Remove(evt.Brain);

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

        private void SetupStorageSolutions()
        {
            bool localFound = false;
            bool cloudFound = false;
            var storageComponents = InterfaceFinder.FindComponentsWithInterface<IBrainStorage>();

            for (int i = 0; i < storageComponents.Length; i++)
            {
                if (localFound && cloudFound)
                    break;

                if (!localFound && storageComponents[i].SupportedStorageTarget != Utils.Enums.StorageTargetType.Cloud)
                    _localStorage = storageComponents[i];

                if (!cloudFound && storageComponents[i].SupportedStorageTarget != Utils.Enums.StorageTargetType.Local)
                    _cloudStorage = storageComponents[i];
            }

            if (!localFound && _localStorage == null)
                _localStorage = gameObject.AddComponent(typeof(BrainsDefaultStorage)) as BrainsDefaultStorage;
        }

        private void SetupSocialUser()
        {
            var socialComponents = InterfaceFinder.FindComponentsWithInterface<IBrainSocialPlatformUser>();

            if (socialComponents.Length > 0)
                _brainSocialUser = socialComponents[0];
        }

        private void SetupLeaderboards()
        {
            var leaderboardComponents = InterfaceFinder.FindComponentsWithInterface<IBrainLeaderboard>();

            if (leaderboardComponents.Length > 0)
                _brainLeaderboards = leaderboardComponents[0];
        }
    }
}