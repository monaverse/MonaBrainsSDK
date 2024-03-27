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
    public class SetVariableOnSelfInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "SetVariableOnSelf";
        public const string NAME = "Set Variable On Self";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetVariableOnSelfInstructionTile);

        [SerializeField] private string _myVariable;
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string MyVariable { get => _myVariable; set => _myVariable = value; }

        [SerializeField] private string _targetVariable;
        [BrainProperty(true)] public string TargetVariable { get => _targetVariable; set => _targetVariable = value; }

        private IMonaBrain _brain;

        public SetVariableOnSelfInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_myVariable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            IMonaBody topBody = _brain.Body;
            while (_brain.Body.Parent != null)
                topBody = topBody.Parent;

            var children = topBody.Children();

            var myValue = _brain.Variables.GetVariable(_myVariable);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].ActiveTransform == null)
                    continue;

                var runner = children[i].ActiveTransform.GetComponent<MonaBrainRunner>();

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

                var tagrgetValue = brainVariables.GetVariable(_targetVariable);

                if (tagrgetValue == null)
                    continue;

                if (myValue is IMonaVariablesFloatValue && tagrgetValue is IMonaVariablesFloatValue)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesFloatValue)myValue).Value);
                }
                else if (myValue is IMonaVariablesStringValue && tagrgetValue is IMonaVariablesStringValue)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesStringValue)myValue).Value);
                }
                else if (myValue is IMonaVariablesBoolValue && tagrgetValue is IMonaVariablesBoolValue)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesBoolValue)myValue).Value);
                }
                else if (myValue is IMonaVariablesVector2Value && tagrgetValue is IMonaVariablesVector2Value)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesVector2Value)myValue).Value);
                }
                else if (myValue is IMonaVariablesVector3Value && tagrgetValue is IMonaVariablesVector3Value)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesVector3Value)myValue).Value);
                }
            }  
        }
    }
}