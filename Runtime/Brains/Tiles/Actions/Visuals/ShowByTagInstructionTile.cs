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
    public class ShowByTagInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile
    {
        public const string ID = "ShowByTag";
        public const string NAME = "Show By Tag";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ShowByTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        public ShowByTagInstructionTile() { }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBodyFactory.FindByTag(_tag);
            for (var i = 0; i < bodies.Count; i++)
                bodies[i].SetVisible(true);
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}