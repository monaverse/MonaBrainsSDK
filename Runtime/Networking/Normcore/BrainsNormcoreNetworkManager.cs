#if BRAINS_NORMCORE
using System;
using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network.Interfaces;
using Mona.SDK.Core.Utils;
using Normal.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Mona.SDK.Brains.ThirdParty.Redcode.Awaiting;
using Normal.Realtime.Serialization;

namespace Mona.Networking
{
    public class BrainsNormcoreNetworkManager : RealtimeComponent<BrainsRoomModel>
    {
        public Action<Room> OnRoomJoined = delegate { };

        private Realtime _realtime;

        public GameObject _networkSpawner;
        public GameObject _avatarLocalPrefab;
        public GameObject _avatarRemotePrefab;
        public GameObject[] _spawnPoints;
        public TMPro.TMP_InputField _roomInput;
        public bool _joinOnStart;
        public Button _joinButton;
        public Button _startButton;
        public Button _carButton;
        public Button _ballButton;
        public Image _mainPanel;
        public Image _bigLogo;
        public Image _smallLogo;
        public Camera _titleCamera;
        public GameObject _panel;
        public MonaBody _statusPanel;
        public TMPro.TextMeshProUGUI _statusText;

        private Action<NetworkSpawnerInitializedEvent> OnNetworkSpawnerInitialized;

        public static BrainsNormcoreNetworkManager Instance;

        public int PlayerCount => model.players.Count;

        private string _playerName;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            _realtime = GetComponent<Realtime>();
            _realtime.didConnectToRoom += DidConnectToRoom;
            _realtime.didDisconnectFromRoom += DidDisconnectFromRoom;

