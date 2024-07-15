#if BRAINS_NORMCORE
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.ThirdParty.Redcode.Awaiting;
using Mona.SDK.Core;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.Network.Interfaces;
using Mona.SDK.Core.State;
using Mona.SDK.Core.Utils;
using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.Networking
{
    public class BrainNormcoreMonaNetworkSpawner : RealtimeComponent<BrainNormcoreMonaNetworkSpawnerModel>, IMonaNetworkSpawner, IMonaPlayerLeft
    {
        public GameObject MonaBodyPrefab;
        public GameObject MonaVariablesPrefab;
        
        private List<IMonaAssetProvider> _providers = new List<IMonaAssetProvider>();
        private IMonaNetworkSettings _spaceNetworkSettings;

        public int LocalPlayerId => realtime.clientID;

        private bool _spawned;

        private Action<MonaBodyInstantiatedEvent> OnMonaBodyInstantiated;
        private Action<MonaAssetProviderAddedEvent> OnMonaAssetProviderAdded;
        private Action<MonaAssetProviderRemovedEvent> OnMonaAssetProviderRemoved;


        public void SetSpaceNetworkSettings(IMonaNetworkSettings settings)
        {
            _spaceNetworkSettings = settings;
        }

        public void RegisterMonaPrefabProvider(IMonaAssetProvider provider)
        {
            if (!_providers.Contains(provider))
                _providers.Add(provider);
        }

        private void Awake()
        {
            MonaGlobalBrainRunner.Instance.NetworkSpawner = this;

            OnMonaBodyInstantiated = HandleMonaBodyInstantiated;
            MonaEventBus.Register<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), OnMonaBodyInstantiated);

            OnMonaAssetProviderAdded = HandleAssetProviderAdded;
            MonaEventBus.Register<MonaAssetProviderAddedEvent>(new EventHook(MonaCoreConstants.MONA_ASSET_PROVIDER_ADDED), OnMonaAssetProviderAdded);

            OnMonaAssetProviderRemoved = HandleAssetProviderRemoved;
            MonaEventBus.Register<MonaAssetProviderRemovedEvent>(new EventHook(MonaCoreConstants.MONA_ASSET_PROVIDER_REMOVED), OnMonaAssetProviderRemoved);
        }


        private void Start()
        {
            if (realtime.connected)
            {
                Spawned(realtime);
            }
            else
            {
                realtime.didConnectToRoom += Spawned;
            }
        }

        protected override void OnRealtimeModelReplaced(BrainNormcoreMonaNetworkSpawnerModel previousModel, BrainNormcoreMonaNetworkSpawnerModel currentModel)
        {
            //Debug.Log($"{nameof(MonaNetworkSpawner)} {nameof(OnRealtimeModelReplaced)} {_spaceNetworkSettings != null} {_spawned} realtime? {realtime != null} {realtimeView != null} connected? {realtime.connected}");
        }


        private void HandleMonaBodyInstantiated(MonaBodyInstantiatedEvent evt)
        {
            MonaEventBus.Trigger(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_STARTED_EVENT, evt.Body), new NetworkSpawnerStartedEvent(this));
        }

        private void HandleAssetProviderAdded(MonaAssetProviderAddedEvent evt)
        {
            if (!_providers.Contains(evt.AssetProvider))
            {
                _providers.Add(evt.AssetProvider);
                Debug.Log($"{nameof(HandleAssetProviderAdded)} {evt.AssetProvider}");
            }
        }

        private void HandleAssetProviderRemoved(MonaAssetProviderRemovedEvent evt)
        {
            if (_providers.Contains(evt.AssetProvider))
            {
                _providers.Remove(evt.AssetProvider);
                Debug.Log($"{nameof(HandleAssetProviderRemoved)} {evt.AssetProvider}");
            }
        }

        public void Spawned(Realtime realtime)
        {
            _spawned = true;
            Debug.Log($"{nameof(MonaNetworkSpawner)} {nameof(Spawned)} {_spaceNetworkSettings != null} spawned {_spawned}");

            MonaEventBus.Trigger<NetworkSpawnerInitializedEvent>(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_INITIALIZED_EVENT), new NetworkSpawnerInitializedEvent(this));
            MonaEventBus.Trigger<NetworkSpawnerStartedEvent>(new EventHook(MonaCoreConstants.NETWORK_SPAWNER_STARTED_EVENT), new NetworkSpawnerStartedEvent(this));
        }

        private void OnEnable()
        {
        }

        private void OnDestroy()
        {
            Debug.Log($"{nameof(OnDestroy)}");
            MonaGlobalBrainRunner.Instance.NetworkSpawner = null;
            if (isOwnedLocallyInHierarchy)
                ClearOwnership();

            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), OnMonaBodyInstantiated);
        }

        public void PlayerLeft(int player)
        {

            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(PlayerLeft)} playerid {player} who has authority {ownerIDSelf}");
            var authPlayerId = ownerIDSelf;
            if ((authPlayerId == player) && authPlayerId != realtime.clientID)
            {
                Debug.Log($"{nameof(PlayerLeft)} take control of NetworkSpawner");
                realtimeView.RequestOwnershipOfSelfAndChildren();
            }
        }

        public void RegisterMonaReactor(IMonaReactor monaReactor)
        {
        }

        public void RegisterMonaBody(IMonaBody monaBody)
        {
            if (monaBody == null) return;
            WaitForSpawn((MonaBody)monaBody);
        }

        private async void WaitForSpawn(MonaBody monaBody)
        {
            while (string.IsNullOrEmpty(monaBody.LocalId))
            {
                Debug.Log($"Waiting for network spawner to spawn for monaBody {monaBody.name} {monaBody.LocalId}");
                await new WaitForSeconds(.1f);
            }

            while (!monaBody.Started)
                await new WaitForSeconds(.1f);

            Debug.Log($"{nameof(MonaNetworkSpawner)} spawning {monaBody.name}");
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(RegisterMonaBody)} {monaBody.name} {monaBody.LocalId}");
            ReconcileNetworkMonaBody(monaBody);
        }

        public void RegisterNetworkMonaReactor(INetworkMonaReactorClient networkReactor)
        {
        }

        public void RegisterNetworkMonaBody(INetworkMonaBodyClient networkMonaBody)
        {
            if (networkMonaBody == null) return;
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(RegisterNetworkMonaBody)} {networkMonaBody.LocalId}");
            ReconcileMonaBody(networkMonaBody);
        }

        public void RegisterNetworkMonaVariables(INetworkMonaVariables networkMonaVariables)
        {
            if (networkMonaVariables == null) return;
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(RegisterNetworkMonaVariables)} {networkMonaVariables.LocalId}");
            WaitForSpawn(networkMonaVariables);
        }

        private async void WaitForSpawn(INetworkMonaVariables networkMonaVariables)
        {
            var networkMonaBody = BrainsNormcoreMonaBodyNetworkBehaviour.FindByLocalId(networkMonaVariables.LocalId);
            var monaBody = MonaBody.FindByLocalId(networkMonaVariables.LocalId);
            while (networkMonaBody == null || monaBody == null)
            {
                Debug.Log($"Waiting for network object to spawn for mona Variables {networkMonaVariables.LocalId} {networkMonaVariables.Index}");
                await new WaitForSeconds(.1f);
                networkMonaBody = BrainsNormcoreMonaBodyNetworkBehaviour.FindByLocalId(networkMonaVariables.LocalId);
                monaBody = MonaBody.FindByLocalId(networkMonaVariables.LocalId); 
            }

            while (!monaBody.Started)
                await new WaitForSeconds(.1f);

            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(RegisterNetworkMonaVariables)} found mona body: {networkMonaBody.LocalId}");
            ReconcileNetworkMonaVariables(networkMonaBody, networkMonaVariables);
        }

        private void HandleMonaBodyDestroyed(INetworkMonaBodyServer networkMonaBody)
        {
            if (networkMonaBody == null) return;
            var client = (INetworkMonaBodyClient)networkMonaBody;
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(HandleMonaBodyDestroyed)} remaining: {BrainsNormcoreMonaBodyNetworkBehaviour.NetworkMonaBodies.Count}");
        }

        private void ReconcileNetworkMonaBody(MonaBody monaBody)
        {
            if (monaBody == null) return;
            var localId = monaBody.LocalId;
            var networkMonaBody = BrainsNormcoreMonaBodyNetworkBehaviour.FindByLocalId(localId);
            if (networkMonaBody != null)
            {
                BindNetworkAndMonaBody(networkMonaBody, monaBody);
            }
            else if (monaBody.LocallyOwnedMonaBody)
            {
                SpawnNetworkMonaBody(localId,
                       monaBody.IsSceneObject,
                       monaBody.LocallyOwnedMonaBody,
                       monaBody.PrefabId,
                       monaBody.SyncType,
                       monaBody.SyncPositionAndRotation,
                       monaBody.transform.position,
                       monaBody.transform.rotation,
                       monaBody.transform.localScale,
                       monaBody.GetComponent<IMonaBrainRunner>() != null ? monaBody.GetComponent<IMonaBrainRunner>().BrainGraphs.Count : 0,
                       monaBody.name,
                       monaBody.PlayerSet,
                       monaBody.PlayerId,
                       monaBody.ClientId,
                       monaBody.PlayerName);
            }
            else
            {
                SpawnNetworkMonaBodyRPC(localId,
                    monaBody.IsSceneObject,
                    monaBody.LocallyOwnedMonaBody,
                    monaBody.PrefabId,
                    monaBody.SyncType,
                    monaBody.SyncPositionAndRotation,
                    monaBody.transform.position,
                    monaBody.transform.rotation,
                    monaBody.transform.localScale,
                    monaBody.GetComponent<IMonaBrainRunner>() != null ? monaBody.GetComponent<IMonaBrainRunner>().BrainGraphs.Count : 0,
                    monaBody.name,
                    monaBody.PlayerSet,
                    monaBody.PlayerId,
                    monaBody.ClientId,
                    monaBody.PlayerName);
            }
        }

        private void ReconcileMonaBody(INetworkMonaBodyClient networkMonaBody)
        {
            if (networkMonaBody == null) return;
            var monaBody = MonaBody.FindByLocalId(networkMonaBody.LocalId);
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(ReconcileMonaBody)} {networkMonaBody.LocalId} instance {monaBody}");
            if (monaBody != null)
            {
                BindNetworkAndMonaBody(networkMonaBody, (MonaBody)monaBody);
            }
            else if (!string.IsNullOrEmpty(networkMonaBody.PrefabId))
            {
                SpawnLocalMonaBody(networkMonaBody);
            }
        }

        private void ReconcileNetworkMonaVariables(INetworkMonaBodyClient networkMonaBody, INetworkMonaVariables networkVariables)
        {
            if (networkMonaBody == null) return;
            var monaBody = MonaBody.FindByLocalId(networkMonaBody.LocalId);
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(ReconcileNetworkMonaVariables)} {networkMonaBody.LocalId} {networkVariables.Index} instance {monaBody}");
            if (monaBody != null)
            {
                ((INetworkMonaBodyServer)networkMonaBody).RegisterNetworkVariables(networkVariables);
                BindNetworkAndMonaVariables(networkVariables, (MonaBody)monaBody);
            }
        }

        public void SpawnLocalMonaBody(INetworkMonaBodyClient networkMonaBody)
        {
            if (networkMonaBody == null) return;
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(SpawnLocalMonaBody)} {networkMonaBody.PrefabId} localPlayer: {realtime.clientID} {networkMonaBody.HasControl()} Providers: {_providers.Count}");
            var found = true;
            if (_providers == null || _providers.Count == 0) found = false;

            if (_providers != null)
            {
                for (var i = 0; i < _providers.Count; i++)
                {
                    var provider = _providers[i];
                    var prefab = provider.GetMonaAsset<IMonaBodyAssetItem>((x) => x.PrefabId == networkMonaBody.PrefabId || (x.Value.GetComponent<MonaBodyBase>()?.PrefabId == networkMonaBody.PrefabId));
                    if (prefab != null)
                    {
                        var monaBody = GameObject.Instantiate<MonaBody>(prefab.Value, networkMonaBody.NetworkTransform.position, networkMonaBody.NetworkTransform.rotation);
                        monaBody.IsSceneObject = false;
                        monaBody.SetDisableOnLoad(false);// ((INetworkMonaBodyServer)networkMonaBody).Active ? false : true;
                        monaBody.transform.localScale = networkMonaBody.NetworkTransform.localScale;
                        monaBody.LocalId = networkMonaBody.LocalId;
                        BindNetworkAndMonaBody(networkMonaBody, monaBody);
                        found = true;
                        break;
                    }
                }
            }

            if(!found)
            {
                Debug.Log($"{nameof(SpawnLocalMonaBody)} cannot find {networkMonaBody.PrefabId} searching Resources");
                var prefab = Resources.Load<MonaBody>(networkMonaBody.PrefabId);
                if (prefab != null)
                {
                    var monaBody = GameObject.Instantiate<MonaBody>(prefab, networkMonaBody.NetworkTransform.position, networkMonaBody.NetworkTransform.rotation);
                    monaBody.IsSceneObject = false;
                    monaBody.SetDisableOnLoad(false);// ((INetworkMonaBodyServer)networkMonaBody).Active ? false : true;
                    monaBody.transform.localScale = networkMonaBody.NetworkTransform.localScale;
                    monaBody.LocalId = networkMonaBody.LocalId;
                    BindNetworkAndMonaBody(networkMonaBody, monaBody);
                    found = true;
                }
            }

        }

        public void SpawnLocalMonaReactor(INetworkMonaReactorClient reactor)
        {

        }

        private void BindNetworkAndMonaBody(INetworkMonaBodyClient networkMonaBody, MonaBody monaBody)
        {
            var server = ((INetworkMonaBodyServer)networkMonaBody);
            if (server == null || monaBody == null) return;

            server.SetSpaceNetworkSettings(_spaceNetworkSettings);
            server.SetMonaBody(monaBody);

            monaBody.SetNetworkMonaBody(networkMonaBody);

            networkMonaBody.SetSyncPositionAndRigidbody(monaBody);
            networkMonaBody.NetworkTransform.name = monaBody.LocalId + ":NetworkBody";
            /*
            var states = networkMonaBody.NetworkTransform.GetComponents<BrainsNormcoreMonaVariablesNetworkBehaviour>();
            for (var i = 0; i < states.Length; i++)
            {
                var state = states[i];
                if (state != null)
                    state.SetMonaBody(monaBody, i);
            }*/
        }

        private void BindNetworkAndMonaVariables(INetworkMonaVariables networkMonaVariables, MonaBody monaBody)
        {
            networkMonaVariables.SetMonaBody(monaBody, networkMonaVariables.Index);
        }

        public void SpawnNetworkMonaReactorRPC(string localId, bool isSceneObject, bool locallyOwnedMonaBody, string prefabId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
        
        }

        public void SpawnNetworkMonaBodyRPC(string localId, bool isSceneObject, bool locallyOwnedMonaBody, string prefabId, MonaBodyNetworkSyncType syncType, bool syncPositionAndRotation, Vector3 position, Quaternion rotation, Vector3 scale, int variablesCount = 0, string name = null, bool playerSet = false, int playerId = 0, int clientId = 0, string playerName = null)
        {
            SpawnNetworkMonaBody(localId, isSceneObject, locallyOwnedMonaBody, prefabId, syncType, syncPositionAndRotation, position, rotation, scale, variablesCount, name, playerSet, playerId, clientId, playerName);
        }

        public void SpawnNetworkMonaBody(string localId, bool isSceneObject, bool locallyOwnedMonaBody, string prefabId, MonaBodyNetworkSyncType syncType, bool syncPositionAndRotation, Vector3 position, Quaternion rotation, Vector3 scale, int variablesCount = 0, string name = null, bool playerSet = false, int playerId = 0, int clientId = 0, string playerName = null)
        {
            Debug.Log($"{nameof(MonaNetworkSpawner)}.{nameof(SpawnNetworkMonaBodyRPC)} {localId} {prefabId} me: {realtime.clientID}");
            var existingNetworkMonaBody = BrainsNormcoreMonaBodyNetworkBehaviour.FindByLocalId(localId);
            if (existingNetworkMonaBody == null)
            {
                GameObject networkObject;
                var options = new Realtime.InstantiateOptions
                {
                    ownedByClient = true,    // Make sure the RealtimeView on this prefab is owned by this client.
                    preventOwnershipTakeover = locallyOwnedMonaBody,    // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                    destroyWhenOwnerLeaves = locallyOwnedMonaBody,
                    destroyWhenLastClientLeaves = true,
                    
                    useInstance = realtime // Use the instance of Realtime that fired the didConnectToRoom event.
                };

                networkObject = Realtime.Instantiate(MonaBodyPrefab.name, position, rotation, options);

                if(isSceneObject)
                    networkObject.GetComponent<INetworkMonaBodyClient>().TakeControl();

                networkObject.gameObject.name = localId + ":NetworkBody";

                if (variablesCount > 0)
                {
                    for (var i = 0; i < variablesCount; i++)
                    {
                        var variablesObject = Realtime.Instantiate(MonaVariablesPrefab.name, Vector3.zero, Quaternion.identity, options);
                        variablesObject.gameObject.name = localId + ":NetworkVariables";

                        var networkVariables = variablesObject.GetComponent<INetworkMonaVariables>();
                        networkVariables.SetIdentifier(localId, i, isSceneObject ? null : prefabId, locallyOwnedMonaBody);

                    }
                }

                Debug.Log($"{nameof(SpawnNetworkMonaBody)} {localId} {isSceneObject} {playerId} {clientId} {playerName}");


                networkObject.transform.localScale = scale;
                networkObject.GetComponent<INetworkMonaBodyClient>().SetIdentifier(localId, isSceneObject ? null : prefabId, locallyOwnedMonaBody);
                networkObject.GetComponent<INetworkMonaBodyClient>().SetSyncPositionAndRigidbody(syncPositionAndRotation);

                if(playerSet)
                    networkObject.GetComponent<INetworkMonaBodyClient>().SetPlayer(playerId, clientId, playerName);
                //if (networkObject.GetComponent<INetworkMonaVariables>() != null)
                //    networkObject.GetComponent<INetworkMonaVariables>().SetIdentifier(localId, 0, isSceneObject ? null : prefabId, locallyOwnedMonaBody);
            }
        }
    }
}
#endif