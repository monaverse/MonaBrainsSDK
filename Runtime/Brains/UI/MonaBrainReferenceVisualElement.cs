using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainReferenceVisualElement : VisualElement
    {
        private ObjectField _objectField;
        private int _listIndex;

        public MonaBrainReferenceVisualElement(MonaBrainRunner runner)
        {
            if (runner == null) return;
            _objectField = new ObjectField();
            _objectField.objectType = typeof(MonaBrainGraph);
            _objectField.RegisterValueChangedCallback((evt) =>
            {
                runner.BrainGraphs[_listIndex] = (MonaBrainGraph)evt.newValue;
            });
            Add(_objectField);
        }

        public void SetValue(int idx, IMonaBrain graph)
        {
            _listIndex = idx;
            if (graph != null)
                _objectField.value = (MonaBrainGraph)graph;
            else
                _objectField.value = null;
        }
    }
}