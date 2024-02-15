using Mona.SDK.Core.State;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.Core.State.UIElements
{
    public class MonaBrainValuesVisualElement : VisualElement
    {
        private IMonaBrainVariables _state;
        private ListView _list;
        private Dictionary<string, Action> _changeListeners = new Dictionary<string, Action>();
        private Toggle _toggleBuiltInVariables;

        private List<IMonaVariablesValue> _values = new List<IMonaVariablesValue>();

        public MonaBrainValuesVisualElement()
        {
            style.flexDirection = FlexDirection.Column;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = new Color(.1f, .1f, .1f);
            header.style.paddingLeft = header.style.paddingRight = header.style.paddingTop = header.style.paddingBottom = 5;
            header.style.marginBottom = 5;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(new Label("Type"));
            header.Add(new Label("Name"));
            header.Add(new Label("Value"));
            header.ElementAt(0).style.width = 80;
            header.ElementAt(0).style.marginRight = 5;
            header.ElementAt(1).style.width = 100;
            header.ElementAt(1).style.marginRight = 5;
            Add(header);

            _list = new ListView(null, 28, () => new MonaBrainValuesItemVisualElement(null), (elem, i) => BindStateItem((MonaBrainValuesItemVisualElement)elem, i));
            //_list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _list.showFoldoutHeader = false;
            _list.showAddRemoveFooter = true;
            _list.reorderable = false;
            _list.bindItem += (elem, i) =>
            {
                var value = _values[i];
                if (value == null) return;
                _changeListeners[value.Name] = () =>
                {
                    ((MonaBrainValuesItemVisualElement)elem).Refresh();
                };
                value.OnChange += _changeListeners[value.Name];
            };
            _list.unbindItem += (elem, i) =>
            {
                var value = ((MonaBrainValuesItemVisualElement)elem).GetStateItem();
                if (value == null) return;
                value.OnChange -= _changeListeners[value.Name];
            };
            _list.itemsAdded += (items) =>
            {
                foreach (var e in items)
                {
                    var variable = _state.CreateVariable("Default", typeof(MonaVariablesString), e);
                    var regex = new Regex("\\d+");
                    var count = _state.VariableList.FindAll(x => regex.Replace(x.Name, "") == "Default");
                    count.Remove(variable);
                    if (count.Count > 0)
                    {
                       variable.Name = "Default" + count.Count.ToString("D2");
                    }
                }
            };
            Add(_list);

            _toggleBuiltInVariables = new Toggle();
            _toggleBuiltInVariables.label = "Show Built-in Brain Variables?";
            _toggleBuiltInVariables.value = false;
            _toggleBuiltInVariables.RegisterValueChangedCallback((evt) =>
            {
                _toggleBuiltInVariables.value = evt.newValue;
                RefreshList();
            });
            Add(_toggleBuiltInVariables);
        }

        private void BindStateItem(MonaBrainValuesItemVisualElement elem, int i)
        {
            var index = _state.VariableList.IndexOf(_values[i]);
            elem.SetStateItem(_state, index);
        }

        public void SetState(IMonaBrainVariables state)
        {
            _state = state;
            RefreshList();
        }

        private void RefreshList()
        {
            if (Application.isPlaying && !_toggleBuiltInVariables.value)
                _values = _state.VariableList.FindAll(x => x.Name != null && !x.Name.StartsWith("__"));
            else
                _values = _state.VariableList;
            _list.itemsSource = _values;
            _list.Rebuild();
        }

    }
}