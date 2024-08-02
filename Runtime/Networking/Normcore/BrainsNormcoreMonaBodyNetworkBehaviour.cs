#if BRAINS_NORMCORE
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Network.Interfaces;
using Mona.SDK.Core.Input;
using Unity.VisualScripting;
using Mona.SDK.Core;
using Mona.SDK.Core.Network.Enums;
using Mona.SDK.Core.Events;
using Normal.Realtime;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Utils;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.Body.Enums;

namespace Mona.Networking
{

    public class BrainsNormcoreMonaBodyNetworkBehaviour : RealtimeComponent<MonaBodyNetworkModel>, INetworkMonaBodyClient, INetworkMonaBodyServer
    {
        public static List<INetworkMonaBodyClient> NetworkMonaBodies = new List<INetworkMonaBodyClient>();
        public static INetworkMonaBodyClient FindByLocalId(string localId) => NetworkMonaBodies.Find((x) => x.LocalId == localId);

        private Rigidbody _networkRigidbody;
        private MonaBody _monaBody;

        public MonaBody MonaBody => _monaBody;
        public Transform NetworkTransform => transform;
        public Rigidbody NetworkRigidbody => _networkRigidbody;
        public float DeltaTime => Time.deltaTime;

        public bool Initialized { get; set; }
        private List<Action> _rpcBackLog = new List<Action>();

        [SerializeField] private string _localId;
        [SerializeField] private string _prefabId;
        [SerializeField] private bool _locallyOwnedMonabody;

        public string LocalId => _localId;
        public string PrefabId => _prefabId;
        public bool LocallyOwnedMonaBody => _locallyOwnedMonabody;

        public bool Active => model.active;

        private bool _changedActive;
        private bool _changedScale;
        private bool _changedColor;
        private bool _changedLayer;

        private bool _playerSet;
        private bool _isPlayer;
        private string _playerName;
        private bool _audience;
        private int _playerId;
        private int _clientId;
        private bool _isRemote;

        private IMonaNetworkSettings _spaceNetworkSettings;
        private List<INetworkMonaVariables> _networkVariables = new List<INetworkMonaVariables>();

        private MonaInput _input = new MonaInput();

        private bool _hasInput;
        private bool _registerOnEnabled;

        public void SetSpaceNetworkSettings(IMonaNetworkSettings settings) => _spaceNetworkSettings = settings;

        private void UpdatePlayer(int playerId, int clientId, string name)
        {
            SyncPlayer();
        }

        public void SyncPlayer()
        {
            if (MonaBody == null) return;
            if (_playerId == model.player.playerID && _clientId == model.player.clientID && _playerName == model.player.name) return;

            var isRemote = model.player.clientID != this.realtime.clientID;
            Debug.Log($"{nameof(SyncPlayer)} SetPlayer {model.player.playerID} client: {model.player.clientID} name: {model.player.name} this: {this.realtime.clientID} isRemote? {isRemote} {gameObject.name}", gameObject);

            _playerId = model.player.playerID;
            _clientId = model.player.clientID;
            _playerName = model.player.name;
            _isRemote = isRemote;

            if (_isRemote)
                GetComponent<RealtimeTransform>().maintainOwnershipWhileSleeping = false;
            else
                GetComponent<RealtimeTransform>().maintainOwnershipWhileSleeping = true;

            if (_isRemote)
            {
                MonaBody.AttachType = MonaBodyAttachType.RemotePlayer;
                MonaBody.AddTag(MonaBrainConstants.TAG_REMOTE_PLAYER);
                MonaBody.RemoveTag(MonaBrainConstants.TAG_PLAYER);
            }
            else
            {
                MonaBody.AttachType = MonaBodyAttachType.LocalPlayer;
                MonaBody.RemoveTag(MonaBrainConstants.TAG_REMOTE_PLAYER);
                MonaBody.AddTag(MonaBrainConstants.TAG_PLAYER);
            }

            MonaBody.SetPlayer(_playerId, _clientId, _playerName, _audience, isNetworked: false);

            BrainsNormcoreNetworkManager.Instance.RegisterPlayerBody(MonaBody);
        }

