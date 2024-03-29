using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CopyResultInstructionTile : InstructionTile, ICopyResultInstructionTile, IActionInstructionTile
    {
        public const string ID = "CopyResult";
        public const string NAME = "Copy Result";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CopyResultInstructionTile);

        [SerializeField] private MonaBrainResultType _source = MonaBrainResultType.OnConditionTarget;
        [BrainProperty(true)] public MonaBrainResultType Source { get => _source; set => _source = value; }

        [SerializeField] string _targetValue;
        [BrainProperty(true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        private IMonaBrain _brain;

        public CopyResultInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            switch(_source)
            {
                case MonaBrainResultType.OnConditionTarget:
                    _brain.Variables.Set(_targetValue, _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET)); break;
                case MonaBrainResultType.OnMessageSender:
                    _brain.Variables.Set(_targetValue, _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER)); break;
                case MonaBrainResultType.OnInputMoveDirection:
                    _brain.Variables.Set(_targetValue, (Vector2)_brain.Variables.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION)); break;
                case MonaBrainResultType.OnInputMouseDirection:
                    _brain.Variables.Set(_targetValue, (Vector2)_brain.Variables.GetVector2(MonaBrainConstants.RESULT_MOUSE_DIRECTION)); break;
                case MonaBrainResultType.OnHitTarget:
                    _brain.Variables.Set(_targetValue, _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET)); break;
                case MonaBrainResultType.OnHitTargetPosition:
                    var target = _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                    if(target != null)
                        _brain.Variables.Set(_targetValue, target.GetPosition()); break;
                case MonaBrainResultType.OnHitPoint:
                    _brain.Variables.Set(_targetValue, (Vector3)_brain.Variables.GetVector3(MonaBrainConstants.RESULT_HIT_POINT)); break;
                case MonaBrainResultType.OnHitNormal:
                    _brain.Variables.Set(_targetValue, (Vector3)_brain.Variables.GetVector3(MonaBrainConstants.RESULT_HIT_NORMAL)); break;
                case MonaBrainResultType.DirectionToTarget:
                    _brain.Variables.Set(_targetValue, (Vector3)_brain.Variables.GetVector3(MonaBrainConstants.RESULT_DIRECTION_TO_TARGET)); break;
                default: break;
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}