using Mona.SDK.Brains.Core.Brain;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaTagReferenceVisualElement : VisualElement
    {
        private DropdownField _monaTagField;
        private int _listIndex;

        public MonaTagReferenceVisualElement(IMonaBrain graph)
        {
            if (graph == null) return;
            _monaTagField = new DropdownField(graph.MonaTagSource.Tags, 0);
            _monaTagField.RegisterValueChangedCallback((evt) =>
            {
                graph.MonaTags[_listIndex] = (string)evt.newValue;
            });
            Add(_monaTagField);
        }

        public void SetValue(int idx, string monaTag)
        {
            _listIndex = idx;
            if (monaTag != null)
                _monaTagField.value = (string)monaTag;
            else
                _monaTagField.value = null;
        }
    }
}