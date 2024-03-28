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

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class SpawnInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
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

        protected IMonaBrain _brain;
        private IMonaBodyAssetItem _item;
        private List<IMonaBody> _equipmentInstances = new List<IMonaBody>();
        private Dictionary<string, List<IMonaBody>> _pool = new Dictionary<string, List<IMonaBody>>();

        public enum LocationType
        {
            Me,
            MyPart,
            OtherWithTag,
            OtherWithTagPart
        }

        public SpawnInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
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
            Debug.Log($"{nameof(SetupSpawnable)} spawn asset instruction tile");
            var items = GetPreloadAssets();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) return;
                if (_brain.Body.IsAttachedToRemotePlayer()) return;
                if (!_brain.Body.HasControl()) return;

                int poolCount = !string.IsNullOrEmpty(_poolCountName) ?
                    (int)Mathf.Ceil(_brain.Variables.GetFloat(_poolCountName)) : (int)Mathf.Ceil(_poolCount);

                for (var j = 0; j < poolCount; j++)
                {
                    Spawn(item.PrefabId, item.Value);
                }
            }
        }

        protected void Spawn(string prefabId, MonaBody monaBody)
        {
            var body = (IMonaBody)GameObject.Instantiate(monaBody);

            var bodies = body.Transform.GetComponentsInChildren<IMonaBody>();
            for (var j = 0; j < bodies.Length; j++)
            {
                var child = bodies[j];
                if (child == body)
                {
                    child.Transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
                    _equipmentInstances.Add(child);

                    if (!_pool.ContainsKey(prefabId))
                        _pool.Add(prefabId, new List<IMonaBody>());

                    _pool[prefabId].Add(child);
                    child.OnDisabled += HandleBodyDisabled;
                    child.SetActive(false);
                }

                ((MonaBodyBase)child).PrefabId = prefabId;
                ((MonaBodyBase)child).MakeUnique(_brain.Player.PlayerId, true);
                EventBus.Trigger<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), new MonaBodyInstantiatedEvent(child));
            }
        }

        private void HandleBodyDisabled(IMonaBody body)
        {
            if (!_pool[((MonaBodyBase)body).PrefabId].Contains(body))
                _pool[((MonaBodyBase)body).PrefabId].Add(body);
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
            if (body != null)
            {
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                var nextItem = GetAsset();

                if (_pool[nextItem.PrefabId].Count > 0)
                {
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
 
                    poolItem.SetActive(true);
                    poolItem.SetVisible(false);

                    if (poolItem.ActiveRigidbody != null)
                        poolItem.ActiveRigidbody.WakeUp();

                    Vector3 position = body.GetPosition() + offset;
                    Quaternion rotation = body.GetRotation() * Quaternion.Euler(eulerAngles);
                    poolItem.SetSpawnTransforms(position, rotation, scale, true);

                    var childBrains = poolItem.Transform.GetComponentsInChildren<IMonaBrainRunner>();
                    for(var i = 0;i < childBrains.Length; i++)
                        childBrains[i].CacheTransforms();

                    poolItem.SetVisible(true);
                    Debug.Log($"{nameof(SpawnInstructionTile)} {poolItem}", poolItem.Transform.gameObject);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, poolItem);
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        public override void Unload()
        {
            if(_brain.LoggingEnabled) Debug.Log($"{nameof(Unload)} spawn asset instruction tile unload");
            base.Unload();
            for (var i = 0; i < _equipmentInstances.Count; i++)
            {
                var instance = _equipmentInstances[i];
                if (instance == null) continue;
                instance.OnDisabled -= HandleBodyDisabled;
                if(instance.Transform != null && instance.Transform.gameObject != null)
                    GameObject.Destroy(instance.Transform.gameObject);
            }
            _equipmentInstances.Clear();
            _pool.Clear();
        }
    }
}