        private void UpdateLocalId(string localId, string prefabId, bool locallyOwnedMonabody)
        {
            if (_localId == localId && _prefabId == prefabId && _locallyOwnedMonabody == locallyOwnedMonabody) return;

            //Debug.Log($"{nameof(BrainsNormcoreMonaBodyNetworkBehaviour)}.{nameof(UpdateLocalId)} new network object, find local copy of {_localId} prefab: {_prefabId}");

            _localId = localId;
            _prefabId = prefabId;
            _locallyOwnedMonabody = locallyOwnedMonabody;

            gameObject.name = _localId + ":NetworkBody";

            UpdateIdentifier();
        }

        private void UpdateIdentifier()
        {
            if (!Initialized)
            {
                Initialized = true;
                RegisterNetworkMonaBody();
            }
        }

        public void RegisterNetworkVariables(INetworkMonaVariables variables)
        {
            if (!_networkVariables.Contains(variables))
            {
                variables.OnStateAuthorityChanged += StateAuthorityChanged;
                _networkVariables.Add(variables);
            }
        }

        private void RegisterNetworkMonaBody()
        {
            StopCoroutine("WaitForNetworkSpawner");
            StartCoroutine("WaitForNetworkSpawner");
        }

        private IEnumerator WaitForNetworkSpawner()
        {
            float timeout = 0f;
            while (MonaGlobalBrainRunner.Instance.NetworkSpawner == null && timeout <= 30f)
            {
                timeout += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();

            if (MonaGlobalBrainRunner.Instance.NetworkSpawner != null && gameObject != null)
                MonaGlobalBrainRunner.Instance.NetworkSpawner.RegisterNetworkMonaBody(this);

                if (_monaBody != null)
                    model.active = _monaBody.GetActive();
        }

        private void UpdateLayer(string layer, bool includeChildren)
        {
            SyncLayer();
        }

        public void SyncLayer()
        {
            if (MonaBody == null) return;
            MonaBody.SetLayer(model.currentLayer.layer.ToString(), model.currentLayer.includeChildren, isNetworked: false);
        }

        public void SetPlayer(int playerId, int clientId, string name, bool audience)
        {
            Debug.Log($"{nameof(SetPlayer)} playerId {playerId} clientId {clientId} name {name} audience {audience}");
            _playerSet = true;
            model.player.SetPlayer(playerId, clientId, name, audience);
        }

        public void SetIdentifier(string localId, string prefabId, bool locallyOwnedMonaBody)
        {
            //Debug.Log($"{nameof(SetIdentifier)} localId {localId} prefabId {prefabId}, locallyOwnedMonaBody {locallyOwnedMonaBody}");
            model.identifier.SetIdentifier(localId, prefabId, locallyOwnedMonaBody);
        }

        public void SetSyncPositionAndRigidbody(bool sync)
        {
            GetComponent<RealtimeTransform>().enabled = sync;
        }

        private void UpdatePause(MonaBodyNetworkModel model, bool value)
        {
            SyncPause();
        }

        public void SyncPause()
        {
            if (MonaBody == null) return;
            if (model.paused)
                MonaBody.Pause(false);
            else
                MonaBody.Resume(false);
        }

        private void UpdateActive(MonaBodyNetworkModel model, bool value)
        {
            SyncActive();
        }

        public void SyncActive()
        {
            if (MonaBody == null) return;
            MonaBody.SetActive(model.active, isNetworked: false);
        }

        private void UpdateVisible(MonaBodyNetworkModel model, bool value)
        {
            SyncVisible();
        }

        public void SyncVisible()
        {
            if (MonaBody == null) return;
            MonaBody.SetVisible(model.visible, isNetworked: false);
        }

        private void UpdateScale(MonaBodyNetworkModel model, Vector3 value)
        {
            SyncScale();
        }

        public void SyncScale()
        {
            if (MonaBody == null) return;
            MonaBody.SetScale(model.scale, isNetworked: false);
        }

        private void UpdateColor(MonaBodyNetworkModel model, Color value)
        {
            SyncColor();
        }

        public void SyncColor()
        {
            if (MonaBody == null) return;
            MonaBody.SetColor(model.color, isNetworked: false);
        }

        protected override void OnRealtimeModelReplaced(MonaBodyNetworkModel previousModel, MonaBodyNetworkModel currentModel)
        {
            if (previousModel != null)
            {
                // Unregister from events
                previousModel.player.eventDidFire -= UpdatePlayer;
                previousModel.identifier.eventDidFire -= UpdateLocalId;
                previousModel.currentLayer.eventDidFire -= UpdateLayer;
                previousModel.activeDidChange -= UpdateActive;
                previousModel.colorDidChange -= UpdateColor;
                previousModel.pausedDidChange -= UpdatePause;
                previousModel.scaleDidChange -= UpdateScale;
                previousModel.visibleDidChange -= UpdateVisible;
            }

            if (currentModel != null)
            {


                // Register for events so we'll know if the color changes later
                currentModel.player.eventDidFire += UpdatePlayer;
                currentModel.identifier.eventDidFire += UpdateLocalId;
                currentModel.currentLayer.eventDidFire += UpdateLayer;
                currentModel.activeDidChange += UpdateActive;
                currentModel.colorDidChange += UpdateColor;
                currentModel.pausedDidChange += UpdatePause;
                currentModel.scaleDidChange += UpdateScale;
                currentModel.visibleDidChange += UpdateVisible;
            }
        }

        private void Awake()
        {
            if (!NetworkMonaBodies.Contains(this))
                NetworkMonaBodies.Add(this);

            _networkRigidbody = GetComponent<Rigidbody>();

            Spawned();
        }

        public string NetworkId()
        {
            return this.realtimeView.viewUUID;
        }

        public void Spawned()
        {
            //Debug.Log($"MONA OBJECT SPAWNED {gameObject.name}", this.gameObject);

            var monaBody = gameObject.GetComponent<MonaBody>();
            if (monaBody != null)
            {
                _isPlayer = true;
                //this is a player
                monaBody.MakeUnique(ownerIDSelf, true);
                SetIdentifier(monaBody.LocalId, null, true);
                SetMonaBody(monaBody);
            }

            ownerIDSelfDidChange += StateAuthorityChanged;
        }


        public void SetMonaBody(MonaBody monaBody)
        {
            if (monaBody == null)
            {
                _monaBody = null;
                return;
            }

            _monaBody = monaBody;
            
            //_monaBody.SetPlayer(_playerId, _clientId, _playerName, false);

            ParentMonaBody();

            ConfigureRigidbody();

            if (!HasControl()) return;

            model.active = _monaBody.GetActive();
            model.visible = _monaBody.GetVisible();
            model.scale = _monaBody.Transform.localScale;
            model.color = _monaBody.GetColor();
            model.currentLayer.SetLayer(LayerMask.LayerToName(_monaBody.GetLayer()), true);
            //Debug.Log($"{nameof(SetMonaBody)} active:{Active}, visible:{Visible}, scale:{Scale}, color:{Color}, layer: {CurrentLayer}", _monaBody.ActiveTransform.gameObject);

        }

        public void PlayerLeft(int player)
        {
            if (IsSceneObject() && ownerIDSelf != realtime.clientID && !LocallyOwnedMonaBody)
            {
                Debug.Log($"{nameof(PlayerLeft)} take control of scene object {LocalId}");
                TakeControl();
            }
        }

        public void SetLocalInput(MonaInput input)
        {
            if (_spaceNetworkSettings == null)
            {
                Debug.Log($"{nameof(SetLocalInput)} missing space network settings");
                return;
            }

            _input = input;
            _hasInput = true;
            /*
            if (HasControl())
            {
                var networkInput = new MonaNetworkInput();
                networkInput.SetInput(input);
                EventBus.Trigger<MonaBrainLocalInputEvent>(new EventHook(MonaCoreConstants.LOCAL_INPUT_EVENT), new MonaBrainLocalInputEvent(networkInput));
                
            }
            else
            {
                _monaBody.FixedUpdateNetwork(Time.fixedDeltaTime, true, input);
            }
            */
        }

        public void TriggerAnimation(string clipName)
        {
            SendAnimationTriggerRpc(LocalId, clipName);
        }

        private void ConfigureRigidbody()
        {
            if (_monaBody == null) return;

            /*if (_isPlayer)
            {
                _monaBody.RemoveRigidbody();
                return;
            }*/

            var monaBodyRigidBody = _monaBody.GetComponent<Rigidbody>();

            if ((_monaBody.SyncType == MonaBodyNetworkSyncType.NetworkRigidbody || monaBodyRigidBody != null))
            {
                if (_monaBody.gameObject != gameObject)
                {
                    var networkRigidbody = gameObject.GetComponent<Rigidbody>();
                    if (networkRigidbody == null)
                        networkRigidbody = gameObject.AddComponent<Rigidbody>();

                    if (monaBodyRigidBody != null && networkRigidbody != null)
                    {
                        networkRigidbody.useGravity = monaBodyRigidBody.useGravity;
                        networkRigidbody.isKinematic = monaBodyRigidBody.isKinematic;
                        networkRigidbody.mass = monaBodyRigidBody.mass;
                        networkRigidbody.drag = monaBodyRigidBody.drag;
                        networkRigidbody.angularDrag = monaBodyRigidBody.angularDrag;
                        networkRigidbody.constraints = monaBodyRigidBody.constraints;
                        //Debug.Log($"{nameof(MonaBodyNetworkBehaviour)}.{nameof(ConfigureRigidbody)} copy rigidbody isKinematic {networkRigidbody.isKinematic}");
                    }

                    _networkRigidbody = networkRigidbody;
                    DestroyImmediate(monaBodyRigidBody);
                }
            }
            else if (_monaBody.SyncType != MonaBodyNetworkSyncType.NetworkRigidbody)
            {
                DestroyImmediate(monaBodyRigidBody);
            }
        }

        private void ParentMonaBody()
        {
            if (_monaBody == null) return;
            if (_monaBody.gameObject == gameObject) return;

            transform.SetParent(_monaBody.transform.parent);
            transform.position = _monaBody.transform.position;
            transform.rotation = _monaBody.transform.rotation;
            transform.localScale = _monaBody.transform.localScale;
            _monaBody.transform.SetParent(transform);
            _monaBody.transform.localPosition = Vector3.zero;
            _monaBody.transform.localRotation = Quaternion.identity;
            _monaBody.transform.localScale = Vector3.one;
            _monaBody.SetActive(true);
        }


        private void OnDestroy()
        {
            StopCoroutine("WaitForNetworkSpawner");

            if (NetworkMonaBodies.Contains(this))
                NetworkMonaBodies.Remove(this);

            if (_monaBody != null)
            {
                //Debug.Log($"{nameof(MonaBodyNetworkBehavior)}.{nameof(OnDestroy)} unparent mona object {_monaBody}");
                _monaBody.SetNetworkMonaBody(null);
            }

            if (HasControl())
                realtimeView.ClearOwnership();

            for (var i = 0; i < _networkVariables.Count; i++)
            {
                if(_networkVariables[i] != null)
                    _networkVariables[i].OnStateAuthorityChanged -= StateAuthorityChanged;
            }

            ownerIDSelfDidChange -= StateAuthorityChanged;

            //Debug.Log($"{nameof(BrainsNormcoreMonaBodyNetworkBehaviour)}.{nameof(OnDestroy)} unparent mona object {_monaBody} client: {_clientId}");
            BrainsNormcoreNetworkManager.Instance.UnregisterPlayerBody(_clientId);
        }

        public void FixedUpdate()
        {
            if (!HasValidAuthority()) return;
            if (_spaceNetworkSettings == null) return;

            if (_spaceNetworkSettings.GetNetworkType() == MonaNetworkType.Shared)
            {
                if (_hasInput)
                    _monaBody?.FixedUpdateNetwork(Time.fixedDeltaTime, _hasInput, _input);
                else
                    _monaBody?.FixedUpdateNetwork(Time.fixedDeltaTime, false, (MonaInput)default);
            }

            _hasInput = false;
        }

        private bool HasValidAuthority() => !isUnownedInHierarchy;

        private bool IsSceneObject() => _monaBody == null ? false : _monaBody.IsSceneObject;

        public void StateAuthorityChanged(RealtimeComponent<MonaBodyNetworkModel> model, int ownerID)
        {
            if (isUnownedInHierarchy)
            {
                Debug.Log($"{nameof(StateAuthorityChanged)} orphaned {nameof(MonaBody)} {MonaBody} scene Object? {MonaBody.IsSceneObject}");
            }
            else
            {
                Debug.Log($"{nameof(StateAuthorityChanged)} {nameof(MonaBody)} {MonaBody} owned? {HasControl()} owner: {ownerID} me: {realtime.clientID}");
                if (HasControl())
                    ExecuteRpcBacklog();
            }
            StateAuthorityChanged();
        }

        public void StateAuthorityChanged()
        {
            var owned = HasControl();
            for(var i = 0;i < _networkVariables.Count; i++)
            {
                if (!_networkVariables[i].HasControl())
                    owned = false;
            }
            _monaBody?.StateAuthorityChanged(owned);
        }

        public void SetAnimator(Animator animator)
        {
            /*TODOvar network = GetComponent<NetworkMecanimAnimator>();
            if (network != null)
                network.Animator = animator;
            */
        }

        private void ExecuteRpcBacklog()
        {
            for (var i = _rpcBackLog.Count - 1; i >= 0; i--)
            {
                _rpcBackLog[i]?.Invoke();
                _rpcBackLog.RemoveAt(i);
            }
        }

        public void SetKinematic(bool b)
        {
            if (HasControl())
                SendKinematic(b);
        }

        public void SetPaused(bool paused)
        {
            if (HasControl())
                SendPaused(paused);
        }

        public void SetActive(bool active)
        {
            if (HasControl())
                SendActiveValue(active);
        }

        public void SetVisible(bool vis)
        {
            if (HasControl())
                SendVisibleValue(vis);
        }

        public bool HasControl() => _monaBody == null || _monaBody.SyncType == MonaBodyNetworkSyncType.NotNetworked || isOwnedLocallyInHierarchy;
        public void TakeControl()
        {
            if (!HasControl())
            {
                Debug.Log($"{nameof(TakeControl)} of {transform}", transform.gameObject);
                realtimeView.RequestOwnershipOfSelfAndChildren();
            }
        }

        public void ReleaseControl()
        {
            if (HasControl())
            {
                Debug.Log($"{nameof(ReleaseControl)} of {transform}");
                ClearOwnership();
            }
        }

        public void SetPosition(Vector3 position, bool isKinematic = false)
        {
            if (HasControl())
                SendPosition(position, isKinematic);
        }

        public void TeleportPosition(Vector3 position, bool isKinematic = false)
        {
            if (HasControl())
                SendTeleportPosition(position, isKinematic);
        }

        public void TeleportPosition(Vector3 position, bool isKinematic = false, bool setToLocal = false)
        {
            if (HasControl())
                SendTeleportPosition(position, isKinematic);
        }

        public void SetRotation(Quaternion rotation, bool isKinematic = false)
        {
            if (HasControl())
                SendRotation(rotation, isKinematic);
        }

        public void TeleportRotation(Quaternion rotation, bool isKinematic = false)
        {
            if (HasControl())
                SendTeleportRotation(rotation, isKinematic);
        }

        public void TeleportGlobalRotation(Quaternion rotation)
        {
            if (HasControl())
                SendTeleportGlobalRotation(rotation);
        }

        public void TeleportScale(Vector3 scale, bool isKinematic = false)
        {
            if (HasControl())
                SendScale(scale);
        }

        public void SetScale(Vector3 scale)
        {
            if (HasControl())
                SendScale(scale);
        }

        public void SetColor(Color color)
        {
            if (HasControl())
                SendColor(color);
        }

        public void SetLayer(string layerName, bool includeChildren = true)
        {
            if (HasControl())
                SendLayer(layerName, includeChildren);
        }

        public void SendKinematicRpc(bool b)
        {
            SendKinematic(b);
            //Debug.Log($"{nameof(SendKinematicRpc)} {b}");
        }

        private void SendKinematic(bool b)
        {
            _networkRigidbody.isKinematic = b;
            Debug.Log($"{nameof(SendKinematic)} isKinematic {_networkRigidbody.isKinematic}");
        }

        public void SendPositionRpc(Vector3 position, bool isKinematic = false)
        {
            SendPosition(position, isKinematic);
        }

        private void SendPosition(Vector3 position, bool isKinematic)
        {
            if (_networkRigidbody != null)
            {
                SendKinematic(isKinematic);
                if (_networkRigidbody.isKinematic)
                    _networkRigidbody.MovePosition(position);
                else
                    _networkRigidbody.position = position;
            }
            else
            {
                transform.position = position;
            }
            //Debug.Log($"{nameof(SendPositionRpc)} {position}");
        }

        public void SendTeleportPositionRpc(Vector3 position, bool isKinematic = false)
        {
            SendTeleportPosition(position, isKinematic);
        }

        private void SendTeleportPosition(Vector3 position, bool isKinematic)
        {
            //Debug.Log($"{nameof(SendTeleportPosition)} {position}", gameObject);
            if (transform.parent != null)
            {
                var pos = transform.parent.InverseTransformPoint(position);
                transform.localPosition = pos;
            }
            else
            {
                transform.localPosition = position;
            }
            /*
            transform.position = position;
            if (_networkRigidbody != null)
            {
                SendKinematic(isKinematic);
                _networkRigidbody.position = position;
            }*/
            //Debug.Log($"{nameof(SendPositionRpc)} {position}");
        }

        public void SendRotationRpc(Quaternion rotation, bool isKinematic = false)
        {
            SendRotation(rotation, isKinematic);
        }

        private void SendRotation(Quaternion rotation, bool isKinematic)
        {
            if (_networkRigidbody != null)
            {
                SendKinematic(isKinematic);
                if (_networkRigidbody.isKinematic)
                    _networkRigidbody.MoveRotation(rotation);
                else
                    _networkRigidbody.rotation = rotation;
            }
            else
                transform.rotation = rotation;
            //Debug.Log($"{nameof(SendRotationRpc)} {rotation}");
        }

        public void SendTeleportRotationRpc(Quaternion rotation, bool isKinematic = false)
        {
            SendTeleportRotation(rotation, isKinematic);
        }

        private void SendTeleportRotation(Quaternion rotation, bool isKinematic)
        {
            //Debug.Log($"{nameof(SendTeleportRotation)} {rotation}", gameObject);
            if (transform.parent != null)
            {
                var rot = Quaternion.Inverse(transform.parent.rotation) * rotation;
                transform.localRotation = rot;
            }
            else
            {
                transform.localRotation = rotation;
            }
            /*
            transform.rotation = rotation;
            if (_networkRigidbody != null)
            {
                SendKinematic(isKinematic);
                _networkRigidbody.rotation = rotation;
            }*/
            //Debug.Log($"{nameof(SendRotationRpc)} {rotation}");
        }

        public void SendTeleportGlobalRotationRpc(Quaternion rotation)
        {
            SendTeleportGlobalRotation(rotation);
        }

        private void SendTeleportGlobalRotation(Quaternion rotation)
        {
            Debug.Log($"{nameof(SendTeleportGlobalRotation)} {rotation}", gameObject);
            if (transform.parent != null)
            {
                var rot = Quaternion.Inverse(transform.parent.rotation) * rotation;
                transform.localRotation = rot;
            }
            else
            {
                transform.localRotation = rotation;
            }
            /*
            transform.rotation = rotation;
            if (_networkRigidbody != null)
            {
                _networkRigidbody.rotation = rotation;
            }*/
            //Debug.Log($"{nameof(SendRotationRpc)} {rotation}");
        }

        public void SendScaleRpc(Vector3 localScale)
        {
            SendScale(localScale);
        }

        private void SendScale(Vector3 localScale)
        {
            model.scale = localScale;
            //Debug.Log($"{nameof(SendScaleRpc)} {localScale}");
        }

        public void SendActiveValueRpc(bool b)
        {
            SendActiveValue(b);
        }

        private void SendActiveValue(bool b)
        {
            model.active = b;
            //Debug.Log($"{nameof(SendActiveValueRpc)} {b}");
        }

        public void SendVisibleValueRpc(bool b)
        {
            SendVisibleValue(b);
        }

        private void SendVisibleValue(bool b)
        {
            model.visible = b;
            //Debug.Log($"{nameof(SendActiveValueRpc)} {b}");
        }

        public void SendColorRpc(Color c)
        {
            SendColor(c);
        }

        private void SendColor(Color c)
        {
            model.color = c;
            //Debug.Log($"{nameof(SendColorRpc)} {c}");
        }

        public void SendLayerRpc(string layer, bool includeChildren = true)
        {
            SendLayer(layer, includeChildren);
        }

        private void SendLayer(string layer, bool includeChildren)
        {
            model.currentLayer.SetLayer(layer, includeChildren);
            //Debug.Log($"{nameof(SendLayerRpc)} {layer} {includeChildren}");
        }

        public void SendPausedRpc(bool paused)
        {
            SendPaused(paused);
        }

        private void SendPaused(bool paused)
        {
            model.paused = paused;
        }

        public void SendAnimationTriggerRpc(string localId, string clipName)
        {
            SendAnimationTrigger(localId, clipName);
        }

        private void SendAnimationTrigger(string localId, string clipName)
        {
            Debug.Log($"{nameof(SendAnimationTriggerRpc)} {localId}, {clipName}");
            var body = MonaBody.FindByLocalId(localId);
            if (body != null)
                MonaEventBus.Trigger<MonaBodyAnimationTriggeredEvent>(new EventHook(MonaCoreConstants.MONA_BODY_ANIMATION_TRIGGERED_EVENT, body), new MonaBodyAnimationTriggeredEvent(clipName));
        }

    }
}
#endif