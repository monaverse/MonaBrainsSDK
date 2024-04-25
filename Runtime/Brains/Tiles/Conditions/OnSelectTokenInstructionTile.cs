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

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnSelectTokenInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile
    {
        public const string ID = "OnSelectToken";
        public const string NAME = "Select Token";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OnSelectTokenInstructionTile);

        [SerializeField] private float _index = 0;
        [SerializeField] private string _indexValueName;

        [BrainProperty(true)] public float Index { get => _index; set => _index = value; }
        [BrainPropertyValueName("Index", typeof(IMonaVariablesFloatValue))] public string IndexValueName { get => _indexValueName; set => _indexValueName = value; }

        [SerializeField] private string _targetValue;
        [BrainProperty(true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        private IMonaBrain _brain;

        private bool _selectedToken;

        public OnSelectTokenInstructionTile() { }

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
            _selectedToken = false;
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

        private List<Token> _tokens;
        private async void FetchTokens()
        {
            var block = MonaGlobalBrainRunner.Instance.Blockchain;
            _tokens = await block.OwnsTokensWithObject();
            
            if (_tokens.Count > 0)
            {
                FilterAndForwardTokens(_tokens);
                _selectedToken = _tokens.Count > 0;
            }
            else
            {
                _selectedToken = false;
            }
            Debug.Log($"{nameof(OnSelectTokenInstructionTile)} {nameof(FetchTokens)} tokens: {_selectedToken}");
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
            if (!string.IsNullOrEmpty(_indexValueName))
                _index = _brain.Variables.GetFloat(_indexValueName);

            var index = (int)_index;

            Debug.Log($"{nameof(OnSelectTokenInstructionTile)} {_tokens.Count} {index}");
            if (_tokens.Count > 0 && index > -1 && index < _tokens.Count)
            {
                Debug.Log($"{nameof(OnSelectTokenInstructionTile)} {_tokens[index].Artifacts[0].Uri}");
                _brain.Variables.Set(_targetValue, _tokens[index].Artifacts[0].Uri);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}