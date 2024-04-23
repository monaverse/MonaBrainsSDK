using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Animation;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class SpawnInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "Spawn";
        public const string NAME = "Spawn Asset";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(SpawnInstructionTile);

        [SerializeField] private float _poolCount = 5;
        [SerializeField] private string _poolCountName;

        [BrainProperty(true)]
        public float PoolCount { get => _poolCount; set => _poolCount = value; }
        [BrainPropertyValueName("PoolCount", typeof(IMonaVariablesFloatValue))]
        public string PoolCountName { get => _poolCountName; set => _poolCountName = value; }

        [SerializeField] private LocationType _location;
        [BrainPropertyEnum(false)] public LocationType Location { get => _location; set => _location = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(Location), (int)LocationType.OtherWithTag)]
        [BrainPropertyShow(nameof(Location), (int)LocationType.OtherWithTagPart)]
        [BrainPropertyMonaTag(false)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField]
        private string _part = "Default";
        [BrainPropertyMonaTag(false)]
        [BrainPropertyShow(nameof(Location), (int)LocationType.MyPart)]
        [BrainPropertyShow(nameof(Location), (int)LocationType.OtherWithTagPart)]
        public string Part { get => _part; set => _part = value; }

        [SerializeField] private bool _spawnAsChild;
        [SerializeField] private string _spawnAsChildName;
        [BrainProperty(false)] public bool SpawnAsChild { get => _spawnAsChild; set => _spawnAsChild = value; }
        [BrainPropertyValueName("SpawnAsChild", typeof(IMonaVariablesBoolValue))] public string SpawnAsChildName { get => _spawnAsChildName; set => _spawnAsChildName = value; }

        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private string[] _offsetName = new string[4];
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesVector3Value))]
        public string[] OffsetName { get => _offsetName; set => _offsetName = value; }

        [SerializeField] private Vector3 _eulerAngles = Vector3.zero;
        [SerializeField] private string[] _eulerAnglesName = new string[4];

        [BrainProperty(false)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }
        [BrainPropertyValueName("Rotation", typeof(IMonaVariablesVector3Value))]
        public string[] EulerAnglesName { get => _eulerAnglesName; set => _eulerAnglesName = value; }

        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private string[] _scaleName = new string[4];
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }
        [BrainPropertyValueName("Scale", typeof(IMonaVariablesVector3Value))]
        public string[] ScaleName { get => _scaleName; set => _scaleName = value; }

        [SerializeField] private bool _spawnOnEmpty;
        [BrainProperty(false)] public bool SpawnOnEmpty { get => _spawnOnEmpty; set => _spawnOnEmpty = value; }

        [SerializeField] private bool _destroyOnDisable;
        [BrainProperty(false)] public bool DestroyOnDisable { get => _destroyOnDisable; set => _destroyOnDisable = value; }

        protected IMonaBrain _brain;
        private Transform _defaultParent;
        private IMonaBodyAssetItem _item;
        private List<IMonaBody> _equipmentInstances = new List<IMonaBody>();
        private Dictionary<string, List<IMonaBody>> _pool = new Dictionary<string, List<IMonaBody>>();
        private bool _shouldSpawn = true;

        public enum LocationType
        {
            Me,
            MyPart,
            OtherWithTag,
            OtherWithTagPart,
            LastSpawnedByMe = 80
        }

        public SpawnInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
            _defaultParent = GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform;
            SetupSpawnable();
        }

        protected virtual List<IMonaBodyAssetItem> GetPreloadAssets()
        {
            return null;
        }

        protected virtual IMonaBodyAssetItem GetAsset()
        {
            return null;
        }

        private void SetupSpawnable()
        {
            if (!_shouldSpawn) return;

            _shouldSpawn = false;

            //Debug.Log($"{nameof(SetupSpawnable)} spawn asset instruction tile");
            var items = GetPreloadAssets();

            int poolCount = !string.IsNullOrEmpty(_poolCountName) ? (int)Mathf.Ceil(_brain.Variables.GetFloat(_poolCountName)) : (int)Mathf.Ceil(_poolCount);

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) return;
                if (_brain.Body.IsAttachedToRemotePlayer()) return;
                if (!_brain.Body.HasControl()) return;

                for (var j = 0; j < poolCount; j++)
                {
                    Spawn(item.PrefabId, item.Value);
                }
            }
        }

        protected void Spawn(string prefabId, MonaBody monaBody, bool disable = true)
        {
            var body = (IMonaBody)GameObject.Instantiate(monaBody, Vector3.up*10000f, Quaternion.identity);

            var bodies = body.Transform.GetComponentsInChildren<IMonaBody>();
            for (var j = 0; j < bodies.Length; j++)
            {
                var child = bodies[j];
                if (child == body)
                {
                    child.Transform.SetParent(_defaultParent);
                    _equipmentInstances.Add(child);

                    if (!_pool.ContainsKey(prefabId))
                        _pool.Add(prefabId, new List<IMonaBody>());

                    _pool[prefabId].Add(child);
                    child.OnDisabled += HandleBodyDisabled;
                    if (disable)
                        child.SetActive(false);
                    else
                        child.SetActive(true);
                }

                ((MonaBodyBase)child).PrefabId = prefabId;
                ((MonaBodyBase)child).MakeUnique(_brain.Player.PlayerId, true);
                MonaEventBus.Trigger<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), new MonaBodyInstantiatedEvent(child));
            }
        }

        private void HandleBodyDisabled(IMonaBody body)
        {
            if (_brain.SpawnedBodies.Contains(body))
                _brain.SpawnedBodies.Remove(body);

            if (_destroyOnDisable)
                body.Destroy();
            else
            {
                if (!_pool[((MonaBodyBase)body).PrefabId].Contains(body))
                    _pool[((MonaBodyBase)body).PrefabId].Add(body);
            }
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private IMonaBody GetBody()
        {
            var body = _brain.Body;

            switch (_location)
            {
                case LocationType.MyPart:
                    body = body.FindChildByTag(_part);
                    break;

                case LocationType.OtherWithTag:
                    return GetTargetBody();

                case LocationType.OtherWithTagPart:
                    var otherPartBody = GetTargetBody();

                    if (otherPartBody == null)
                        return null;

                    var otherPart = otherPartBody.FindChildByTag(_part);
                    return otherPart != null ? otherPart : otherPartBody;
                case LocationType.LastSpawnedByMe:
                    var lastSpawnedBody = _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                    return lastSpawnedBody != null ? lastSpawnedBody : body;
            }
            
            return body;
        }

        private IMonaBody GetTargetBody()
        {
            if (_brain.MonaTagSource.GetTag(_tag).IsPlayerTag && _brain.Player.PlayerBody != null)
            {
                return _brain.Player.PlayerBody;
            }
            else
            {
                var bodies = MonaBody.FindByTag(_tag);
                if (bodies != null && bodies.Count > 0)
                {
                    var body = bodies[0];
                    return body;
                }
            }

            return null;
        }

        public override InstructionTileResult Do()
        {
            var body = GetBody();

            if (body == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_spawnAsChildName))
                _spawnAsChild = _brain.Variables.GetBool(_spawnAsChildName);

            if (_brain.HasPlayerTag(body.MonaTags))
                _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

            var nextItem = GetAsset();

            if (nextItem == null)
                return Complete(InstructionTileResult.Failure);

            if (!_pool.ContainsKey(nextItem.PrefabId))
                _pool.Add(nextItem.PrefabId, new List<IMonaBody>());

            if (_pool[nextItem.PrefabId].Count < 1 && !_spawnOnEmpty)
                return Complete(InstructionTileResult.Failure);

            if (_pool[nextItem.PrefabId].Count == 0)
                Spawn(nextItem.PrefabId, nextItem.Value, disable: false);

            var poolItem = _pool[nextItem.PrefabId][0];
            _pool[nextItem.PrefabId].RemoveAt(0);
            poolItem.SetScale(_scale, true);

            var offset = _offset;
            if (HasVector3Values(_offsetName))
                offset = GetVector3Value(_brain, _offsetName);

            var eulerAngles = _eulerAngles;
            if (HasVector3Values(_eulerAnglesName))
                eulerAngles = GetVector3Value(_brain, _eulerAnglesName);

            var scale = _scale;
            if (HasVector3Values(_scaleName))
                scale = GetVector3Value(_brain, _scaleName);

            //Debug.Log($"{nameof(SpawnInstructionTile)} {poolItem}", poolItem.Transform.gameObject);

            poolItem.SetActive(true);
            poolItem.SetVisible(false);

            if (poolItem.ActiveRigidbody != null)
                poolItem.ActiveRigidbody.WakeUp();

            poolItem.Transform.SetParent(_spawnAsChild ? _brain.Body.Transform : _defaultParent);

            Vector3 position = body.GetPosition() + body.GetRotation() * offset;
            Quaternion rotation = body.GetRotation() * Quaternion.Euler(eulerAngles);

            poolItem.SetSpawnTransforms(position, rotation, scale, _spawnAsChild, true);

            var childBrains = poolItem.Transform.GetComponentsInChildren<IMonaBrainRunner>();
            for (var i = 0; i < childBrains.Length; i++)
                childBrains[i].CacheTransforms();

            poolItem.SetVisible(true);

            IMonaBody previouslySpawnedBody = _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);

            _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, poolItem);
            _brain.Variables.Set(MonaBrainConstants.RESULT_LAST_SPAWNED, poolItem);
            _brain.SpawnedBodies.Add(poolItem);
            SetSpawnerReferenceOnSpawned(poolItem, previouslySpawnedBody);

            if (previouslySpawnedBody != null)
                SetNextBodyReferenceOnPrevious(previouslySpawnedBody, poolItem);


            //Debug.Log($"{nameof(SpawnInstructionTile)} SPAWN COMPLETE: {poolItem}", poolItem.Transform.gameObject);
            return Complete(InstructionTileResult.Success);

        }

        private void SetSpawnerReferenceOnSpawned(IMonaBody spawned, IMonaBody previouslySpawnedBody)
        {
            spawned.Spawner = _brain.Body;
            spawned.PoolBodyPrevious = previouslySpawnedBody;

            var children = spawned.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null)
                    SetSpawnerReferenceOnSpawned(children[i], previouslySpawnedBody);
            }
        }

        private void SetNextBodyReferenceOnPrevious(IMonaBody targetBody, IMonaBody spawned)
        {
            targetBody.PoolBodyNext = spawned;

            var children = targetBody.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null)
                    SetNextBodyReferenceOnPrevious(children[i], spawned);
            }
        }

        public override void Unload(bool destroy = false)
        {
            //if(_brain.LoggingEnabled) //
            //Debug.Log($"{nameof(Unload)} spawn asset instruction tile unload");
            base.Unload();
            for (var i = 0; i < _equipmentInstances.Count; i++)
            {
                var instance = _equipmentInstances[i];
                if (instance == null) continue;
                if (destroy)
                {
                    instance.OnDisabled -= HandleBodyDisabled;
                    if (instance.Transform != null && instance.Transform.gameObject != null)
                        instance.Destroy();
                }
                else
                    instance.SetActive(false);
            }

            if (destroy)
            {
                _shouldSpawn = true;
                _brain.SpawnedBodies.Clear();
                _equipmentInstances.Clear();
                _pool.Clear();
            }
        }
    }
}