using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnInputMoveInstructionTile : OnInputGeneralInstructionTile
    {
        public const string ID = "OnInputMove";
        public const string NAME = "On Move Input";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnInputMoveInstructionTile);

        public override MonaInputType InputType => MonaInputType.Move;
    }
}