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
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Blockchain
{
    [Serializable]
    public class CopyTokenInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IActionInstructionTile, INeedAuthorityInstructionTile
    {
        public const string ID = "CopyToken";
        public const string NAME = "Copy Token";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(CopyTokenInstructionTile);

        [SerializeField] private MonaBrainTokenResultType _source = MonaBrainTokenResultType.AssetUrl;
        [BrainProperty(true)] public MonaBrainTokenResultType Source { get => _source; set => _source = value; }

        [SerializeField] private string _targetValue;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.AssetUrl)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string TargetUrlValue { get => _targetValue; set => _targetValue = value; }

        [SerializeField] private string _traitName;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.TraitValue)]
        [BrainProperty(true)] public string TraitName { get => _traitName; set => _traitName = value; }

        [SerializeField] private string _targetTraitValue;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.TraitValue)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string TargetTraitValue { get => _targetTraitValue; set => _targetTraitValue = value; }

        [SerializeField] private string _defaultTraitValue = "";
        [SerializeField] private string _defaultTraitValueName = "";

        [BrainPropertyShow(nameof(Source), (int)MonaBrainTokenResultType.TraitValue)]
        [BrainProperty(false)] public string DefaultTraitValue { get => _defaultTraitValue; set => _defaultTraitValue = value; }
        [BrainPropertyValueName(nameof(DefaultTraitValue), typeof(IMonaVariablesStringValue))] public string DefaultTraitValueName { get => _defaultTraitValueName; set => _defaultTraitValueName = value; }

        private IMonaBrain _brain;

        public IMonaBody GetBodyToControl() => _brain.Body;

        public CopyTokenInstructionTile() { }

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _instruction = instruction;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            Token token = default;
            switch (_source)
            {
                case MonaBrainTokenResultType.AssetUrl:
                    if (_instruction.Tokens.Count > 0)
                    {
                        token = _instruction.Tokens[0];
                        _brain.Variables.Set(_targetValue, token.AssetUrl);
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
                        token = tokens[0];
                        _brain.Variables.Set(_targetTraitValue, token.Traits[_traitName.ToLower()].ToString().ToLower());
                    }

                    break;
                default: break;
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}