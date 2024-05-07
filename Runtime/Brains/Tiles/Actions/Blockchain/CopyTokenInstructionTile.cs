using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Actions.Blockchain.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Blockchain
{
    [Serializable]
    public class CopyTokenInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IActionInstructionTile
    {
        public const string ID = "CopyToken";
        public const string NAME = "Copy Token";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(CopyTokenInstructionTile);

        [SerializeField] private MonaBrainTokenResultType _source = MonaBrainTokenResultType.AvatarUrl;
        [BrainProperty(true)] public MonaBrainTokenResultType Source { get => _source; set => _source = value; }

        [SerializeField] private MonaBrainSelectTokenType _select = MonaBrainSelectTokenType.Last;
        [BrainProperty(true)] public MonaBrainSelectTokenType Method { get => _select; set => _select = value; }

        [SerializeField] private string _targetValue;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.AvatarUrl)]
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.ObjectUrl)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string TargetUrlValue { get => _targetValue; set => _targetValue = value; }

        [SerializeField] private string _traitName;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.TraitValue)]
        [BrainProperty(true)] public string TraitName { get => _traitName; set => _traitName = value; }

        [SerializeField] private string _targetTraitValue;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.TraitValue)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string TargetTraitValue { get => _targetTraitValue; set => _targetTraitValue = value; }

        private IMonaBrain _brain;

        public CopyTokenInstructionTile() { }

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _instruction = instruction;
        }

        public override InstructionTileResult Do()
        {
            Token token = default;
            int index = -1;
            switch (_source)
            {
                case MonaBrainTokenResultType.ObjectUrl:
                    if (_instruction.Tokens.Count > 0)
                    {
                        switch (_select)
                        {
                            case MonaBrainSelectTokenType.Last:
                                token = _instruction.Tokens[_instruction.Tokens.Count - 1];
                                _brain.Variables.Set(_targetValue, token.AssetUrl);
                                break;
                            case MonaBrainSelectTokenType.First:
                                token = _instruction.Tokens[0];
                                _brain.Variables.Set(_targetValue, token.AssetUrl);
                                break;
                            case MonaBrainSelectTokenType.Random:
                                token = _instruction.Tokens[UnityEngine.Random.Range(0, _instruction.Tokens.Count)];
                                _brain.Variables.Set(_targetValue, token.AssetUrl);
                                break;
                        }
                    }
                    break;

                case MonaBrainTokenResultType.AvatarUrl:
                    if (_instruction.Tokens.Count > 0)
                    {
                        switch (_select)
                        {
                            case MonaBrainSelectTokenType.Last:
                                token = _instruction.Tokens[_instruction.Tokens.Count - 1];
                                _brain.Variables.Set(_targetValue, token.AssetUrl);
                                break;
                            case MonaBrainSelectTokenType.First:
                                token = _instruction.Tokens[0];
                                _brain.Variables.Set(_targetValue, token.AssetUrl);
                                break;
                            case MonaBrainSelectTokenType.Random:
                                token = _instruction.Tokens[UnityEngine.Random.Range(0, _instruction.Tokens.Count)];
                                _brain.Variables.Set(_targetValue, token.AssetUrl);
                                break;
                        }
                    }
                    break;

                case MonaBrainTokenResultType.TraitValue:

                    var tokens = _instruction.Tokens.FindAll(x =>
                    {
                        if (x.Traits.ContainsKey(_traitName.ToLower()))
                            return true;
                        return false;
                    });

                    Debug.Log($"{nameof(CopyTokenInstructionTile)} copy trait : {_traitName} {tokens.Count}");

                    if (tokens.Count > 0)
                    {
                        switch (_select)
                        {
                            case MonaBrainSelectTokenType.Last:
                                token = tokens[tokens.Count - 1];
                                _brain.Variables.Set(_targetTraitValue, token.Traits[_traitName.ToLower()].ToString().ToLower());
                                break;
                            case MonaBrainSelectTokenType.First:
                                token = tokens[0];
                                _brain.Variables.Set(_targetTraitValue, token.Traits[_traitName.ToLower()].ToString().ToLower());
                                break;
                            case MonaBrainSelectTokenType.Random:
                                token = tokens[UnityEngine.Random.Range(0, tokens.Count)];
                                _brain.Variables.Set(_targetTraitValue, token.Traits[_traitName.ToLower()].ToString().ToLower());
                                break;
                        }
                    }
                    break;
                default: break;
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}