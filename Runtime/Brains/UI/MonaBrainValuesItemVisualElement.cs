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
        protected override void Refresh()
        {
            base.Refresh();

            var value = _state.Values[_index];
            if (value is IMonaStateBrainValue)
            {
                _typeField.value = MonaCoreConstants.REFERENCE_TYPE_LABEL;
                Add(_stringField);
                _stringField.value = ((IMonaStateBrainValue)value).Value.Name;
                _stringField.SetEnabled(false);
            }
            else if (value is IMonaStateBodyValue)
            {
                _typeField.value = MonaCoreConstants.REFERENCE_TYPE_LABEL;
                Add(_stringField);
                _stringField.value = ((IMonaStateBodyValue)value).Value.Transform.name;
                _stringField.SetEnabled(false);
            }
        }
    }
}