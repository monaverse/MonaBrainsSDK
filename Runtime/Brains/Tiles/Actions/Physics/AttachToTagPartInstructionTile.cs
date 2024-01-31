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

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class AttachToTagPartInstructionTile : InstructionTile, IAttachToTagPartInstructionTile, IActionInstructionTile
    {
        public const string ID = "AttachToTagPart";
        public const string NAME = "Attach To Tag Part";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToTagPartInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag] public string Tag { get => _tag; set => _tag = value; }
        
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

        public AttachToTagPartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_tag);

            if (bodies != null && bodies.Count > 0)
            {
                var body = bodies[0];
                var playerPart = body.FindChildByTag(_part.ToString());
                if (playerPart == null) playerPart = body;
                if (body.HasMonaTag(MonaCoreConstants.TAG_PLAYER))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetTransformParent(playerPart.ActiveTransform);
                _brain.Body.SetPosition(playerPart.ActiveTransform.position + playerPart.ActiveTransform.parent.TransformDirection(_offset), true, true);
                _brain.Body.SetRotation(playerPart.ActiveTransform.rotation, true, true);
            }
         
            return Complete(InstructionTileResult.Success);
        }
    }
}