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
    public class SaveStoredDataInstructionTile : InstructionTile, IActionInstructionTile
    {
        public const string ID = "SaveStoredData";
        public const string NAME = "Save Stored Data";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(SaveStoredDataInstructionTile);

        public SaveStoredDataInstructionTile() { }

        public override InstructionTileResult Do()
        {
            PlayerPrefs.Save();
            return Complete(InstructionTileResult.Success);
        }
    }
}