using Mona.SDK.Core.State;
using Mona.SDK.Core.State.Structs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.Core.State.UIElements
{
    public class MonaBrainValuesVisualElement : VisualElement
    {
        private IMonaBrainState _state;
        private ListView _list;

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

            _list = new ListView(null, 28, () => new MonaBrainValuesItemVisualElement(), (elem, i) => BindStateItem((MonaBrainValuesItemVisualElement)elem, i));
            //_list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _list.showFoldoutHeader = false;
            _list.showAddRemoveFooter = true;
            _list.reorderable = false;
            _list.itemsAdded += (items) =>
            {
                foreach (var e in items)
                    _state.CreateValue("Default", typeof(MonaStateString), e);
            };
            Add(_list);
        }

        private void BindStateItem(MonaBrainValuesItemVisualElement elem, int i)
        {
            elem.SetStateItem(_state, i);
        }

        public void SetState(IMonaBrainState state)
        {
            _state = state;
            _list.itemsSource = _state.Values;
            _list.Rebuild();
        }

    }
}