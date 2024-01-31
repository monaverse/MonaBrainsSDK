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
    public class HidePartInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "HidePart";
        public const string NAME = "Hide Part";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(HidePartInstructionTile);

        [SerializeField]
        private bool _all;
        public bool All { get => _all; set => _all = value; }

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        public HidePartInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (_all)
            {
                var bodies = _brain.Body.Children();
                for (var i = 0; i < bodies.Count; i++)
                    bodies[i].SetVisible(false);
            }
            else
            {
                var bodies = _brain.Body.FindChildrenByTag(_tag);
                for (var i = 0; i < bodies.Count; i++)
                    bodies[i].SetVisible(false);
            }
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}