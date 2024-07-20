using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnIsHostInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "OnIsHost";
        public const string NAME = "IsHost";
        public const string CATEGORY = "Multiplayer";
        public override Type TileType => typeof(OnIsHostInstructionTile);

        private IMonaBrain _brain;

        public OnIsHostInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            //if(_brain.LoggingEnabled) Debug.Log($"{nameof(OnIsHostInstructionTile)} {_brain.Body.HasControl()}", _brain.Body.Transform.gameObject);
            if (MonaGlobalBrainRunner.Instance.IsHost)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_HOST);
        }
    }
}