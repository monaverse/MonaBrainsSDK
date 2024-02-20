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

namespace Mona.SDK.Brains.Tiles.Actions.Character
{
    [Serializable]
    public class EquipAssetInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "EquipAsset";
        public const string NAME = "Equip Asset";
        public const string CATEGORY = "Character";
        public override Type TileType => typeof(EquipAssetInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaBodyAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField]
        private string _part = "Default";
        [BrainPropertyMonaTag]
        public string Part { get => _part; set => _part = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _eulerAngles = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        private IMonaBrain _brain;
        private Transform _root;
        private IMonaAnimationController _monaAnimationController;
        private IMonaBodyAssetItem _item;
        private IMonaBody _equipmentInstance;

        public EquipAssetInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
            SetupWearable();
        }

        private void SetupWearable()
        {
            _root = _brain.Root;
            _monaAnimationController = _root.GetComponent<IMonaAnimationController>();
            _monaAnimationController.SetBrain(_brain);

            _item = (IMonaBodyAssetItem)_brain.GetMonaAsset(_monaAsset);
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private IMonaBody GetTarget()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            var body = GetTarget();
            if (body != null)
            {
                var playerPart = body.FindChildByTag(_part.ToString());
                if (playerPart == null) playerPart = body;
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                _equipmentInstance = (IMonaBody)GameObject.Instantiate(_item.Value);
                _equipmentInstance.SetScale(_scale, true);
                _equipmentInstance.SetTransformParent(playerPart.ActiveTransform);
                _equipmentInstance.Transform.localPosition = _offset;
                _equipmentInstance.Transform.localRotation = Quaternion.Euler(_eulerAngles);
            }

            return Complete(InstructionTileResult.Success);
        }

        public override void Unload()
        {
            base.Unload();
            if(_equipmentInstance != null)
                GameObject.Destroy(_equipmentInstance.Transform.gameObject);
            _equipmentInstance = null;
        }
    }
}