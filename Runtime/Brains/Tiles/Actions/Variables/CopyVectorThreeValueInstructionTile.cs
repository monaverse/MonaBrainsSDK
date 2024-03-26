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
    public class CopyVectorThreeValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "CopyVectorThreeValue";
        public const string NAME = "Copy Vector3 Value";
        public const string CATEGORY = "Numbers";
        public override Type TileType => typeof(CopyVectorThreeValueInstructionTile);
        
        [SerializeField] string _vector;
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string Vector { get => _vector; set => _vector = value; }

        [SerializeField] private VectorThreeAxis _axis = VectorThreeAxis.Y;
        [BrainPropertyEnum(true)] public VectorThreeAxis Axis { get => _axis; set => _axis = value; }

        [SerializeField] private string _targetNumber;
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string TargetNumber { get => _targetNumber; set => _targetNumber = value; }

        private IMonaBrain _brain;

        public CopyVectorThreeValueInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (string.IsNullOrEmpty(_vector) || string.IsNullOrEmpty(_targetNumber))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var vectorVariable = (IMonaVariablesVector3Value)_brain.Variables.GetVariable(_vector);

            switch (_axis)
            {
                case VectorThreeAxis.X:
                    _brain.Variables.Set(_targetNumber, vectorVariable.Value.x); break;
                case VectorThreeAxis.Y:
                    _brain.Variables.Set(_targetNumber, vectorVariable.Value.y); break;
                case VectorThreeAxis.Z:
                    _brain.Variables.Set(_targetNumber, vectorVariable.Value.z); break;
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}