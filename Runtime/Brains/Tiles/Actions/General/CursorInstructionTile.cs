using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CursorInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "Cursor";
        public const string NAME = "Cursor";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CursorInstructionTile);

        [SerializeField] private CursorLockMode _cursorLockMode;
        [BrainPropertyEnum(true)] public CursorLockMode CursorLockMode { get => _cursorLockMode; set => _cursorLockMode = value; }

        public CursorInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            Cursor.lockState = _cursorLockMode;
            return Complete(InstructionTileResult.Success);
        }
    }
}