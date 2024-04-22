using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Utils.Interfaces;

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

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _instruction = instruction;
        }

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
                    var dirTarget = _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                    if (dirTarget != null)
                    {
                        var dir = (dirTarget.GetPosition() - _brain.Body.GetPosition()).normalized;
                        _brain.Variables.Set(_targetValue, (Vector3)dir); break;
                    }
                    else
                    {
                        _brain.Variables.Set(_targetValue, Vector3.zero); break;
                    }
                case MonaBrainResultType.LastMoveDirection:
                    _brain.Variables.Set(_targetValue, _brain.Variables.GetInternalVector3(MonaBrainConstants.LAST_MOVE_DIRECTION));
                    break;
                case MonaBrainResultType.LastTokenAvatar:
                    if (_instruction.Tokens.Count > 0)
                    {
                        for (var i = 0; i < _instruction.Tokens.Count; i++)
                        {
                            var token = _instruction.Tokens[i];
                            var index = token.Artifacts.FindIndex(x => x.AssetType == TokenAssetType.Avatar);
                            if (index > -1)
                                _brain.Variables.Set(_targetValue, token.Artifacts[index].Uri);
                        }
                    }
                    break;
                case MonaBrainResultType.LastTokenObject:
                    if (_instruction.Tokens.Count > 0)
                    {
                        for (var i = 0; i < _instruction.Tokens.Count; i++)
                        {
                            var token = _instruction.Tokens[i];
                            var index = token.Artifacts.FindIndex(x => x.AssetType == TokenAssetType.Object);
                            if (index > -1)
                                _brain.Variables.Set(_targetValue, token.Artifacts[index].Uri);
                        }
                    }
                    break;
                case MonaBrainResultType.LastTokenTexture:
                    if (_instruction.Tokens.Count > 0)
                    {
                        for (var i = 0; i < _instruction.Tokens.Count; i++)
                        {
                            var token = _instruction.Tokens[i];
                            var index = token.Artifacts.FindIndex(x => x.AssetType == TokenAssetType.Texture);
                            if (index > -1)
                                _brain.Variables.Set(_targetValue, token.Artifacts[index].Uri);
                        }
                    }
                    break;
                default: break;
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}