using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using System;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaTagEditItemVisualElement : VisualElement
    {
        private TextField _monaTagField;
        private Toggle _isPlayer;

        private int _listIndex;

        public MonaTagEditItemVisualElement(IMonaTags tags)
        {
            style.flexDirection = FlexDirection.Row;

            _monaTagField = new TextField();
            _monaTagField.style.flexGrow = 1;
            _monaTagField.RegisterValueChangedCallback((evt) =>
            {
                tags.AllTags[_listIndex].Tag = (string)evt.newValue;
            });

            _isPlayer = new Toggle();
            _isPlayer.label = "Is Player Tag?";
            _isPlayer.RegisterValueChangedCallback((evt) =>
            {
                tags.AllTags[_listIndex].IsPlayerTag = evt.newValue;
            });

            Add(_monaTagField);
            Add(_isPlayer);
        }

        public void SetFocus()
        {
            _monaTagField.Focus();
        }

        public void SetValue(int idx, IMonaTagItem monaTag)
        {
            _listIndex = idx;
            if (monaTag != null)
            {
                _monaTagField.value = monaTag.Tag;
                _isPlayer.value = monaTag.IsPlayerTag;
                _monaTagField.SetEnabled(monaTag.IsEditable);
                _isPlayer.SetEnabled(monaTag.IsEditable);
            }
            else
            {
                _monaTagField.value = "";
                _isPlayer.value = false;
                _monaTagField.SetEnabled(monaTag.IsEditable);
                _isPlayer.SetEnabled(monaTag.IsEditable);
            }
        }
    }
}