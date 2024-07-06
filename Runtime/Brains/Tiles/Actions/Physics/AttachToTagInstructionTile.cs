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
    public class AttachToTagInstructionTile : InstructionTile, IAttachToTagInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile
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

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }

        private IMonaBody GetTarget()
        {
            if (_brain.MonaTagSource.GetTag(_tag).IsPlayerTag)
            {
                if (_brain.Player.PlayerBody != null)
                {
                    return _brain.Player.PlayerBody;
                }
            }
                        
            var bodies = MonaBody.FindByTag(_tag.ToString());
            if (bodies != null && bodies.Count > 0)
            {
                var body = bodies[0];
                return body;
            }
            return null;            
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            var body = GetTarget();
            if(body != null)
            {
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetTransformParent(body.ActiveTransform);
                if(body.ActiveTransform.parent != null)
                    _brain.Body.TeleportPosition(body.ActiveTransform.position + body.ActiveTransform.parent.TransformDirection(_offset), true);
                else
                    _brain.Body.TeleportPosition(body.ActiveTransform.position + body.ActiveTransform.TransformDirection(_offset), true);
                _brain.Body.SetRotation(body.ActiveTransform.rotation, true);
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}