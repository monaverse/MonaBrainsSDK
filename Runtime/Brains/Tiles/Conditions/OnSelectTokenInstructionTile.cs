﻿using UnityEngine;
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
    public enum SelectTokenCopyType
    {
        None = 0,
        AvatarUrl = 1,
        TraitValue = 2
    }

    [Serializable]
    public class OnSelectTokenInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile, IBlockchainTokenFilterInstructionTile
    {
        public const string ID = "OnSelectToken";
        public const string NAME = "Select Token";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OnSelectTokenInstructionTile);

        private IMonaBrain _brain;

        private bool _selectedToken;
        private Token _token;

        public OnSelectTokenInstructionTile() { }

        private Action<MonaWalletConnectedEvent> OnWalletConnected;
        private Action<MonaWalletConnectedEvent> OnWalletDisconnected;
        private Action<MonaWalletTokenSelectedEvent> OnTokenSelected;

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
                    MonaEventBus.Register<MonaWalletConnectedEvent>(new EventHook(MonaBrainConstants.WALLET_DISCONNECTED_EVENT), OnWalletDisconnected);

                    OnTokenSelected = HandleTokenSelected;
                    MonaEventBus.Register<MonaWalletTokenSelectedEvent>(new EventHook(MonaBrainConstants.WALLET_TOKEN_SELECTED_EVENT), OnTokenSelected);

                }
            }
        }

        private void HandleWalletConnected(MonaWalletConnectedEvent evt)
        {
            _selectedToken = false;
        }

        private void HandleWalletDisconneccted(MonaWalletConnectedEvent evt)
        {
            _selectedToken = false;
        }

        private void HandleTokenSelected(MonaWalletTokenSelectedEvent evt)
        {
            _selectedToken = true;
            _token = evt.Token;

            FilterAndForwardTokens(evt.Token);
                
            //Debug.Log($"{nameof(OnSelectTokenInstructionTile)} {nameof(HandleTokenSelected)} token: {evt.Token}");
            TriggerRefresh();
        }

        public override void Unload(bool destroy = false)
        {
            if(destroy)
            {
                MonaEventBus.Unregister(new EventHook(MonaBrainConstants.WALLET_CONNECTED_EVENT), OnWalletConnected);
                MonaEventBus.Unregister(new EventHook(MonaBrainConstants.WALLET_DISCONNECTED_EVENT), OnWalletDisconnected);
                MonaEventBus.Unregister(new EventHook(MonaBrainConstants.WALLET_TOKEN_SELECTED_EVENT), OnTokenSelected);
            }
        }

        private void TriggerRefresh()
        {
            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Blockchain, _instruction));
        }

        private List<Token> FilterAndForwardTokens(Token token)
        {
            IBlockchainTokenFilterInstructionTile firstFilter = null;
            for (var i = 0; i < _instruction.BlockchainTiles.Count; i++)
            {
                if (_instruction.BlockchainTiles[i] is IBlockchainTokenFilterInstructionTile)
                {
                    firstFilter = (IBlockchainTokenFilterInstructionTile)_instruction.BlockchainTiles[i];
                    break;
                }
            }

            if (firstFilter == this)
                _instruction.Tokens.Clear();

            if (_instruction.Tokens.Count == 0)
            {
                if (firstFilter == this)
                {
                    _instruction.Tokens.Add(token);
                }
            }
            else
            {
                var filtered = _instruction.Tokens.FindAll(x =>
                {
                    return x.Equals(token);
                });
                _instruction.Tokens = filtered;
            }

            //Debug.Log($"{nameof(OnSelectInstructionTile)} tokens: {_instruction.Tokens.Count}");
            return _instruction.Tokens;
        }

        public override InstructionTileResult Do()
        {
            if (_selectedToken)
            {
                _selectedToken = false;
                //Debug.Log($"{nameof(OnSelectTokenInstructionTile)}");
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}