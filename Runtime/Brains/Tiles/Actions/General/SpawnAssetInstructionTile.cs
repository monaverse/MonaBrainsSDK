﻿using UnityEngine;
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

        [SerializeField]
        private float _poolCount = 5;
        [BrainProperty(true)]
        public float PoolCount { get => _poolCount; set => _poolCount = value; }

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
        [SerializeField] private string _offsetName;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesVector3Value))]
        public string OffsetName { get => _offsetName; set => _offsetName = value; }

        [SerializeField] private Vector3 _eulerAngles = Vector3.zero;
        [SerializeField] private string _eulerAnglesName;

        [BrainProperty(false)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }
        [BrainPropertyValueName("Rotation", typeof(IMonaVariablesVector3Value))]
        public string EulerAnglesName { get => _eulerAnglesName; set => _eulerAnglesName = value; }

        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private string _scaleName;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }
        [BrainPropertyValueName("Scale", typeof(IMonaVariablesVector3Value))]
        public string ScaleName { get => _scaleName; set => _scaleName = value; }

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

            for (var i = 0; i < _poolCount; i++)
            {
                var body = (IMonaBody)GameObject.Instantiate(_item.Value);
                body.Transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
                ((MonaBodyBase)body).PrefabId = _monaAsset;
                ((MonaBodyBase)body).MakeUnique(_brain.Player.PlayerId, true);
                _equipmentInstances.Add(body);
                _pool.Add(body);
                body.OnDisabled += HandleBodyDisabled;
                body.SetActive(false);
                EventBus.Trigger<MonaBodyInstantiatedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_INSTANTIATED), new MonaBodyInstantiatedEvent(body));
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

                    var offset = !string.IsNullOrEmpty(_offsetName) ?
                        _brain.Variables.GetVector3(_offsetName) : _offset;
                    var eulerAngles = !string.IsNullOrEmpty(_eulerAnglesName) ?
                        _brain.Variables.GetVector3(_eulerAnglesName) : _eulerAngles;
                    var scale = !string.IsNullOrEmpty(_scaleName) ?
                        _brain.Variables.GetVector3(_scaleName) : _scale;

                    if (body.ActiveTransform != null)
                        offset = body.ActiveTransform.TransformDirection(_offset);
 
                    poolItem.SetActive(true);
                    poolItem.SetVisible(false);

                    if (poolItem.ActiveRigidbody != null)
                        poolItem.ActiveRigidbody.WakeUp();

                    poolItem.TeleportPosition(body.GetPosition() + offset, true);
                    poolItem.TeleportRotation(body.GetRotation() * Quaternion.Euler(eulerAngles), true);
                    poolItem.TeleportScale(scale, true);
                    if(poolItem.Transform.GetComponent<IMonaBrainRunner>() != null)
                        poolItem.Transform.GetComponent<IMonaBrainRunner>().CacheTransforms();
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