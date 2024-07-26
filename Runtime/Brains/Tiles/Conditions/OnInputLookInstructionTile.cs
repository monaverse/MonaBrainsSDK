using System;
using Mona.SDK.Core.Input.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnInputLookInstructionTile : OnInputGeneralInstructionTile
    {
        public const string ID = "OnInputLook";
        public const string NAME = "On Look Input";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnInputLookInstructionTile);

        public override MonaInputType InputType => MonaInputType.Look;
    }
}