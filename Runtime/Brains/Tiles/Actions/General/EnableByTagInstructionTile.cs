using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class EnableByTagInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile
    {
        public const string ID = "EnableByTag";
        public const string NAME = "Enable By Tag";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(EnableByTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        public EnableByTagInstructionTile() { }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_tag);
            for (var i = 0; i < bodies.Count; i++)
                bodies[i].SetActive(true);
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}