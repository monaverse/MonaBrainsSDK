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
    public class ResumeBodyByTagInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile
    {
        public const string ID = "ResumeBodyByTag";
        public const string NAME = "Resume Body By Tag";
        public const string CATEGORY = "Pausing";
        public override Type TileType => typeof(ResumeBodyByTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        public ResumeBodyByTagInstructionTile() { }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_tag);
            for (var i = 0; i < bodies.Count; i++)
                bodies[i].Resume();
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}