using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
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
        [BrainPropertyValueName("Source", typeof(IMonaVariablesBrainValue))] public string Target { get => _target; set => _target = value; }

        public override PushDirectionType DirectionType => PushDirectionType.Pull;

        protected override IMonaBody GetTarget()
        {
            IMonaBody source = GetSource();
            if (!string.IsNullOrEmpty(_target))
            {
                var variable = _brain.Variables.GetVariable(_target);
                if (variable is IMonaVariablesBrainValue)
                    source = ((IMonaVariablesBrainValue)variable).Value.Body;
                else if (variable is IMonaVariablesBodyValue)
                    source = ((IMonaVariablesBodyValue)variable).Value;
            }
            _bodiesToControl.Clear();
            _bodiesToControl.Add(source);
            return source;
        }

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();

        protected override bool ApplyForceToTarget()
        {
            return true;
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
    }
}