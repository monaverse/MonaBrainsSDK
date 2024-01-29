using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class RemoveTagInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "RemoveTag";
        public const string NAME = "Remove Tag";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(RemoveTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        private IMonaBrain _brain;
        private string _stateProperty;

        public RemoveTagInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            _brain.RemoveTag(_tag);
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}