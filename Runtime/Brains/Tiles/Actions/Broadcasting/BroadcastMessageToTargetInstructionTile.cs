using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using System;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageToTargetInstructionTile : BroadcastMessageInstructionTile, IBroadcastMessageToTargetInstructionTile
    {
        public const string ID = "BroadcastMessageToTarget";
        public const string NAME = "Broadcast Message\n To Target";
        public const string CATEGORY = "Action/Broadcasting";
        public override Type TileType => typeof(BroadcastMessageToTargetInstructionTile);

        [SerializeField] private string _message;
        [BrainProperty(true)] public string Message { get => _message; set => _message = value; }

        [SerializeField] private MonaBrainTargetResultType _source = MonaBrainTargetResultType.OnConditionTarget;
        [SerializeField] private string _targetValue;

        [BrainProperty(true)] public MonaBrainTargetResultType Source { get => _source; set => _source = value; }
        [BrainPropertyValueName("Source")] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        private IMonaBrain _brain;

        public BroadcastMessageToTargetInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            IMonaBody source = GetSource();
            if (!string.IsNullOrEmpty(_targetValue))
            {
                var value = _brain.State.GetValue(_targetValue);
                if (value is IMonaStateBrainValue)
                {
                    var targetBrain = ((IMonaStateBrainValue)value).Value;
                    if (targetBrain != null)
                        BroadcastMessage(_brain, _message, targetBrain);
                }
                else
                {
                    var targetBody = ((IMonaStateBodyValue)value).Value;
                    if (targetBody != null)
                        BroadcastMessage(_brain, _message, targetBody);
                }
            }
            else
            {
                if (source != null)
                    BroadcastMessage(_brain, _message, source);
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