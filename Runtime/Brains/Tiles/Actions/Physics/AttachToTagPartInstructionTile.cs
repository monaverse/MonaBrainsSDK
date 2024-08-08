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

        [SerializeField]
        private bool _pinDontParent = false;
        [BrainProperty(false)]
        public bool PinDontParent { get => _pinDontParent; set => _pinDontParent = value; }

        private IMonaBrain _brain;

        public AttachToTagPartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
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

            var bodies = MonaBodyFactory.FindByTag(_tag.ToString());
            if (bodies != null && bodies.Count > 0)
            {
                var body = bodies[0];
                return body;
            }
            return null;
        }

        public override InstructionTileResult Do()
        {
            var body = GetTarget();
            if(body != null)
            { 
                var playerPart = body.FindChildByTag(_part.ToString());
                if (playerPart == null) playerPart = body;
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetTransformParent(playerPart.ActiveTransform);

                if (_pinDontParent)
                    _brain.Body.PinToParent(playerPart.ActiveTransform, () => {
                        if (playerPart.ActiveTransform.parent != null)
                            return playerPart.ActiveTransform.position + playerPart.ActiveTransform.parent.TransformDirection(_offset);
                        else
                            return playerPart.ActiveTransform.position + _offset;

                    }, () => playerPart.ActiveTransform.rotation);
                else
                    _brain.Body.SetTransformParent(playerPart.ActiveTransform);

                if (playerPart.ActiveTransform.parent != null)
                    _brain.Body.TeleportPosition(playerPart.ActiveTransform.position + playerPart.ActiveTransform.parent.TransformDirection(_offset), true);
                else
                    _brain.Body.TeleportPosition(playerPart.ActiveTransform.position + _offset, true);
                _brain.Body.SetRotation(playerPart.ActiveTransform.rotation, true);
            }
         
            return Complete(InstructionTileResult.Success);
        }
    }
}