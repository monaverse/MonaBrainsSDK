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
    public class UnBindRotationYInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UnBindRotationY";
        public const string NAME = "Unbind Yaw L / R";
        public const string CATEGORY = "Rotation Bounds";
        public override Type TileType => typeof(UnBindRotationYInstructionTile);

        private IMonaBrain _brain;

        public UnBindRotationYInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.RotationBounds.y.UnBind();
            return Complete(InstructionTileResult.Success);
        }
    }
}
