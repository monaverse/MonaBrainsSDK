using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using UnityEngine.AI;
using Unity.VisualScripting;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding
{
    [Serializable]
    public class PathFindToPositionInstructionTile : PathFindInstructionTile
    {
        public new const string ID = "PathFindToPositionInstructionTile";
        public new const string NAME = "Path Find To Position";
        public new const string CATEGORY = "Path Finding";
        public override Type TileType => typeof(PathFindToPositionInstructionTile);

        [SerializeField] private Vector3 _value;
        [SerializeField] private string[] _valueValueName = new string[4];
        [BrainProperty(true)] public Vector3 Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesVector3Value))] public string[] ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        public PathFindToPositionInstructionTile() { }

        public override InstructionTileResult Do()
        {
            if (HasVector3Values(_valueValueName))
                _value = GetVector3Value(_brain, _valueValueName);

            //Debug.Log($"{nameof(PathFindToPositionInstructionTile)} {_value} {_valueValueName}");
            if (_brain != null)
            {
                _agent.SetDestination(_value);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}