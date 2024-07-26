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
    public class OnInputMonaInstructionTile : OnInputGeneralInstructionTile
    {
        public const string ID = "OnInputMona";
        public const string NAME = "On Mona Input";
        public const string CATEGORY = "Monaverse";
        public override Type TileType => typeof(OnInputMonaInstructionTile);

        [SerializeField]
        private MonaInputType _monaInputType = MonaInputType.Action;
        [BrainPropertyEnum(true)]
        public override MonaInputType InputType { get => _monaInputType; set => _monaInputType = value; }
    }
}