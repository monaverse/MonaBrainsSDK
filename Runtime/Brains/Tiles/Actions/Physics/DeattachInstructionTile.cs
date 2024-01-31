using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class DeattachInstructionTile : InstructionTile, IDeattachInstructionTile, IActionInstructionTile
    {
        public const string ID = "Deattach";
        public const string NAME = "Deattach";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(DeattachInstructionTile);

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        [SerializeField]
        private bool _letFall = true;
        [BrainProperty(false)]
        public bool LetFall { get => _letFall; set => _letFall = value; }

        private IMonaBrain _brain;

        public DeattachInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var parent = _brain.Body.GetTransformParent();
            if (parent != null)
            {
                var space = GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE);
                _brain.Body.SetTransformParent(space != null ? space.transform : null);
                _brain.Body.SetLayer("Default", true, true);
                _brain.Body.SetPosition(parent.position + parent.rotation * _offset, true, true);
                _brain.Body.SetRotation(parent.rotation, true, true);
                _brain.Body.SetScale(_scale, true);
                if (_letFall) _brain.Body.SetKinematic(false, true);
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}