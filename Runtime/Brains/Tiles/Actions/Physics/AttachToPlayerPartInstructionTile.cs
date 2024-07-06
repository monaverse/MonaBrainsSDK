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
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class AttachToPlayerPartInstructionTile : InstructionTile, IAttachToPlayerPartInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile
    {
        public const string ID = "AttachToPlayerPart";
        public const string NAME = "Attach To Player Part";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToPlayerPartInstructionTile);

        [SerializeField]
        private MonaPlayerBodyParts _part = MonaPlayerBodyParts.Camera;
        [BrainPropertyEnum(true)]
        public MonaPlayerBodyParts Part { get => _part; set => _part = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        private IMonaBrain _brain;
        private IMonaBody _playerPart;

        public AttachToPlayerPartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            if (_brain.Player != null && _brain.Player.PlayerBody != null)
            {
                _playerPart = _brain.Player.PlayerBody.FindChildByTag(_part.ToString());
                if (_playerPart == null) _playerPart = _brain.Player.PlayerBody;
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true, true);
                _brain.Body.SetTransformParent(_playerPart.ActiveTransform);
                if (_playerPart.ActiveTransform.parent != null)
                    _brain.Body.TeleportPosition(_playerPart.ActiveTransform.position + _playerPart.ActiveTransform.parent.TransformDirection(_offset), true);
                else
                    _brain.Body.TeleportPosition(_playerPart.ActiveTransform.position + _playerPart.ActiveTransform.TransformDirection(_offset), true);
                _brain.Body.SetRotation(_playerPart.ActiveTransform.rotation, true);
            }
         
            return Complete(InstructionTileResult.Success);
        }
    }
}