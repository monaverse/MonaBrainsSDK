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
    public class UnBindRotationAllInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UnBindRotationAll";
        public const string NAME = "Unbind All Rotations";
        public const string CATEGORY = "Rotation Bounds";
        public override Type TileType => typeof(UnBindRotationAllInstructionTile);

        private IMonaBrain _brain;

        public UnBindRotationAllInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.RotationBounds.x.UnBind();
            _brain.Body.RotationBounds.y.UnBind();
            _brain.Body.RotationBounds.z.UnBind();
            return Complete(InstructionTileResult.Success);
        }
    }
}
