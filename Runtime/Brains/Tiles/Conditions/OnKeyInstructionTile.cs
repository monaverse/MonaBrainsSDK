using Mona.Brains.Core;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Conditions.Interfaces;
using System;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnKeyInstructionTile : InstructionTile, IOnKeyInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnKey";
        public const string NAME = "On Key";
        public const string CATEGORY = "Condition/Input";
        public override Type TileType => typeof(OnKeyInstructionTile);

        [SerializeField]
        private KeyCode _keyCode;
        [BrainPropertyEnum(true)]
        public KeyCode KeyCode { get => _keyCode; set => _keyCode = value; }

        public OnKeyInstructionTile() { }

        public override InstructionTileResult Do()
        {
            if (Input.GetKey(_keyCode))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}