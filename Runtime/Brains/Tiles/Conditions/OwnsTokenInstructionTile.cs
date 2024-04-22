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
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Events;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OwnsTokenInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile
    {
        public const string ID = "OwnsToken";
        public const string NAME = "Owns Token";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OwnsTokenInstructionTile);

        [SerializeField] private string _contractAddress;
        [SerializeField] private string _contractAddressValueName;
        [BrainProperty(true)] public string Contract { get => _contractAddress; set => _contractAddress = value; }
        [BrainPropertyValueName("Contract", typeof(IMonaVariablesStringValue))] public string ContractAddressValueName { get => _contractAddressValueName; set => _contractAddressValueName = value; }

        [SerializeField] private string _tokenId;
        [SerializeField] private string _tokenIdValueName;
        [BrainProperty(true)] public string TokenId { get => _tokenId; set => _tokenId = value; }
        [BrainPropertyValueName("TokenId", typeof(IMonaVariablesStringValue))] public string TokenIdValueName { get => _tokenIdValueName; set => _tokenIdValueName = value; }


        private IMonaBrain _brain;

        private bool _ownsToken;

        public OwnsTokenInstructionTile() { }

        private Action<MonaWalletConnectedEvent> OnWalletConnected;

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            if (MonaGlobalBrainRunner.Instance.Blockchain != null)
            {
                if (OnWalletConnected == null)
                {
                    OnWalletConnected = HandleWalletConnected;
                    EventBus.Register<MonaWalletConnectedEvent>(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), OnWalletConnected);
                }

                FetchTokens();
            }
        }

        private void HandleWalletConnected(MonaWalletConnectedEvent evt)
        {
            FetchTokens();
        }

        public override void Unload(bool destroy = false)
        {
            if(destroy)
            {
                EventBus.Unregister(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), OnWalletConnected);
            }
        }

        private async void FetchTokens()
        {
            if (!string.IsNullOrEmpty(_contractAddressValueName))
                _contractAddress = _brain.Variables.GetString(_contractAddressValueName);

            if (!string.IsNullOrEmpty(_tokenIdValueName))
                _tokenId = _brain.Variables.GetString(_tokenIdValueName);

            var block = MonaGlobalBrainRunner.Instance.Blockchain;
            Token token = await block.OwnsToken(_contractAddress, _tokenId);
            if (!token.Equals((Token)default))
            {
                var tokens = FilterAndForwardTokens(token);
                _ownsToken = tokens.Count > 0;
            }
            else
            {
                _ownsToken = false;
            }
            Debug.Log($"{nameof(OwnsTokenInstructionTile)} {nameof(FetchTokens)} tokens: {_ownsToken}");
            EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Blockchain, _instruction));
        }

        private List<Token> FilterAndForwardTokens(Token token)
        {
            if (_instruction.BlockchainTiles[0] == this)
                _instruction.Tokens.Clear();

            if (_instruction.Tokens.Count == 0)
            {
                if (_instruction.BlockchainTiles[0] == this)
                {
                    _instruction.Tokens.Add(token);
                }
            }
            else
            {
                var filtered = _instruction.Tokens.FindAll(x => x.Equals(token));
                _instruction.Tokens = filtered;

            }
            return _instruction.Tokens;
        }

        public override InstructionTileResult Do()
        {
            Debug.Log($"{nameof(OwnsTokenInstructionTile)} {_ownsToken}");
            if(_ownsToken)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}