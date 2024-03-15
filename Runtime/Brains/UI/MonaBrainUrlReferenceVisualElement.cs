using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainUrlReferenceVisualElement : VisualElement
    {
        private TextField _textField;

        private int _listIndex;

        public MonaBrainUrlReferenceVisualElement(MonaBrainRunner runner)
        {
            if (runner == null) return;
#if UNITY_EDITOR
            _textField = new TextField();
            _textField.RegisterValueChangedCallback((evt) =>
            {
                runner.BrainUrls[_listIndex] = (string)evt.newValue;
            });
            Add(_textField);
#endif
        }

        public void SetValue(int idx, string url)
        {
            _listIndex = idx;
            if (url != null)
                _textField.value = url;
            else
                _textField.value = null;
        }
    }
}