            OnNetworkSpawnerInitialized = HandleNetworkSpawnerInitialized;
            MonaEventBus.Register(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_INITIALIZED_EVENT), OnNetworkSpawnerInitialized);
        }

        private void Start()
        {
            if (_joinOnStart)
            {
                _realtime.Connect(_realtime.roomToJoinOnStart);
            }
            ShowRoom();
        }

        public void Join()
        {
            UpdateStatus("Connecting...");
            HideRoomConnect();

            _ready = false;

            if (!string.IsNullOrEmpty(_roomInput.text))
                _realtime.Connect(_roomInput.text);
            else
                _realtime.Connect(_realtime.roomToJoinOnStart);
        }

        private void DidConnectToRoom(Realtime realtime)
        {
            Debug.Log($"{nameof(DidConnectToRoom)}");

            // Update the mesh render to match the new model
            //if (isUnownedInHierarchy)
            //    realtimeView.RequestOwnershipOfSelfAndChildren();
            OnRoomJoined?.Invoke(realtime.room);

            if (isUnownedInHierarchy)
            {
                RequestOwnership();
            }
            else
            {
                ClaimClient();
            }

            ShowStart();
        }

        private void DidDisconnectFromRoom(Realtime realtime)
        {
            ShowRoom();
        }

        public bool _ready;
        protected override void OnRealtimeModelReplaced(BrainsRoomModel previousModel, BrainsRoomModel currentModel)
        {
            Debug.Log($"{nameof(OnRealtimeModelReplaced)} {previousModel != null} {currentModel != null}");
            if (previousModel != null)
            {
                // Unregister from events
                previousModel.ownerIDSelfDidChange -= HandleOwnerIDChanged;
                previousModel.PLAYER_IDDidChange -= HandlePlayerIDChanged;
                previousModel.players.modelAdded -= HandlePlayerAdded;
                previousModel.players.modelRemoved -= HandlePlayerRemoved;
            }

            if (currentModel != null)
            {
                // If this is a model that has no data set on it, populate it with the current mesh renderer color.
                if (currentModel.isFreshModel)
                {
                    Debug.Log($"fresh model");
                }

                if(_realtime.connected)
                    Debug.Log($"player count {currentModel.players.Count} PLAYERID {currentModel.PLAYER_ID}");

                // Register for events so we'll know if the color changes later
                currentModel.ownerIDSelfDidChange += HandleOwnerIDChanged;
                currentModel.PLAYER_IDDidChange += HandlePlayerIDChanged;
                currentModel.players.modelAdded += HandlePlayerAdded;
                currentModel.players.modelRemoved += HandlePlayerRemoved;
            }
        }

        private void HandlePlayerAdded(RealtimeSet<BrainsPlayerModel> set, BrainsPlayerModel model, bool remote)
        {
            if (model.clientID == realtime.clientID)
            {
                model.nameDidChange += HandlePlayerNameChanged;

                var position = _spawnPoints[model.playerID % _spawnPoints.Length].transform.position;
                var rotation = _spawnPoints[model.playerID % _spawnPoints.Length].transform.rotation;

                Debug.Log($"{nameof(HandlePlayerAdded)} Teleport: {model.playerID} {model.name} {position} {rotation}");

                MonaGlobalBrainRunner.Instance.PlayerBody.SetPlayer(model.playerID, model.clientID, model.name, true);
                MonaGlobalBrainRunner.Instance.PlayerBody.SetInitialTransforms(position, rotation);
                MonaGlobalBrainRunner.Instance.PlayerBody.TeleportPosition(position);
                MonaGlobalBrainRunner.Instance.PlayerBody.TeleportRotation(rotation);

            }
        }

        private void HandlePlayerRemoved(RealtimeSet<BrainsPlayerModel> set, BrainsPlayerModel model, bool remote)
        {
            model.nameDidChange -= HandlePlayerNameChanged;

            Debug.Log($"{nameof(HandlePlayerRemoved)} Teleport: {model.playerID} {model.name}");

        }

        private void HandlePlayerNameChanged(BrainsPlayerModel model, string name)
        {
            Debug.Log($"Player {model.playerID} changed their name to {name}");
        }

        private void HandleOwnerIDChanged(RealtimeModel model, int id)
        {
            Debug.Log($"{nameof(HandleOwnerIDChanged)} owner: {id} client: {_realtime.clientID}");
            if (model.ownerIDSelf == _realtime.clientID)
                ClaimHost();
        }

        private void HandlePlayerIDChanged(RealtimeModel model, int value)
        {
            Debug.Log($"Room PLAYER_ID changed {((BrainsRoomModel)model).PLAYER_ID}");
        }

        private void ClaimHost()
        {
            Debug.Log($"{nameof(ClaimHost)}");
            MakeMonaBodiesUnique();
            CreateNetworkSpawner();
            SpawnAvatar(true);
        }

        private void ClaimClient()
        {
            Debug.Log($"{nameof(ClaimClient)}");
            MakeMonaBodiesUnique();
            SpawnAvatar();
        }

        private void HideRoomConnect()
        {
            _roomInput.gameObject.SetActive(false);
            _joinButton.gameObject.SetActive(false);
        }

        private void ShowRoom()
        {
            _statusPanel?.gameObject.SetActive(false);
            _roomInput.gameObject.SetActive(true);
            _joinButton.gameObject.SetActive(true);
            _startButton.gameObject.SetActive(false);
            _titleCamera.gameObject.SetActive(true);
            _carButton.gameObject.SetActive(false);
            _ballButton.gameObject.SetActive(false);
            _panel.gameObject.SetActive(true);
            _smallLogo.gameObject.SetActive(false);
            _bigLogo.gameObject.SetActive(true);
            _mainPanel.enabled = true;
        }

        public void ShowStart()
        {
            _statusPanel?.gameObject.SetActive(false);
            _joinButton.gameObject.SetActive(false);
            _roomInput.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(true);
            _carButton.gameObject.SetActive(true);
            _ballButton.gameObject.SetActive(true);
            _titleCamera.gameObject.SetActive(false);
            _smallLogo.gameObject.SetActive(true);
            _bigLogo.gameObject.SetActive(false);
            _mainPanel.enabled = false;
        }

        public void HideStart()
        {
            _panel.gameObject.SetActive(false);
            _startButton.gameObject.SetActive(false);
            _carButton.gameObject.SetActive(false);
            _ballButton.gameObject.SetActive(false);
            _smallLogo.gameObject.SetActive(false);
            _bigLogo.gameObject.SetActive(false);
            _mainPanel.enabled = false;
        }

        private void UpdateStatus(string text)
        {
            _statusPanel?.gameObject.SetActive(true);
            _statusText.text = text;
        }

        private bool _spawned;

        private void SpawnAvatar(bool isHost = false)
        {
            if (_spawned) return;
            _spawned = true;

            var player = AddPlayer();
            
            if (_avatarLocalPrefab != null)
            {
                var position = Vector3.up * 10f;
                var avatar = GameObject.Instantiate(_avatarLocalPrefab, position, Quaternion.identity);
                avatar.GetComponent<MonaBodyBase>().MakeUnique(player.clientID, true);
                avatar.GetComponent<MonaBodyBase>().PrefabId = _avatarRemotePrefab.name;
                avatar.GetComponent<IMonaBody>().SetPlayer(player.clientID, player.clientID, player.name);

                MonaGlobalBrainRunner.Instance.PlayerCamera = avatar.GetComponentInChildren<Camera>();
                MonaGlobalBrainRunner.Instance.PlayerBody = avatar.GetComponent<IMonaBody>();
            }

            MonaGlobalBrainRunner.Instance.PlayerId = player.clientID;
            MonaGlobalBrainRunner.Instance.IsHost = isHost;

        }

        private BrainsPlayerModel AddPlayer()
        {
            Debug.Log($"{nameof(AddPlayer)} {model.PLAYER_ID}");

            var player = new BrainsPlayerModel();
            player.clientID = realtime.clientID;
            player.name = $"Client {player.clientID}";

            return player;
        }

        public void RegisterPlayerBody(IMonaBody body)
        {
            if(!isOwnedLocallyInHierarchy)
            {
                Debug.Log($"{nameof(RegisterPlayerBody)} {body.NetworkBody.LocalId} not on host");
                return;
            }

            var clientID = body.ClientId;
            var found = false;

            foreach (var player in model.players)
            {
                if(player.clientID == clientID)
                {
                    Debug.Log($"{nameof(RegisterPlayerBody)} update player name {body.PlayerName}");
                    player.name = body.PlayerName;
                    found = true;
                }
            }

            if(!found)
            {
                var player = new BrainsPlayerModel();
                player.clientID = body.ClientId;
                player.playerID = model.PLAYER_ID;
                player.name = !string.IsNullOrEmpty(_playerName) ? _playerName : $"Player {player.playerID}";

                Debug.Log($"{nameof(RegisterPlayerBody)} update player id {player.playerID} {player.name} {player.clientID}");

                model.PLAYER_ID += 1;
                model.players.Add(player);
            }

        }

        public void UnregisterPlayerBody(int clientID)
        {
            if (!isOwnedLocallyInHierarchy)
            {
                Debug.Log($"{nameof(UnregisterPlayerBody)} {clientID} not on host");
                return;
            }

            BrainsPlayerModel playerLeft = null;
            foreach (var player in model.players)
            {
                if (player.clientID == clientID)
                {
                    playerLeft = player;
                }
            }

            if(playerLeft != null)
            {
                model.players.Remove(playerLeft);
                model.PLAYER_ID = model.players.Count;
                Debug.Log($"{nameof(UnregisterPlayerBody)} {playerLeft.name} {playerLeft.clientID} player left");
            }
        }

        public void SetPlayerName(string name)
        {
            _playerName = name;
            foreach (var player in model.players)
            {
                if (player.clientID == realtime.clientID)
                {
                    player.name = name;
                    var body = MonaGlobalBrainRunner.Instance.PlayerBody;
                    body.SetPlayer(body.PlayerId, body.ClientId, name, true);
                    return;
                }
            }
        }

        private void CreateNetworkSpawner()
        {
            var options = new Realtime.InstantiateOptions
            {
                ownedByClient = true,    // Make sure the RealtimeView on this prefab is owned by this client.
                preventOwnershipTakeover = false,    // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                destroyWhenOwnerLeaves = false,
                destroyWhenLastClientLeaves = true,
                useInstance = realtime // Use the instance of Realtime that fired the didConnectToRoom event.
            };
            var networkSpawner = Realtime.Instantiate(_networkSpawner.name, Vector3.zero, Quaternion.identity, options);
        }

        private void HandleNetworkSpawnerInitialized(NetworkSpawnerInitializedEvent evt)
        {
            evt.NetworkSpawner.SetSpaceNetworkSettings(MonaGlobalBrainRunner.Instance.NetworkSettings);
        }

        private void MakeMonaBodiesUnique()
        {
            /* This must be blocking to ensure that all duplicate GUIDs are uniquely identified

            //Attempt to calculate a deterministic GUID for scene objects that have the same GUID.
            //Scenes with Reactors before this update will have an empty GUID.
            //monavox-editor has over 22000 reactors and they need to be durably identified otherwise networking does not sync correctly.
            //old spaces should be migrated to the new guid convention

            */
            var uniques = new List<MonaBodyBase>(GameObject.FindObjectsOfType<MonaBodyBase>(true));
            uniques = uniques.FindAll(x => x.IsSceneObject);
            uniques.Sort((a, b) =>
            {
                var compare = a.PathName.CompareTo(b.PathName);
                if (compare == 0) return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
                else return compare;
            });

            Debug.Log($"{nameof(MakeMonaBodiesUnique)} found and sorted monabodies: {uniques.Count}");

            var uniquesIndex = new Dictionary<MonaBodyBase, int>();
            var tempIndex = new Dictionary<SerializableGuid, int>();
            for (var i = 0; i < uniques.Count; i++)
            {
                var unique = uniques[i];
                if (!tempIndex.ContainsKey(unique.guid))
                    tempIndex.Add(unique.guid, 0);
                uniquesIndex[unique] = tempIndex[unique.guid];
                tempIndex[unique.guid]++;
            }

            Debug.Log($"{nameof(MakeMonaBodiesUnique)} indexed monabodies: {uniques.Count}");

            for (var i = 0; i < uniques.Count; i++)
                uniques[i].CalculateUniqueId(uniquesIndex);

            Debug.Log($"{nameof(MakeMonaBodiesUnique)} made all monabodies unique.");

        }

    }

}
#endif