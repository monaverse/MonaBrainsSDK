using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using UnityEngine;
using System;
using Mona.Brains.Core.Brain;
using Mona.Brains.Tiles.Actions.General.Interfaces;

namespace Mona.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ChangeStateInstructionTile : InstructionTile, IChangeStateInstructionTile, IActionInstructionTile
    {
        public const string ID = "ChangeState";
        public const string NAME = "Change State";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeStateInstructionTile);

        [SerializeField]
        private string _changeState;
        [BrainProperty]
        public string State { get => _changeState; set => _changeState = value; }

        private IMonaBrain _brain;
        private string _stateProperty;

        public ChangeStateInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            _brain.BrainState = _changeState;
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}