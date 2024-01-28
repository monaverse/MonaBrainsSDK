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
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class GlueToMonaPlayerInstructionTile : InstructionTile, IGlueToMonaPlayerInstructionTile, IActionInstructionTile
    {
        public const string ID = "GlueToMonaPlayer";
        public const string NAME = "Glue To Mona Player";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(GlueToMonaPlayerInstructionTile);

        [SerializeField]
        private MonaPlayerBodyParts _monaPart = MonaPlayerBodyParts.Camera;
        [BrainPropertyEnum(true)]
        public MonaPlayerBodyParts MonaPart { get => _monaPart; set => _monaPart = value; }

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

        public GlueToMonaPlayerInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain.Player != null && _brain.Player.PlayerBody != null)
            {
                _playerPart = _brain.Player.PlayerBody.FindChildByTag(_monaPart.ToString());
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetParent(_playerPart.ActiveTransform);
                _brain.Body.SetPosition(_playerPart.ActiveTransform.position + _playerPart.ActiveTransform.parent.TransformDirection(_offset), true, true);
                _brain.Body.SetRotation(_playerPart.ActiveTransform.rotation, true, true);
            }
         
            return Complete(InstructionTileResult.Success);
        }
    }
}