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
    public class OwnsTokensInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile
    {
        public const string ID = "OwnsTokens";
        public const string NAME = "Owns Tokens";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OwnsTokensInstructionTile);

        [SerializeField] private bool _ownsTokens = true;
        [SerializeField] private string _ownsTokensName;
        [BrainProperty(true)] public bool OwnsTokens { get => _ownsTokens; set => _ownsTokens = value; }
        [BrainPropertyValueName("OwnsTokens", typeof(IMonaVariablesBoolValue))] public string OwnsTokensName { get => _ownsTokensName; set => _ownsTokensName = value; }

        [SerializeField] private MonaBrainTokenFilterType _tokenFilter = MonaBrainTokenFilterType.IncludeAll;
        [BrainProperty(true)] public MonaBrainTokenFilterType TokenFilter { get => _tokenFilter; set => _tokenFilter = value; }

        [SerializeField] private MonaBrainTraitFilterType _traitFilter = MonaBrainTraitFilterType.None;
        [BrainProperty(true)] public MonaBrainTraitFilterType TraitFilter { get => _traitFilter; set => _traitFilter = value; }

        [SerializeField] protected string _traitName;
        [BrainPropertyShow(nameof(TraitFilter), (int)MonaBrainTraitFilterType.Include)]
        [BrainPropertyShow(nameof(TraitFilter), (int)MonaBrainTraitFilterType.Exclude)]
        [BrainProperty(true)] public string TraitName { get => _traitName; set => _traitName = value; }

        [SerializeField] protected string _traitValue;
        [SerializeField] protected string _traitValueName;
        [BrainPropertyShow(nameof(TraitFilter), (int)MonaBrainTraitFilterType.Include)]
        [BrainPropertyShow(nameof(TraitFilter), (int)MonaBrainTraitFilterType.Exclude)]
        [BrainProperty(true)] public string TraitValue { get => _traitValue; set => _traitValue = value; }
        [BrainPropertyValueName(nameof(TraitValue), typeof(IMonaVariablesStringValue))] public string TraitValueName { get => _traitValueName; set => _traitValueName = value; }


        private IMonaBrain _brain;

        private bool _TokensFound;

        public OwnsTokensInstructionTile() { }

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
            _TokensFound = false;
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
            List<Token> tokens = await block.OwnsTokensWithObject();

            if (tokens.Count > 0)
            {
                if (!string.IsNullOrEmpty(_traitValueName))
                    _traitValue = _brain.Variables.GetString(_traitValueName);

                if (_traitFilter == MonaBrainTraitFilterType.Include)
                {
                    tokens = tokens.FindAll(x =>
                    {
                        if (x.Traits.ContainsKey(_traitName.ToLower()))
                        {
                            string value = x.Traits[_traitName.ToLower()].ToString().ToLower();
                            if (_traitValue != null && value == _traitValue.ToLower())
                                return true;
                        }
                        return false;
                    });
                }
                else if(_traitFilter == MonaBrainTraitFilterType.Exclude)
                {
                    tokens = tokens.FindAll(x =>
                    {
                        if (x.Traits.ContainsKey(_traitName.ToLower()))
                        {
                            string value = x.Traits[_traitName.ToLower()].ToString().ToLower();
                            if (_traitValue != null && value == _traitValue.ToLower())
                                return false;
                        }
                        return true;
                    });
                }

                if (_tokenFilter == MonaBrainTokenFilterType.OnlyAvatars)
                    tokens = tokens.FindAll(x => x.Avatars != null && x.Avatars.Count > 0);
                else if (_tokenFilter == MonaBrainTokenFilterType.OnlyObjects)
                    tokens = tokens.FindAll(x => x.Artifacts.Count > 0 && (x.Avatars == null || x.Avatars.Count == 0));

                FilterAndForwardTokens(tokens);
                _TokensFound = tokens.Count > 0;
            }
            else
            {
                _TokensFound = false;
            }
            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} {nameof(FetchTokens)} tokens: {_TokensFound}");
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
            if (!string.IsNullOrEmpty(_ownsTokensName))
                _ownsTokens = _brain.Variables.GetBool(_ownsTokensName);

            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} {_TokensFound}");

            if(_TokensFound == _ownsTokens)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}