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
    public class UnBindPositionXInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UnBindPositionX";
        public const string NAME = "Unbind East / West";
        public const string CATEGORY = "Position Bounds";
        public override Type TileType => typeof(UnBindPositionXInstructionTile);

        private IMonaBrain _brain;

        public UnBindPositionXInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.PositionBounds.x.UnBind();
            return Complete(InstructionTileResult.Success);
        }
    }
}
