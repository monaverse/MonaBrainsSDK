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
using Mona.SDK.Brains.Core.Utils.Enums;
using Unity.Profiling;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OwnsTokensInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IConditionInstructionTile, IStartableInstructionTile, IBlockchainInstructionTile, IBlockchainTokenFilterInstructionTile
    {
        public const string ID = "OwnsTokens";
        public const string NAME = "Owns Tokens";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(OwnsTokensInstructionTile);

        [SerializeField] private bool _ownsTokens = true;
        [SerializeField] private string _ownsTokensName;
        [BrainProperty(false)] public bool OwnsTokens { get => _ownsTokens; set => _ownsTokens = value; }
        [BrainPropertyValueName("OwnsTokens", typeof(IMonaVariablesBoolValue))] public string OwnsTokensName { get => _ownsTokensName; set => _ownsTokensName = value; }

        [SerializeField] private MonaBrainFilterOperationType _filterOperation = MonaBrainFilterOperationType.Include;
        [BrainProperty(false)] public MonaBrainFilterOperationType FilterOperation { get => _filterOperation; set => _filterOperation = value; }

        [SerializeField] private MonaBrainTokenPredicateType _predicateType = MonaBrainTokenPredicateType.TraitValue;
        [BrainProperty(true)] public MonaBrainTokenPredicateType SearchType { get => _predicateType; set => _predicateType = value; }

        [SerializeField] private MonaBrainTokenFilterType _tokenType = MonaBrainTokenFilterType.IncludeAll;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.TokenType)]
        [BrainProperty(true)] public MonaBrainTokenFilterType TokenType { get => _tokenType; set => _tokenType = value; }

        [SerializeField] protected string _tokenName;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Name)]
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.NameContains)]
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.NameStartsWith)]
        [BrainProperty(true)] public string TokenName { get => _tokenName; set => _tokenName = value; }

        [SerializeField] protected string _tokenDescription;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.DescriptionContains)]
        [BrainProperty(true)] public string TokenDescription { get => _tokenDescription; set => _tokenDescription = value; }

        [SerializeField] protected string _traitName;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.HasTrait)]
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.TraitValue)]
        [BrainProperty(true)] public string TraitName { get => _traitName; set => _traitName = value; }

        [SerializeField] protected string _traitValue;
        [SerializeField] protected string _traitValueName;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.TraitValue)]
        [BrainProperty(true)] public string TraitValue { get => _traitValue; set => _traitValue = value; }
        [BrainPropertyValueName(nameof(TraitValue), typeof(IMonaVariablesStringValue))] public string TraitValueName { get => _traitValueName; set => _traitValueName = value; }

        [SerializeField] protected string _byCollection;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Collection)]
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.CollectionSymbol)]
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.CollectionSlug)]
        [BrainProperty(true)] public string Collection { get => _byCollection; set => _byCollection = value; }

        [SerializeField] protected string _byContract;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Contract)]
        [BrainProperty(true)] public string Contract { get => _byContract; set => _byContract = value; }

        [SerializeField] protected string _byTokenId;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Token)]
        [BrainProperty(true)] public string TokenId { get => _byTokenId; set => _byTokenId = value; }

        [SerializeField] protected float _byTokenMin;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.TokenRange)]
        [BrainProperty(true)] public float TokenMin { get => _byTokenMin; set => _byTokenMin = value; }

        [SerializeField] protected float _byTokenMax;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.TokenRange)]
        [BrainProperty(true)] public float TokenMax { get => _byTokenMax; set => _byTokenMax = value; }

        [SerializeField] protected string _byChainId;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.ChainId)]
        [BrainProperty(true)] public string ChainId { get => _byChainId; set => _byChainId = value; }

        [SerializeField] protected float _limit;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Limit)]
        [BrainProperty(true)] public float Limit { get => _limit; set => _limit = value; }

        [SerializeField] protected MonaBrainTokenPredicatePositionType _positionType;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Position)]
        [BrainPropertyEnum(true)] public MonaBrainTokenPredicatePositionType PositionType { get => _positionType; set => _positionType = value; }

        [SerializeField] protected float _byIndexValue;
        [SerializeField] protected string _byIndexValueName;
        [BrainPropertyShow(nameof(SearchType), (int)MonaBrainTokenPredicateType.Position)]
        [BrainProperty(true)] public float IndexValue { get => _byIndexValue; set => _byIndexValue = value; }
        [BrainPropertyValueName(nameof(IndexValue), typeof(IMonaVariablesFloatValue))] public string IndexValueName { get => _byIndexValueName; set => _byIndexValueName = value; }


        private IMonaBrain _brain;

        private bool _TokensFound;

        public OwnsTokensInstructionTile() { }

        private Action<MonaWalletConnectedEvent> OnWalletConnected;
        private Action<MonaWalletConnectedEvent> OnWalletDisconnected;
        private Action<MonaWalletTokenSelectedEvent> OnTokenSelected;

        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(OwnsTokensInstructionTile)}.{nameof(Do)}");
        static readonly ProfilerMarker _profilerPreload = new ProfilerMarker($"MonaBrains.{nameof(OwnsTokensInstructionTile)}.{nameof(Preload)}");

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _profilerPreload.Begin();
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

                if(MonaGlobalBrainRunner.Instance.Blockchain.WalletConnected)
                {
                    SyncFetchTokens();
                }

            }
            _profilerPreload.End();
        }

        private void HandleTokenSelected(MonaWalletTokenSelectedEvent evt)
        {
        }

        private void HandleWalletConnected(MonaWalletConnectedEvent evt)
        {
            SyncFetchTokens();
        }

        private async void SyncFetchTokens()
        { 
            //Debug.Log($"{nameof(HandleWalletConnected)} wallet connected");
            await FetchTokens();
            //Debug.Log($"{nameof(FetchTokens)} tokens fetched");
        }

        private void HandleWalletDisconneccted(MonaWalletConnectedEvent evt)
        {
            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} tokens found set to false");
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
        
        private List<Token> _tokens = new List<Token>();

        private async Task FetchTokens()
        {
            if (_instruction.Muted)
            {
                _TokensFound = true;
                return;
            }
            else
                _TokensFound = false;

            var block = MonaGlobalBrainRunner.Instance.Blockchain;

            var tokens = await block.OwnsTokens();
            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} owns tokens: {tokens.Count}");


            _tokens.Clear();
            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} clear");

            switch (_predicateType)
            {
                case MonaBrainTokenPredicateType.TokenType:

                    //Debug.Log($"{nameof(FetchTokens)} {_tokenType}");
                    if (_tokenType == MonaBrainTokenFilterType.OnlyAvatars)
                        _tokens = tokens.FindAll(x =>
                        {
                            if (x.Files.Find(x => x.Filetype == "vrm") != null)
                                return true;
                            return false;
                        });
                    else if (_tokenType == MonaBrainTokenFilterType.OnlyGLBs)
                        _tokens = tokens.FindAll(x =>
                        {
                            if(x.Files.Find(x => x.Filetype == "glb") != null && x.Files.Find(x => x.Filetype == "vrm") == null)
                            return true;
                            return false;
                        });
                    else if (_tokenType == MonaBrainTokenFilterType.OnlyAvatarsAndGLBs)
                        _tokens = tokens.FindAll(x =>
                        {
                            if (x.Files.Find(x => x.Filetype == "vrm") != null || x.Files.Find(x => x.Filetype == "glb") != null)
                                return true;
                            return false;
                        });
                    else if (_tokenType == MonaBrainTokenFilterType.OnlyUnitySpaces)
                        _tokens = tokens.FindAll(x =>
                        {
                            if (x.Files.Find(x => x.Filetype == "unityasset" && x.Url.Contains("Space")) != null)
                                return true;
                            return false;
                        });

                    Debug.Log($"{nameof(FetchTokens)} {tokens.Count}");
                    break;
                case MonaBrainTokenPredicateType.HasTrait:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Traits.ContainsKey(_traitName.ToLower())) return true;
                        return false;
                    });

                    break;
                case MonaBrainTokenPredicateType.TraitValue:

                    if (!string.IsNullOrEmpty(_traitValueName))
                        _traitValue = _brain.Variables.GetString(_traitValueName);

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Traits.ContainsKey(_traitName.ToLower()))
                        {
                            string value = x.Traits[_traitName.ToLower()].ToString().ToLower();
                            if (_traitValue != null && value == _traitValue.ToLower())
                                return true;
                        }
                        return false;
                    });

                    break;
                case MonaBrainTokenPredicateType.Name:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Name.ToLower() == _tokenName.ToLower())
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.NameContains:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Name.ToLower().Contains(_tokenName.ToLower()))
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.NameStartsWith:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Name.ToLower().StartsWith(_tokenName.ToLower()))
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.DescriptionContains:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Description != null && x.Description.ToLower().StartsWith(_tokenDescription.ToLower()))
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.Collection:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Collection.Id == _byCollection)
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.CollectionSymbol:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Collection.Id == _byCollection)
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.CollectionSlug:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Collection.Id == _byCollection)
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.ChainId:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.ChainId == _byChainId)
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.Contract:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.Contract == _byContract)
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.Token:

                    _tokens = tokens.FindAll(x =>
                    {
                        if (x.TokenId.ToLower() == _byTokenId.ToLower())
                            return true;
                        return false;
                    });
                    break;
                case MonaBrainTokenPredicateType.TokenRange:

                    _tokens = tokens.FindAll(x =>
                    {
                        var tokenId = float.Parse(x.TokenId);
                        if (tokenId >= _byTokenMin && tokenId <= _byTokenMax)
                            return true;
                        return false;
                    });
                    break;

                case MonaBrainTokenPredicateType.Limit:

                    _tokens.Clear();
                    var c = Mathf.Min(tokens.Count, _limit);
                    for (var i = 0; i < c; i++)
                        _tokens.Add(tokens[i]);
                    break;
            }

            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} tokens found {_tokenType} {_TokensFound} {tokens.Count}", _brain.Body.Transform.gameObject);
            TriggerRefresh();
            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} {nameof(FetchTokens)} tokens: {_TokensFound}");
        }

        private void TriggerRefresh()
        {

            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Blockchain, _instruction));

        }

        private List<Token> FilterAndForwardTokens(List<Token> tokens)
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

            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} {nameof(FilterAndForwardTokens)} count {_instruction.Tokens.Count} {_predicateType}");
            if (_instruction.Tokens.Count == 0)
            {
                if (firstFilter == this && _filterOperation == MonaBrainFilterOperationType.Include)
                {
                    _instruction.Tokens.AddRange(tokens);
                }
            }
            else if (_predicateType == MonaBrainTokenPredicateType.Position)
            {
                Token item = default;
                var found = false;
                switch (_positionType)
                {
                    case MonaBrainTokenPredicatePositionType.Index:

                        if(!string.IsNullOrEmpty(_byIndexValueName))
                            _byIndexValue = _brain.Variables.GetFloat(_byIndexValueName);

                        if ((int)_byIndexValue > 0 && (int)_byIndexValue < _instruction.Tokens.Count)
                        {
                            item = _instruction.Tokens[(int)_byIndexValue];
                            found = true;
                        }
                        break;
                    case MonaBrainTokenPredicatePositionType.First:
                        if(_instruction.Tokens.Count > 0)
                        {
                            item = _instruction.Tokens[0];
                            found = true;
                        }
                        break;
                    case MonaBrainTokenPredicatePositionType.Last:
                        if (_instruction.Tokens.Count > 0)
                        {
                            item = _instruction.Tokens[_instruction.Tokens.Count-1];
                            found = true;
                        }
                        break;
                    case MonaBrainTokenPredicatePositionType.Random:
                        if (_instruction.Tokens.Count > 0)
                        {
                            item = _instruction.Tokens[UnityEngine.Random.Range(0, _instruction.Tokens.Count)];
                            found = true;
                        }
                        break;
                }
                if (found)
                {
                    if (_filterOperation == MonaBrainFilterOperationType.Include)
                        _instruction.Tokens = new List<Token>() { item };
                    else
                        _instruction.Tokens = _instruction.Tokens.FindAll(x => !x.Equals(item));
                }
            }
            else { 
                var filtered = _instruction.Tokens.FindAll(x =>
                {
                    if (_filterOperation == MonaBrainFilterOperationType.Include)
                    {
                        return tokens.FindIndex(a => a.Equals(x)) > -1;
                    }
                    else
                        return !tokens.Contains(x);
                });
                _instruction.Tokens = filtered;

            }

            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} {_predicateType} count {_instruction.Tokens.Count}");
            /*
            for (var i = 0; i < _instruction.Tokens.Count; i++)
                Debug.Log($"{nameof(OwnsTokenInstructionTile)} found token {_instruction.Tokens[i].Nft.Metadata.Name} - tokenid: {_instruction.Tokens[i].Nft.TokenId} - contract: {_instruction.Tokens[i].Contract}");
            */
            return _instruction.Tokens;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_ownsTokensName))
                _ownsTokens = _brain.Variables.GetBool(_ownsTokensName);

            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} do {_tokens.Count} in this filter, {_instruction.Tokens.Count} entering this filter");
            FilterAndForwardTokens(_tokens);
            _TokensFound = _instruction.Tokens.Count > 0;

            //Debug.Log($"{nameof(OwnsTokensInstructionTile)} DO: tokens found {_TokensFound} {_instruction.Tokens.Count}", _brain.Body.Transform.gameObject);
            if (_TokensFound == _ownsTokens)
            {
                //Debug.Log($"{nameof(OwnsTokensInstructionTile)} {_TokensFound} {_instruction.Tokens.Count}");
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

        }

    }
}