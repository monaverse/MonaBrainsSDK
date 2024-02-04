using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.State.UIElements;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.Core.State.UIElements
{
    public class MonaBrainValuesItemVisualElement : MonaStateItemVisualElement
    {
        public override void Refresh()
        {
            base.Refresh();

            var value = _state.Values[_index];
            if (value is IMonaStateBrainValue)
            {
                _typeField.value = MonaCoreConstants.REFERENCE_TYPE_LABEL;
                Add(_stringField);
                var brain = ((IMonaStateBrainValue)value).Value;
                _stringField.value = brain != null ? brain.Name : "Null";
                _stringField.SetEnabled(false);
            }
            else if (value is IMonaStateBodyValue)
            {
                _typeField.value = MonaCoreConstants.REFERENCE_TYPE_LABEL;
                Add(_stringField);
                var body = ((IMonaStateBodyValue)value).Value;
                _stringField.value = body != null ? body.Transform.name : "Null";
                _stringField.SetEnabled(false);
            }
        }
    }
}