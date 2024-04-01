using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class SetVariableOnSpawnedInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "SetVariableOnSpawned";
        public const string NAME = "Set Variable On Spawned";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetVariableOnSpawnedInstructionTile);

        [SerializeField] private string _myVariable;
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string MyVariable { get => _myVariable; set => _myVariable = value; }

        [SerializeField] private string _targetVariable;
        [BrainProperty(true)] public string TargetVariable { get => _targetVariable; set => _targetVariable = value; }

        [SerializeField] private bool _includeAttached = true;
        [SerializeField] private string _includeAttachedName;
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

        private IMonaBrain _brain;

        public SetVariableOnSpawnedInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_myVariable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            IMonaBody targetBody = _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);

            if (targetBody == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_TARGET);

            if (!string.IsNullOrEmpty(_includeAttachedName))
                _includeAttached = _brain.Variables.GetBool(_includeAttachedName);

            var myValue = _brain.Variables.GetVariable(_myVariable);

            SetValueOnBrains(myValue, targetBody);
            return Complete(InstructionTileResult.Success);
        }

        private void SetValueOnBrains(IMonaVariablesValue myValue, IMonaBody body)
        {
            if (body.ActiveTransform == null)
                return;

            var runner = body.ActiveTransform.GetComponent<MonaBrainRunner>();

            if (runner == null)
                return;

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

            if (!_includeAttached)
                return;

            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
                SetValueOnBrains(myValue, children[i]);
        }        
    }
}