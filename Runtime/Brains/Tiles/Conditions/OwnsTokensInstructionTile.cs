using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OwnsTokensInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile
    {
        public const string ID = "OwnsTokens";
        public const string NAME = "Owns Tokens";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OwnsTokensInstructionTile);

        [SerializeField] private string _contractAddress;
        [SerializeField] private string _contractAddressValueName;
        [BrainProperty(true)] public string Contract { get => _contractAddress; set => _contractAddress = value; }
        [BrainPropertyValueName("Contract", typeof(IMonaVariablesStringValue))] public string ContractAddressValueName { get => _contractAddressValueName; set => _contractAddressValueName = value; }

        private IMonaBrain _brain;
        private IInstruction _instruction;

        public OwnsTokensInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;
            if (MonaGlobalBrainRunner.Instance.Blockchain != null)
            {
                var block = MonaGlobalBrainRunner.Instance.Blockchain;
                block.RegisterContract(_contractAddress);
            }
        }

        public override InstructionTileResult Do()
        {
            if(MonaGlobalBrainRunner.Instance.Blockchain != null)
            {
                var block = MonaGlobalBrainRunner.Instance.Blockchain;
                var tokens = FilterAndForwardTokens(block.OwnsTokens(_contractAddress));
                if(tokens.Count > 0)
                {
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private List<Token> FilterAndForwardTokens(List<Token> tokens)
        {
            if (_instruction.InstructionTiles[0] == this)
                _instruction.Tokens.Clear();

            if (_instruction.Tokens.Count == 0)
            {
                if (_instruction.InstructionTiles[0] == this)
                {
                    _instruction.Tokens.AddRange(tokens);
                }
            }
            else
            {
                var filtered = _instruction.Tokens.FindAll(x => tokens.Contains(x));
                _instruction.Tokens = filtered;

            }
            return _instruction.Tokens;
        }
    }
}