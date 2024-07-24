using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class SetVariableOnTagInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "SetVariableOnTag";
        public const string NAME = "Set Variable On Tag";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetVariableOnTagInstructionTile);

        [SerializeField] private string _myVariable;
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string MyVariable { get => _myVariable; set => _myVariable = value; }

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private string _tagVariable;
        [BrainProperty(true)] public string TagVariable { get => _tagVariable; set => _tagVariable = value; }

        private IMonaBrain _brain;

        public SetVariableOnTagInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_myVariable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var tagBodies = MonaBody.FindByTag(_tag);

            if (tagBodies.Count < 1)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_TARGET);

            var myValue = _brain.Variables.GetVariable(_myVariable);

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i].ActiveTransform == null)
                    continue;

                var runner = tagBodies[i].ActiveTransform.GetComponent<MonaBrainRunner>();

                if (runner == null)
                    continue;

                SetValueOnBrains(myValue, runner);
            }

            return Complete(InstructionTileResult.Success);
        }

        private void SetValueOnBrains(IMonaVariablesValue myValue, MonaBrainRunner runner)
        {
            for (int i = 0; i < runner.BrainInstances.Count; i++)
            {
                var brainVariables = runner.BrainInstances[i].Variables;

                if (brainVariables == null)
                    continue;

                var tagValue = brainVariables.GetVariable(_tagVariable);

                if (tagValue == null)
                    continue;

                if (myValue is IMonaVariablesFloatValue && tagValue is IMonaVariablesFloatValue)
                {
                    brainVariables.Set(_tagVariable, ((IMonaVariablesFloatValue)myValue).ValueToReturnFromTile);
                }
                else if (myValue is IMonaVariablesStringValue && tagValue is IMonaVariablesStringValue)
                {
                    brainVariables.Set(_tagVariable, ((IMonaVariablesStringValue)myValue).Value);
                }
                else if (myValue is IMonaVariablesBoolValue && tagValue is IMonaVariablesBoolValue)
                {
                    brainVariables.Set(_tagVariable, ((IMonaVariablesBoolValue)myValue).Value);
                }
                else if (myValue is IMonaVariablesVector2Value && tagValue is IMonaVariablesVector2Value)
                {
                    brainVariables.Set(_tagVariable, ((IMonaVariablesVector2Value)myValue).Value);
                }
                else if (myValue is IMonaVariablesVector3Value && tagValue is IMonaVariablesVector3Value)
                {
                    brainVariables.Set(_tagVariable, ((IMonaVariablesVector3Value)myValue).Value);
                }
            }  
        }
    }
}