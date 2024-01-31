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
    public class AttachToTargetPartInstructionTile : InstructionTile, IAttachToTargetPartInstructionTile, IActionInstructionTile
    {
        public const string ID = "AttachToTargetPart";
        public const string NAME = "Attach To Target Part";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToTargetPartInstructionTile);

        [SerializeField] private MonaBrainTargetResultType _source = MonaBrainTargetResultType.OnConditionTarget;
        [SerializeField] private string _target;

        [BrainProperty(true)] public MonaBrainTargetResultType Source { get => _source; set => _source = value; }
        [BrainPropertyValueName("Source")] public string Target { get => _target; set => _target = value; }

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

        public AttachToTargetPartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            IMonaBody body = GetSource();
            if(!string.IsNullOrEmpty(_target))
            {
                var value = _brain.State.GetValue(_target);
                if (value is IMonaStateBrainValue)
                    body = ((IMonaStateBrainValue)value).Value.Body;
                else if (value is IMonaStateBodyValue)
                    body = ((IMonaStateBodyValue)value).Value;
            }

            if (body != null)
            {
                var playerPart = body.FindChildByTag(_part.ToString());
                if (playerPart == null) playerPart = body;
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);
                _brain.Body.SetTransformParent(playerPart.ActiveTransform);
                _brain.Body.SetPosition(playerPart.ActiveTransform.position + playerPart.ActiveTransform.parent.TransformDirection(_offset), true, true);
                _brain.Body.SetRotation(playerPart.ActiveTransform.rotation, true, true);
            }
         
            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetSource()
        {
            switch (_source)
            {
                case MonaBrainTargetResultType.OnConditionTarget:
                    return _brain.State.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainTargetResultType.OnMessageSender:
                    var brain = _brain.State.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainTargetResultType.OnHitTarget:
                    return _brain.State.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
            }
            return null;
        }
    }
}