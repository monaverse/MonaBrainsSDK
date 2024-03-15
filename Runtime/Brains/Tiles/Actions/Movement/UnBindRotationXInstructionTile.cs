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
    public class UnBindRotationXInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UnBindRotationX";
        public const string NAME = "Unbind Pitch Up / Down";
        public const string CATEGORY = "Rotation Bounds";
        public override Type TileType => typeof(UnBindRotationXInstructionTile);

        private IMonaBrain _brain;

        public UnBindRotationXInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.RotationBounds.x.UnBind();
            return Complete(InstructionTileResult.Success);
        }
    }
}
