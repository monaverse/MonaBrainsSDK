﻿using Mona.SDK.Brains.Core.Enums;
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
    public class DisablePartInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "DisablePart";
        public const string NAME = "Disable Part";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(DisablePartInstructionTile);

        [SerializeField]
        private bool _all;
        public bool All { get => _all; set => _all = value; }

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }


        public DisablePartInstructionTile() { }

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
                    bodies[i].SetActive(false);
            }
            else
            {
                var bodies = _brain.Body.FindChildrenByTag(_tag);
                for (var i = 0; i < bodies.Count; i++)
                    bodies[i].SetActive(false);
            }
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}