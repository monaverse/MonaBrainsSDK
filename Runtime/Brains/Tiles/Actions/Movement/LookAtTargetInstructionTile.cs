using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class LookAtTargetInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "LookAtTarget";
        public const string NAME = "Look At Target";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(LookAtTargetInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.None;

        [SerializeField] private MonaBrainTargetResultType _source = MonaBrainTargetResultType.OnConditionTarget;
        [SerializeField] private string _target;

        [BrainProperty(true)] public MonaBrainTargetResultType Source { get => _source; set => _source = value; }
        [BrainPropertyValueName("Source", typeof(IMonaVariablesBodyValue))] public string Target { get => _target; set => _target = value; }

        [BrainProperty(false)] public bool LookStraightAhead { get => _lookStraightAhead; set => _lookStraightAhead = value; }

        private IMonaBody GetTarget()
        {
            var body = GetSource();
            if (!string.IsNullOrEmpty(_target))
            {
                var variable = _brain.Variables.GetVariable(_target);
                if (variable is IMonaVariablesBrainValue)
                    body = ((IMonaVariablesBrainValue)variable).Value.Body;
                else if (variable is IMonaVariablesBodyValue)
                    body = ((IMonaVariablesBodyValue)variable).Value;
            }
            return body;
        }

        private IMonaBody GetSource()
        {
            switch (_source)
            {
                case MonaBrainTargetResultType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainTargetResultType.OnMessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainTargetResultType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
            }
            return null;
        }

        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff)
        {
            IMonaBody body = GetTarget();
            if (body != null)
            {
                var fwd = body.GetPosition() - _brain.Body.GetPosition();
                if (_lookStraightAhead)
                    fwd.y = 0;
                return Quaternion.LookRotation(fwd, Vector3.up);
            }
            else
            {
                return _brain.Body.GetRotation();
            }            
        }
    }
}