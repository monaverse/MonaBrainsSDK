using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ShowInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "Show";
        public const string NAME = "Show";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ShowInstructionTile);

        private IMonaBrain _brain;

        public ShowInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            _brain.Body.SetVisible(true);
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}