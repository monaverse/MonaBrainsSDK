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
    public class AttachToTagInstructionTile : InstructionTile, IAttachToTagInstructionTile, IActionInstructionTile
    {
        public const string ID = "AttachToTag";
        public const string NAME = "Attach To Tag";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToTagInstructionTile);

        [SerializeField]
        private string _tag = "Default";
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        private IMonaBrain _brain;

        public AttachToTagInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_tag.ToString());
            if (bodies != null && bodies.Count > 0)
            {
                var body = bodies[0];
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetTransformParent(body.ActiveTransform);
                _brain.Body.SetPosition(body.ActiveTransform.position + body.ActiveTransform.parent.TransformDirection(_offset), true, true);
                _brain.Body.SetRotation(body.ActiveTransform.rotation, true, true);
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}