using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.State.UIElements;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;

namespace Mona.SDK.Brains.Core.State.UIElements
{
    public class MonaBrainValuesItemVisualElement : MonaVariablesItemVisualElement
    {
        public override void Refresh()
        {
            base.Refresh();

            var value = _state.VariableList[_index];
            if (value is IMonaVariablesBrainValue)
            {
                _typeField.value = MonaCoreConstants.REFERENCE_TYPE_LABEL;
                Add(_stringField);
                var brain = ((IMonaVariablesBrainValue)value).Value;
                _stringField.value = brain != null ? brain.Name : "Null";
                _stringField.SetEnabled(false);
            }
            else if (value is IMonaVariablesBodyValue)
            {
                _typeField.value = MonaCoreConstants.REFERENCE_TYPE_LABEL;
                Add(_stringField);
                var body = ((IMonaVariablesBodyValue)value).Value;
                _stringField.value = body != null ? body.Transform.name : "Null";
                _stringField.SetEnabled(false);
            }
        }

        public MonaBrainValuesItemVisualElement(Action newCallback) : base (newCallback)
        {

        }
    }
}