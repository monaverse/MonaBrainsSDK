using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [Serializable]
    public class CancelForcesInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IRigidbodyInstructionTile
    {
        public const string ID = "CancelForces";
        public const string NAME = "Cancel Forces";
        public const string CATEGORY = "Forces";
        public override Type TileType => typeof(CancelForcesInstructionTile);

        public CancelForcesInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _brain.Body.CancelForces();
            
            return Complete(InstructionTileResult.Success);
        }
    }
}