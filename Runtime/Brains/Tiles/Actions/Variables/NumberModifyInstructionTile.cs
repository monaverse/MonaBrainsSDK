using System;
using UnityEngine;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class NumberModifyInstructionTile : SetNumberToInstructionTile
    {
        public new const string ID = "Modify Number";
        public new const string NAME = "Modify Number";
        public new const string CATEGORY = "Numbers";
        public override Type TileType => typeof(NumberModifyInstructionTile);

        [SerializeField] private ValueChangeType _operation;
        [BrainPropertyEnum(true)] public ValueChangeType Operation { get => _operation; set => _operation = value; }

        public NumberModifyInstructionTile() { }

        protected override ValueChangeType GetOperator()
        {
            return _operation;
        }
    }
}