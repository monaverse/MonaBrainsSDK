using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class DeleteAllDataInstructionTile : InstructionTile, IActionInstructionTile
    {
        public const string ID = "DeleteAllData";
        public const string NAME = "Delete All Data";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(DeleteAllDataInstructionTile);

        public DeleteAllDataInstructionTile() { }

        public override InstructionTileResult Do()
        {
            PlayerPrefs.DeleteAll();
            return Complete(InstructionTileResult.Success);
        }
    }
}