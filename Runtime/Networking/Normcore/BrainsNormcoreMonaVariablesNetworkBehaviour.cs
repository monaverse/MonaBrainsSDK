#if BRAINS_NORMCORE
using UnityEngine;
using System;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State;
using Mona.SDK.Core.Network;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Network.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Normal.Realtime;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Normal.Realtime.Serialization;

namespace Mona.Networking
{
    public class BrainsNormcoreMonaVariablesNetworkBehaviour : RealtimeComponent<MonaVariablesNetworkModel>, INetworkMonaVariables
    {
        public event Action OnStateAuthorityChanged = delegate { };

        public static List<INetworkMonaVariables> NetworkMonaVariables = new List<INetworkMonaVariables>();
        public static INetworkMonaVariables FindByLocalId(string localId, int index) => NetworkMonaVariables.Find((x) => x.LocalId == localId && x.Index == index);

        private MonaBody _monaBody;
        private IMonaBrainVariables _monaVariables;

        public MonaBody MonaBody => _monaBody;
        public IMonaBrainVariables MonaVariables
        {
            get
            {
                if (_monaVariables == null) return null;
                return _monaVariables;
            }
        }

        public string LocalId => _localId;
        public string PrefabId => _prefabId;
        public int Index => _index;

        public bool LocallyOwnedMonaBody => model.identifier.locallyOwnedMonaBody;

        public bool Initialized { get; set; }

        [SerializeField] private string _localId;
        [SerializeField] private string _prefabId;
        [SerializeField] private bool _locallyOwnedMonabody;
        [SerializeField] private int _index;

        public bool HasControl() => _monaBody == null || _monaBody.SyncType == MonaBodyNetworkSyncType.NotNetworked || isOwnedLocallyInHierarchy;

        public MonaVariablesNetworkIdentifierModel Identifier { get; set; }

        private void IdentifierChanged(string localId, string prefabId, bool locallyOwnedMonabody, int index)
        {
            if (_localId == localId && _prefabId == prefabId && _locallyOwnedMonabody == locallyOwnedMonabody && _index == index) return;

            _localId = localId;
            _prefabId = prefabId;
            _locallyOwnedMonabody = locallyOwnedMonabody;
            _index = index;

            Debug.Log($"{nameof(BrainsNormcoreMonaVariablesNetworkBehaviour)}.{nameof(IdentifierChanged)} new network object, find local copy of {_localId} prefab: {_prefabId}");

            gameObject.name = _localId + ":NetworkVariables";

            if (!Initialized)
            {
                Initialized = true;
                RegisterNetworkMonaVariables();
            }
        }

        private void RegisterNetworkMonaVariables()
        {
            StopCoroutine("WaitForNetworkSpawner");
            StartCoroutine("WaitForNetworkSpawner");
        }

