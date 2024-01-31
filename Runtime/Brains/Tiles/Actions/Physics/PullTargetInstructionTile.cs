using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class PullTargetInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "PullTarget";
        public const string NAME = "Pull Target";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(PullTargetInstructionTile);

        [SerializeField] private MonaBrainTargetResultType _source = MonaBrainTargetResultType.OnConditionTarget;
        [SerializeField] private string _target;

        [BrainProperty(true)] public MonaBrainTargetResultType Source { get => _source; set => _source = value; }
        [BrainPropertyValueName("Source")] public string Target { get => _target; set => _target = value; }

        public override PushDirectionType DirectionType => PushDirectionType.Pull;

        protected override IMonaBody GetTarget()
        {
            IMonaBody source = GetSource();
            if (!string.IsNullOrEmpty(_target))
            {
                var value = _brain.State.GetValue(_target);
                if (value is IMonaStateBrainValue)
                    source = ((IMonaStateBrainValue)value).Value.Body;
                else if (value is IMonaStateBodyValue)
                    source = ((IMonaStateBodyValue)value).Value;
            }
            return source;
        }

        protected override bool ApplyForceToTarget()
        {
            return true;
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