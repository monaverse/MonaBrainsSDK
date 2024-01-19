using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
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