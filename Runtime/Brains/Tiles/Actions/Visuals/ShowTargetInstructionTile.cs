using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ShowTargetInstructionTile : InstructionTile, IChangeTargetInstructionTile, IActionInstructionTile
    {
        public const string ID = "ShowTarget";
        public const string NAME = "Show Target";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ShowTargetInstructionTile);

        [SerializeField]
        private string _target;
        [BrainProperty]
        public string Target { get => _target; set => _target = value; }

        public ShowTargetInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            var value = _brain.State.GetValue(_target);
            if(value is IMonaStateBrainValue)
            {
                var brain = ((IMonaStateBrainValue)value).Value;
                brain.Body.SetVisible(true);
            }
            else if(value is IMonaStateBodyValue)
            {
                var body = ((IMonaStateBodyValue)value).Value;
                body.SetVisible(true);
            }
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}