        private IEnumerator WaitForNetworkSpawner()
        {
            var timeout = 0f;
            while (MonaGlobalBrainRunner.Instance.NetworkSpawner == null && timeout <= MonaCoreConstants.WAIT_FOR_NETWORK_SPAWNER_TIMEOUT)
            {
                timeout += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();

            if (MonaGlobalBrainRunner.Instance.NetworkSpawner != null && gameObject != null)
                MonaGlobalBrainRunner.Instance.NetworkSpawner.RegisterNetworkMonaVariables(this);
        }

        public void SetIdentifier(string localId, int index, string prefabId, bool locallyOwnedMonaBody)
        {
            model.identifier.SetIdentifier(localId, prefabId, locallyOwnedMonaBody, index);
        }

        private void FloatsChanged(MonaVariablesFloatNetworkModel model, float value)
        {
            //Debug.Log($"{nameof(FloatsChanged)} {model.key} {model.value}");
            SyncFloats(value, model.key);
        }

        private void SyncFloats()
        {
            foreach (var pair in model.floats)
                SyncFloats(pair.Value.value, (int)pair.Key);
        }

        private void SyncFloats(float value, int key)
        {
            //Debug.Log($"{nameof(SyncFloats)} {value} key {key}");
            if (MonaVariables == null) return;
            var variable = MonaVariables.GetVariableByIndex(key);
            if (!variable.IsLocal)
            {
                var variableName = variable.Name;
                MonaVariables?.Set(variableName, value, false);
            }
        }

        private void BoolsChanged(MonaVariablesBoolNetworkModel model, bool value)
        {
            //Debug.Log($"{nameof(BoolsChanged)} {model.key} {model.value}");
            SyncBools(value, model.key);
        }

        private void SyncBools()
        {
            foreach (var pair in model.bools)
                SyncBools(pair.Value.value, (int)pair.Key);
        }

        private void SyncBools(bool value, int key)
        {
            if (MonaVariables == null) return;
            //Debug.Log($"{nameof(SyncBools)} {_monaBody.gameObject.name} name: {key} index: {_index}", _monaBody.gameObject);
            var variable = MonaVariables.GetVariableByIndex(key);
            if (!variable.IsLocal)
            {
                var variableName = variable.Name;
                MonaVariables?.Set(variableName, value, false);
            }
        }

        private void StringsChanged(MonaVariablesStringNetworkModel model, string value)
        {
            Debug.Log($"{nameof(BrainsNormcoreMonaVariablesNetworkBehaviour)}.{nameof(StringsChanged)} {model.key} {value}");
            SyncStrings(value, model.key);
        }

        private void SyncStrings()
        {
            foreach (var pair in model.strings)
                SyncStrings(pair.Value.value, (int)pair.Key);
        }

        private void SyncStrings(string value, int key)
        {
            if (MonaVariables == null) return;
            var variable = MonaVariables.GetVariableByIndex(key);
            if (!variable.IsLocal)
            {
                var variableName = variable.Name;
                Debug.Log($"{nameof(SyncStrings)} {_monaBody.LocalId} {variableName} = {value} ");
                MonaVariables?.Set(variableName, value, false);
            }
        }

        private void Vector2Changed(MonaVariablesVector2NetworkModel model, Vector2 value)
        {
            SyncVector2s(value, model.key);
        }

        private void SyncVector2s()
        {
            foreach (var pair in model.vector2s)
                SyncVector2s(pair.Value.value, (int)pair.Key);
        }

        private void SyncVector2s(Vector2 value, int key)
        {
            if (MonaVariables == null) return;
            var variable = MonaVariables.GetVariableByIndex(key);
            if (!variable.IsLocal)
            {
                var variableName = variable.Name;
                MonaVariables?.Set(variableName, value, false);
            }
        }

        private void Vector3Changed(MonaVariablesVector3NetworkModel model, Vector3 value)
        {
            //Debug.Log($"{nameof(MonaVariablesNetworkBehavior)}.{nameof(Vector3Changed)}");
            SyncVector3s(value, model.key);
        }

        private void SyncVector3s()
        {
            foreach (var pair in model.vector3s)
                SyncVector3s(pair.Value.value, (int)pair.Key);
        }

        private void SyncVector3s(Vector3 value, int key)
        {
            if (MonaVariables == null) return;
            var variable = MonaVariables.GetVariableByIndex(key);
            if (!variable.IsLocal)
            {
                var variableName = variable.Name;
                MonaVariables?.Set(variableName, value, false);
            }
        }

        protected override void OnRealtimeModelReplaced(MonaVariablesNetworkModel previousModel, MonaVariablesNetworkModel currentModel)
        {
            if (previousModel != null)
            {
                // Unregister from events
                previousModel.identifier.eventDidFire -= IdentifierChanged;

                previousModel.floats.modelAdded -= FloatModelAdded;
                previousModel.floats.modelRemoved -= FloatModelAdded;

                previousModel.bools.modelAdded -= BoolModelAdded;
                previousModel.bools.modelRemoved -= BoolModelAdded;

                previousModel.strings.modelAdded -= StringModelAdded;
                previousModel.strings.modelRemoved -= StringModelRemoved;

                previousModel.vector2s.modelAdded -= Vector2ModelAdded;
                previousModel.vector2s.modelRemoved -= Vector2ModelRemoved;

                previousModel.vector3s.modelAdded -= Vector3ModelAdded;
                previousModel.vector3s.modelRemoved -= Vector3ModelRemoved;

            }

            if (currentModel != null)
            {
                // If this is a model that has no data set on it, populate it with the current mesh renderer color.
                if (currentModel.isFreshModel)
                {
                    
                }

                // Update the mesh render to match the new model

                // Register for events so we'll know if the color changes later
                currentModel.identifier.eventDidFire += IdentifierChanged;

                currentModel.floats.modelAdded += FloatModelAdded;
                currentModel.floats.modelRemoved += FloatModelAdded;

                currentModel.bools.modelAdded += BoolModelAdded;
                currentModel.bools.modelRemoved += BoolModelAdded;

                currentModel.strings.modelAdded += StringModelAdded;
                currentModel.strings.modelRemoved += StringModelRemoved;

                currentModel.vector2s.modelAdded += Vector2ModelAdded;
                currentModel.vector2s.modelRemoved += Vector2ModelRemoved;

                currentModel.vector3s.modelAdded += Vector3ModelAdded;
                currentModel.vector3s.modelRemoved += Vector3ModelRemoved;

                foreach (var pair in currentModel.floats)
                    pair.Value.valueDidChange += FloatsChanged;

                foreach (var pair in currentModel.bools)
                    pair.Value.valueDidChange += BoolsChanged;

                foreach (var pair in currentModel.strings)
                    pair.Value.valueDidChange += StringsChanged;

                foreach (var pair in currentModel.vector2s)
                    pair.Value.valueDidChange += Vector2Changed;

                foreach (var pair in currentModel.vector3s)
                    pair.Value.valueDidChange += Vector3Changed;
            }
        }

        private void FloatModelAdded(RealtimeDictionary<MonaVariablesFloatNetworkModel> dictionary, uint key, MonaVariablesFloatNetworkModel model, bool remote)
        {
            model.valueDidChange += FloatsChanged;
            SyncFloats(model.value, (int)key);
        }

        private void FloatModelRemoved(RealtimeDictionary<MonaVariablesFloatNetworkModel> dictionary, uint key, MonaVariablesFloatNetworkModel model, bool remote)
        {
            model.valueDidChange -= FloatsChanged;
        }

        private void BoolModelAdded(RealtimeDictionary<MonaVariablesBoolNetworkModel> dictionary, uint key, MonaVariablesBoolNetworkModel model, bool remote)
        {
            model.valueDidChange += BoolsChanged;
            SyncBools(model.value, (int)key);
        }

        private void BoolModelRemoved(RealtimeDictionary<MonaVariablesBoolNetworkModel> dictionary, uint key, MonaVariablesBoolNetworkModel model, bool remote)
        {
            model.valueDidChange -= BoolsChanged;
        }

        private void StringModelAdded(RealtimeDictionary<MonaVariablesStringNetworkModel> dictionary, uint key, MonaVariablesStringNetworkModel model, bool remote)
        {
            model.valueDidChange += StringsChanged;
            SyncStrings(model.value, (int)key);
        }

        private void StringModelRemoved(RealtimeDictionary<MonaVariablesStringNetworkModel> dictionary, uint key, MonaVariablesStringNetworkModel model, bool remote)
        {
            model.valueDidChange -= StringsChanged;
        }

        private void Vector2ModelAdded(RealtimeDictionary<MonaVariablesVector2NetworkModel> dictionary, uint key, MonaVariablesVector2NetworkModel model, bool remote)
        {
            model.valueDidChange += Vector2Changed;
            SyncVector2s(model.value, (int)key);
        }

        private void Vector2ModelRemoved(RealtimeDictionary<MonaVariablesVector2NetworkModel> dictionary, uint key, MonaVariablesVector2NetworkModel model, bool remote)
        {
            model.valueDidChange -= Vector2Changed;
        }

        private void Vector3ModelAdded(RealtimeDictionary<MonaVariablesVector3NetworkModel> dictionary, uint key, MonaVariablesVector3NetworkModel model, bool remote)
        {
            model.valueDidChange += Vector3Changed;
            SyncVector3s(model.value, (int)key);
        }

        private void Vector3ModelRemoved(RealtimeDictionary<MonaVariablesVector3NetworkModel> dictionary, uint key, MonaVariablesVector3NetworkModel model, bool remote)
        {
            model.valueDidChange -= Vector3Changed;
        }


        private void Awake()
        {
            if (!NetworkMonaVariables.Contains(this))
                NetworkMonaVariables.Add(this);
        }

        private void Start()
        {
            if (realtime.connected)
            {
                HandleConnected(realtime);
            }
            else
            {
                realtime.didConnectToRoom += HandleConnected;
            }

        }

        public void HandleConnected(Realtime realtime)
        {
            Debug.Log($"MONA OBJECT STATE SPAWNED");
        }

        public void SetMonaBody(MonaBody monaBody, int index)
        {
            Debug.Log($"{nameof(SetMonaBody)} {monaBody.LocalId} {index}", monaBody.Transform.gameObject);
            _monaBody = monaBody;

            if (_monaBody != null)
            {
                var states = _monaBody.GetComponents<IMonaBrainVariables>();
                if (index < states.Length)
                {
                    _monaVariables = states[index];
                    Debug.Log($"bind network variables instance to {_monaBody.name} at index {index}", this.gameObject);
                    _monaVariables.SetNetworkVariables((INetworkMonaVariables)this);
                    _monaBody.OnControlRequested -= HandleBodyControlRequested;
                    _monaBody.OnControlRequested += HandleBodyControlRequested;
                }
            }

            ownerIDSelfDidChange -= HandleOwnerChanged;
            ownerIDSelfDidChange += HandleOwnerChanged;

            SyncBools();
            SyncFloats();
            SyncStrings();
            SyncVector2s();
            SyncVector3s();
        }

        private void HandleOwnerChanged(RealtimeComponent<MonaVariablesNetworkModel> view, int ownerID)
        {
            OnStateAuthorityChanged?.Invoke();
        }


        private void HandleBodyControlRequested()
        {
            //Debug.Log($"{nameof(HandleBodyControlRequested)} {this}", this.gameObject);
            realtimeView.RequestOwnershipOfSelfAndChildren();
        }

        public void TakeControl()
        {
            realtimeView.RequestOwnershipOfSelfAndChildren();
        }

        private void OnDestroy()
        {
            if (_monaBody != null)
                _monaBody.OnControlRequested -= HandleBodyControlRequested;
        }

        public void UpdateValue(IMonaVariablesValue value)
        {
            if (!HasControl()) return;
            if (value.IsLocal) return;
            if (value is IMonaVariablesBoolValue) SendBoolRpc(value.Name, ((IMonaVariablesBoolValue)value).Value);
            else if (value is IMonaVariablesFloatValue) SendFloatRpc(value.Name, ((IMonaVariablesFloatValue)value).Value);
            else if (value is IMonaVariablesStringValue) SendStringRpc(value.Name, ((IMonaVariablesStringValue)value).Value);
            else if (value is IMonaVariablesVector2Value) SendVector2Rpc(value.Name, ((IMonaVariablesVector2Value)value).Value);
            else if (value is IMonaVariablesVector3Value) SendVector3Rpc(value.Name, ((IMonaVariablesVector3Value)value).Value);
        }

        public void SendFloatRpc(string variableName, float value)
        {
            SetNetworkFloat(variableName, value);
        }

        private void SetNetworkFloat(string variableName, float value)
        {
            var key = MonaVariables.GetVariableIndexByName(variableName);
            MonaVariablesFloatNetworkModel localModel;
            if (model.floats.ContainsKey((uint)key))
            {
                model.floats[(uint)key].value = value;
            }
            else
            {
                localModel = new MonaVariablesFloatNetworkModel();
                localModel.value = value;
                localModel.key = (int)key;
                model.floats.Add((uint)key, localModel);
            }
        }

        public void SendBoolRpc(string variableName, bool value)
        {
            SetNetworkBool(variableName, value);
        }

        private void SetNetworkBool(string variableName, bool value)
        {
            var key = MonaVariables.GetVariableIndexByName(variableName);
            MonaVariablesBoolNetworkModel localModel;
            if (model.bools.ContainsKey((uint)key))
            {
                model.bools[(uint)key].value = value;
            }
            else
            {
                localModel = new MonaVariablesBoolNetworkModel();
                localModel.value = value;
                localModel.key = (int)key;
                model.bools.Add((uint)key, localModel);
            }
        }

        public void SendStringRpc(string variableName, string value)
        {
            Debug.Log($"{nameof(SendStringRpc)} {variableName} : {value}");
            SetNetworkString(variableName, value);
        }

        private void SetNetworkString(string variableName, string value)
        {
            var key = MonaVariables.GetVariableIndexByName(variableName);
            MonaVariablesStringNetworkModel localModel;
            if (model.strings.ContainsKey((uint)key))
            {
                model.strings[(uint)key].value = value;
            }
            else
            {
                localModel = new MonaVariablesStringNetworkModel();
                localModel.value = value;
                localModel.key = (int)key;
                model.strings.Add((uint)key, localModel);
            }
        }

        public void SendVector2Rpc(string variableName, Vector2 value)
        {
            SetNetworkVector2(variableName, value);
        }

        private void SetNetworkVector2(string variableName, Vector2 value)
        {
            var key = MonaVariables.GetVariableIndexByName(variableName);
            MonaVariablesVector2NetworkModel localModel;
            if (model.vector2s.ContainsKey((uint)key))
            {
                model.vector2s[(uint)key].value = value;
            }
            else
            {
                localModel = new MonaVariablesVector2NetworkModel();
                localModel.value = value;
                localModel.key = (int)key;
                model.vector2s.Add((uint)key, localModel);
            }
        }

        public void SendVector3Rpc(string variableName, Vector3 value)
        {
            SetNetworkVector3(variableName, value);
        }

        private void SetNetworkVector3(string variableName, Vector3 value)
        {
            var key = MonaVariables.GetVariableIndexByName(variableName);
            MonaVariablesVector3NetworkModel localModel;
            if (model.vector3s.ContainsKey((uint)key))
            {
                model.vector3s[(uint)key].value = value;
            }
            else
            {
                localModel = new MonaVariablesVector3NetworkModel();
                localModel.value = value;
                localModel.key = (int)key;
                model.vector3s.Add((uint)key, localModel);
            }
        }
    }
}
#endif