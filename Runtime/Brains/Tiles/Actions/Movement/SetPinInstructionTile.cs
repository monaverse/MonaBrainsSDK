using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class SetPinHereInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IActionEndInstructionTile
    {
        public const string ID = "SetPinHere";
        public const string NAME = "Set Pin Here";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(SetPinHereInstructionTile);

        public SetPinHereInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            _brain.Body.SetPin();
            return Complete(InstructionTileResult.Success);
        }
    }
}