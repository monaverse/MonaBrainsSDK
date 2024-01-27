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
    public class EnableTargetInstructionTile : InstructionTile, IChangeTargetInstructionTile, IActionInstructionTile
    {
        public const string ID = "EnableTarget";
        public const string NAME = "Enable Target";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(EnableTargetInstructionTile);

        [SerializeField]
        private string _target;
        [BrainProperty]
        public string Target { get => _target; set => _target = value; }

        public EnableTargetInstructionTile() { }

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
                brain.Body.SetActive(true);
            }
            else if(value is IMonaStateBodyValue)
            {
                var body = ((IMonaStateBodyValue)value).Value;
                body.SetActive(true);
            }
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}