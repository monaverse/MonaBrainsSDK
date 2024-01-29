using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainReferenceVisualElement : VisualElement
    {
#if UNITY_EDITOR
        private ObjectField _objectField;
#endif

        private int _listIndex;

        public MonaBrainReferenceVisualElement(MonaBrainRunner runner)
        {
            if (runner == null) return;
#if UNITY_EDITOR
            _objectField = new ObjectField();
            _objectField.objectType = typeof(MonaBrainGraph);
            _objectField.RegisterValueChangedCallback((evt) =>
            {
                runner.BrainGraphs[_listIndex] = (MonaBrainGraph)evt.newValue;
            });
            Add(_objectField);
#endif
        }

        public MonaBrainReferenceVisualElement(MonaGlobalBrainRunner runner)
        {
            if (runner == null) return;
#if UNITY_EDITOR
            _objectField = new ObjectField();
            _objectField.objectType = typeof(MonaBrainGraph);
            _objectField.RegisterValueChangedCallback((evt) =>
            {
                runner.BrainGraphs[_listIndex] = (MonaBrainGraph)evt.newValue;
            });
            Add(_objectField);
#endif
        }

        public void SetValue(int idx, IMonaBrain graph)
        {
            _listIndex = idx;
#if UNITY_EDITOR
            if (graph != null)
                _objectField.value = (MonaBrainGraph)graph;
            else
                _objectField.value = null;
#endif
        }
    }
}