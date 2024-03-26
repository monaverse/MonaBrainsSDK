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
    public class SpawnAssetInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "SpawnAsset";
        public const string NAME = "Spawn Asset";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(SpawnAssetInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaBodyAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

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

        private IMonaBrain _brain;
        private IMonaBodyAssetItem _item;
        private List<IMonaBody> _equipmentInstances = new List<IMonaBody>();
        private List<IMonaBody> _pool = new List<IMonaBody>();

        public enum LocationType
        {
            Me,
            MyPart,
            OtherWithTag,
            OtherWithTagPart
        }

        public SpawnAssetInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
            SetupSpawnable();
        }

        private void SetupSpawnable()
        {
            Debug.Log($"{nameof(SetupSpawnable)} spawn asset instruction tile");
            _item = (IMonaBodyAssetItem)_brain.GetMonaAsset(_monaAsset);

            if (_brain.Body.IsAttachedToRemotePlayer()) return;
            if (!_brain.Body.HasControl()) return;

            if (_equipmentInstances.Count > 0)
                return;

            int poolCount = !string.IsNullOrEmpty(_poolCountName) ?
                (int)Mathf.Ceil(_brain.Variables.GetFloat(_poolCountName)) : (int)Mathf.Ceil(_poolCount);

            for (var i = 0; i < poolCount; i++)
            {
                var body = (IMonaBody)GameObject.Instantiate(_item.Value);

                var bodies = body.Transform.GetComponentsInChildren<IMonaBody>();
                for(var j = 0;j < bodies.Length; j++)
                {
                    var child = bodies[j];
                    if (child == body)
                    {
                        child.Transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
                        _equipmentInstances.Add(child);
                        _pool.Add(child);
                        child.OnDisabled += HandleBodyDisabled;
                        child.SetActive(false);
                    }

                    ((MonaBodyBase)child).PrefabId = _monaAsset;
                    ((MonaBodyBase)child).MakeUnique(_brain.Player.PlayerId, true);
                    EventBus.Trigger<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), new MonaBodyInstantiatedEvent(child));
                }
            }
        }

        private void HandleBodyDisabled(IMonaBody body)
        {
            if (!_pool.Contains(body))
                _pool.Add(body);
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

                if (_pool.Count > 0)
                {
                    var poolItem = _pool[0];
                    _pool.RemoveAt(0);
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

                    if (body.ActiveTransform != null)
                        offset = body.ActiveTransform.TransformDirection(_offset);
 
                    poolItem.SetActive(true);
                    poolItem.SetVisible(false);

                    if (poolItem.ActiveRigidbody != null)
                        poolItem.ActiveRigidbody.WakeUp();

                    poolItem.TeleportPosition(body.GetPosition() + offset, true);
                    poolItem.TeleportRotation(body.GetRotation() * Quaternion.Euler(eulerAngles), true);
                    poolItem.TeleportScale(scale, true);

                    var childBrains = poolItem.Transform.GetComponentsInChildren<IMonaBrainRunner>();
                    for(var i = 0;i < childBrains.Length; i++)
                        childBrains[i].CacheTransforms();

                    poolItem.SetVisible(true);
                    Debug.Log($"{nameof(SpawnAssetInstructionTile)} {poolItem}", poolItem.Transform.gameObject);
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