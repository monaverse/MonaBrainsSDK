using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using Mona.SDK.Core.State.Structs;
using System.Text.RegularExpressions;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class GetNumberFromVector3DataInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "GetVector3Data";
        public const string NAME = "Get Vector Data";
        public const string CATEGORY = "Numbers";

        public override Type TileType => typeof(GetNumberFromVector3DataInstructionTile);

        [SerializeField] private VectorOperation _operation = VectorOperation.Magnitude;
        [BrainPropertyEnum(true)] public VectorOperation Operation { get => _operation; set => _operation = value; }

        [SerializeField] private string _numberName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Magnitude)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Distance)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Angle)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProduct)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProductWithForward)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string NumberName { get => _numberName; set => _numberName = value; }

        [SerializeField] private string _vectorName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Direction)]
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string VectorName { get => _vectorName; set => _vectorName = value; }

        [SerializeField] private Vector3 _mainVector;
        [SerializeField] private string[] _mainVectorName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Direction)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Magnitude)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Angle)]
        [BrainProperty(true)] public Vector3 MainVector { get => _mainVector; set => _mainVector = value; }
        [BrainPropertyValueName("MainVector", typeof(IMonaVariablesVector3Value))] public string[] MainVectorName { get => _mainVectorName; set => _mainVectorName = value; }

        [SerializeField] private Vector3 _secondVector;
        [SerializeField] private string[] _secondVectorName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Direction)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Angle)]
        [BrainProperty(true)] public Vector3 SecondVector { get => _secondVector; set => _secondVector = value; }
        [BrainPropertyValueName("SecondVector", typeof(IMonaVariablesVector3Value))] public string[] SecondVectorName { get => _secondVectorName; set => _secondVectorName = value; }

        [SerializeField] private Vector3 _forwardVector;
        [SerializeField] private string[] _forwardVectorName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProductWithForward)]
        [BrainProperty(true)] public Vector3 ForwardVector { get => _forwardVector; set => _forwardVector = value; }
        [BrainPropertyValueName("ForwardVector", typeof(IMonaVariablesVector3Value))] public string[] ForwardVectorName { get => _forwardVectorName; set => _forwardVectorName = value; }

        [SerializeField] private Vector3 _positionA;
        [SerializeField] private string[] _positionAName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Distance)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProduct)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProductWithForward)]
        [BrainProperty(true)] public Vector3 PositionA { get => _positionA; set => _positionA = value; }
        [BrainPropertyValueName("PositionA", typeof(IMonaVariablesVector3Value))] public string[] PositionAName { get => _positionAName; set => _positionAName = value; }
        [SerializeField] private Vector3 _positionB;
        [SerializeField] private string[] _positionBName;
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.Distance)]
        
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProduct)]
        [BrainPropertyShow(nameof(Operation), (int)VectorOperation.DotProductWithForward)]
        [BrainProperty(true)] public Vector3 PositionB { get => _positionB; set => _positionB = value; }
        [BrainPropertyValueName("PositionB", typeof(IMonaVariablesVector3Value))] public string[] PositionBName { get => _positionBName; set => _positionBName = value; }

        [SerializeField]
        public enum VectorOperation
        {
            Magnitude = 0,
            Distance = 10,
            Direction = 15,
            Angle = 20,
            DotProduct = 30,
            DotProductWithForward = 40
        }

        protected IMonaBrain _brain;

        public GetNumberFromVector3DataInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || (string.IsNullOrEmpty(_numberName) && _operation != VectorOperation.Direction) || (string.IsNullOrEmpty(_numberName) && _operation == VectorOperation.Direction))
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (HasVector3Values(_mainVectorName))
                _mainVector = GetVector3Value(_brain, _mainVectorName);

            if (HasVector3Values(_secondVectorName))
                _secondVector = GetVector3Value(_brain, _secondVectorName);

            if (HasVector3Values(_forwardVectorName))
                _forwardVector = GetVector3Value(_brain, _forwardVectorName);

            if (HasVector3Values(_positionAName))
                _positionA = GetVector3Value(_brain, _positionAName);

            if (HasVector3Values(_positionBName))
                _positionB = GetVector3Value(_brain, _positionBName);

            switch (_operation)
            {
                case VectorOperation.Magnitude:
                    _brain.Variables.Set(_numberName, Vector3.Magnitude(_mainVector));
                    break;
                case VectorOperation.Distance:
                    _brain.Variables.Set(_numberName, Vector3.Distance(_positionA, _positionB));
                    break;
                case VectorOperation.Direction:
                    Vector3 heading = _secondVector - _mainVector;
                    float distance = heading.magnitude;
                    Vector3 direction = heading / distance;
                    _brain.Variables.Set(_vectorName, direction);
                    break;
                case VectorOperation.Angle:
                    _brain.Variables.Set(_numberName, Vector3.Angle(_mainVector, _secondVector));
                    break;
                case VectorOperation.DotProduct:
                    _brain.Variables.Set(_numberName, Vector3.Dot(_positionA, _positionB));
                    break;
                case VectorOperation.DotProductWithForward:
                    _brain.Variables.Set(_numberName, Vector3.Dot(_forwardVector, Vector3.Normalize(_positionB - _positionA)));
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}