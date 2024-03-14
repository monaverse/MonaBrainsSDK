using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class UnBindRadiusInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UnBindRadius";
        public const string NAME = "Unbind Radius";
        public const string CATEGORY = "Position Bounds";
        public override Type TileType => typeof(UnBindRadiusInstructionTile);

        private IMonaBrain _brain;

        public UnBindRadiusInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.PositionBounds.radius.UnBind();
            return Complete(InstructionTileResult.Success);
        }
    }
}
