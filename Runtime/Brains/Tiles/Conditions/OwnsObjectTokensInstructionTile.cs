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
using Mona.SDK.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OwnsObjectTokensInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile
    {
        public const string ID = "OwnsObjectTokens";
        public const string NAME = "Owns Artifact Tokens";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OwnsObjectTokensInstructionTile);

        private IMonaBrain _brain;

        private bool _OwnsObjectTokens;

        public OwnsObjectTokensInstructionTile() { }

        private Action<MonaWalletConnectedEvent> OnWalletConnected;
        private Action<MonaWalletConnectedEvent> OnWalletDisconnected;

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            if (MonaGlobalBrainRunner.Instance.Blockchain != null)
            {
                if (OnWalletConnected == null)
                {
                    OnWalletConnected = HandleWalletConnected;
                    MonaEventBus.Register<MonaWalletConnectedEvent>(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), OnWalletConnected);

                    OnWalletDisconnected = HandleWalletDisconneccted;
                    MonaEventBus.Register<MonaWalletConnectedEvent>(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), OnWalletDisconnected);
                }

                FetchTokens();
            }
        }

        private void HandleWalletConnected(MonaWalletConnectedEvent evt)
        {
            FetchTokens();
        }

        private void HandleWalletDisconneccted(MonaWalletConnectedEvent evt)
        {
            _OwnsObjectTokens = false;
            TriggerRefresh();
        }

        public override void Unload(bool destroy = false)
        {
            if(destroy)
            {
                MonaEventBus.Unregister(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), OnWalletConnected);
                MonaEventBus.Unregister(new EventHook(MonaBrainConstants.WALLET_DISCONNECTED_EVENT), OnWalletDisconnected);
            }
        }

        private async void FetchTokens()
        {
            var block = MonaGlobalBrainRunner.Instance.Blockchain;
            List<Token> tokens = await block.OwnsTokensWithArtifact();

            if (tokens.Count > 0)
            {
                FilterAndForwardTokens(tokens);
                _OwnsObjectTokens = tokens.Count > 0;
            }
            else
            {
                _OwnsObjectTokens = false;
            }
            Debug.Log($"{nameof(OwnsObjectTokensInstructionTile)} {nameof(FetchTokens)} tokens: {_OwnsObjectTokens}");
            TriggerRefresh();
        }

        private void TriggerRefresh()
        {

            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Blockchain, _instruction));

        }

        private List<Token> FilterAndForwardTokens(List<Token> tokens)
        {
            if (_instruction.BlockchainTiles[0] == this)
                _instruction.Tokens.Clear();

            if (_instruction.Tokens.Count == 0)
            {
                if (_instruction.BlockchainTiles[0] == this)
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

        public override InstructionTileResult Do()
        {
            Debug.Log($"{nameof(OwnsObjectTokensInstructionTile)} {_OwnsObjectTokens}");
            if(_OwnsObjectTokens)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}