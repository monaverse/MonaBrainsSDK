using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class LogInstructionTile : InstructionTile, ILogInstructionTile, IActionInstructionTile
    {
        public const string ID = "Log";
        public const string NAME = "Log";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(LogInstructionTile);

        [SerializeField]
        private string _message;
        [BrainProperty]
        public string Message { get => _message; set => _message = value; }

        public LogInstructionTile() { }

        public override InstructionTileResult Do()
        {
            Debug.Log(_message);
            return Complete(InstructionTileResult.Success);
        }
    }
}