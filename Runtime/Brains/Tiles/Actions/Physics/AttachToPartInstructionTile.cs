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

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class AttachToPartInstructionTile : InstructionTile, IAttachToPartInstructionTile, IActionInstructionTile
    {
        public const string ID = "AttachToPart";
        public const string NAME = "Attach To Part";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToPartInstructionTile);

        [SerializeField]
        private string _tag = "Player";
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        [SerializeField]
        private string _part = "Default";
        [BrainPropertyMonaTag]
        public string Part { get => _part; set => _part = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        private IMonaBrain _brain;

        public AttachToPartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var part = _brain.Body.FindChildByTag(_part.ToString());
            if (part != null)
            {
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetTransformParent(part.ActiveTransform);
                _brain.Body.SetPosition(part.ActiveTransform.position + part.ActiveTransform.parent.TransformDirection(_offset), true, true);
                _brain.Body.SetRotation(part.ActiveTransform.rotation, true, true);
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}