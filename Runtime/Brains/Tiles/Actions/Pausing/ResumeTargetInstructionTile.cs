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
    public class ResumeTargetInstructionTile : InstructionTile, IChangeTargetInstructionTile, IActionInstructionTile
    {
        public const string ID = "ResumeTarget";
        public const string NAME = "Resume Target";
        public const string CATEGORY = "Pausing";
        public override Type TileType => typeof(ResumeTargetInstructionTile);

        [SerializeField]
        private string _target;
        [BrainProperty]
        public string Target { get => _target; set => _target = value; }

        public ResumeTargetInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            var value = _brain.State.GetValue(_target);
            if (value is IMonaStateBrainValue)
            {
                var brain = ((IMonaStateBrainValue)value).Value;
                brain.Body.Resume();
            }
            else if (value is IMonaStateBodyValue)
            {
                var body = ((IMonaStateBodyValue)value).Value;
                body.Resume();
            }
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}