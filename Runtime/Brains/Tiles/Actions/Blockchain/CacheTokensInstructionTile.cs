using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Blockchain
{
    [Serializable]
    public class CacheTokensInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IActionInstructionTile, IActionStateEndInstructionTile, 
        INeedAuthorityInstructionTile
    {
        public const string ID = "CacheTokens";
        public const string NAME = "Cache Tokens";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(CacheTokensInstructionTile);

        [SerializeField]
        private float _poolSize = 1;
        [BrainProperty(true)] public float PoolSize { get => _poolSize; set => _poolSize = value; }

        private IMonaBrain _brain;

        private BrainsGlbLoader _loader;

        public CacheTokensInstructionTile() { }

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _instruction = instruction;
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            if(_loader == null)
            {
                var go = new GameObject("CacheTokensGlbLoader");
                _loader = go.AddComponent<BrainsGlbLoader>();
            }
            if(_instruction.Tokens.Count > 0)
            {
                _loader.CacheTokens(_instruction.Tokens, () => {
                    Complete(InstructionTileResult.Success, true);
                    return;
                },
                (int)_poolSize);
                return Complete(InstructionTileResult.Running);
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}