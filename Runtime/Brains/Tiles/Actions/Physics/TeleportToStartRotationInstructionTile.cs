using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class TeleportToStartRotationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "TeleportToStartRotationInstructionTile";
        public const string NAME = "Teleport To Start Rotation";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(TeleportToStartRotationInstructionTile);

        private IMonaBrain _brain;

        public TeleportToStartRotationInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        } 

        public override InstructionTileResult Do()
        {
            if (_brain != null)
            {
                _brain.Body.OnTeleported -= HandleTeleported;
                _brain.Body.OnTeleported += HandleTeleported;
                _brain.Body.TeleportRotation(_brain.Body.InitialRotation, true);
                return _brain.Body.ActiveRigidbody != null ? Complete(InstructionTileResult.Running) : Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private void HandleTeleported()
        {
            //Debug.Log($"{nameof(HandleTeleported)} ", _brain.Body.Transform.gameObject);
            _brain.Body.OnTeleported -= HandleTeleported;
            Complete(InstructionTileResult.Success, true);
        }

    }
}