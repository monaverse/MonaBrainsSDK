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
    public class DisableByTagInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "DisableByTag";
        public const string NAME = "Disable By Tag";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(DisableByTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        public DisableByTagInstructionTile() { }

        private IMonaBrain _brain;
        
        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBodyFactory.FindByTag(_tag);
            if(_brain.LoggingEnabled)
                Debug.Log($"{nameof(DisableByTagInstructionTile)} tag: {_tag}, bodies: {bodies.Count}");

            for (var i = 0; i < bodies.Count; i++)
            {
                bodies[i].SetActive(false);
                if (_brain.LoggingEnabled)
                    Debug.Log($"{nameof(DisableByTagInstructionTile)} {bodies[i].ActiveTransform.name}", bodies[i].ActiveTransform.gameObject);
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}