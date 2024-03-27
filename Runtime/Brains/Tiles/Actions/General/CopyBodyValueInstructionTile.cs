using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CopyBodyValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "CopyBodyValue";
        public const string NAME = "Copy Body Value";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CopyBodyValueInstructionTile);

        [SerializeField] private MonaBodyValueType _source = MonaBodyValueType.Position;
        [BrainPropertyEnum(true)] public MonaBodyValueType Source { get => _source; set => _source = value; }

        [SerializeField] string _targetValue;
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        private IMonaBrain _brain;

        public CopyBodyValueInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            switch(_source)
            {
                case MonaBodyValueType.StartPosition:
                    _brain.Variables.Set(_targetValue, _brain.Body.InitialLocalPosition); break;
                case MonaBodyValueType.Rotation:
                    _brain.Variables.Set(_targetValue, _brain.Body.GetRotation().eulerAngles); break;
                case MonaBodyValueType.StartRotation:
                    _brain.Variables.Set(_targetValue, _brain.Body.InitialLocalRotation.eulerAngles); break;
                case MonaBodyValueType.Scale:
                    _brain.Variables.Set(_targetValue, _brain.Body.GetScale()); break;
                case MonaBodyValueType.StartScale:
                    _brain.Variables.Set(_targetValue, _brain.Body.InitialScale); break;
                case MonaBodyValueType.Velocity:
                    _brain.Variables.Set(_targetValue, _brain.Body.GetVelocity()); break;
                default:
                    _brain.Variables.Set(_targetValue, _brain.Body.GetPosition()); break;
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}