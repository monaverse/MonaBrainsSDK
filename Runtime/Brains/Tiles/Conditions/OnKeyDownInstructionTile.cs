using Mona.Brains.Core;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Conditions.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnKeyDownInstructionTile : InstructionTile, IOnKeyDownInstructionTile, IConditionInstructionTile, IStartableInstructionTile
    {
        public const string ID = "OnKeyDown";
        public const string NAME = "On Key\nDown";
        public const string CATEGORY = "Condition/Input";
        public override Type TileType => typeof(OnKeyDownInstructionTile);

        [SerializeField]
        private KeyCode _keyCode;
        [BrainProperty]
        public KeyCode KeyCode { get => _keyCode; set => _keyCode = value; }

        private bool _wasDown;
        public OnKeyDownInstructionTile() { }
        
        public override InstructionTileResult Do()
        {
            if (Input.GetKey(_keyCode) && !_wasDown)
            {
                _wasDown = true;
                return Complete(InstructionTileResult.Success);
            }
            else if(!Input.GetKey(_keyCode) && _wasDown)
            {
                _wasDown = false;
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}