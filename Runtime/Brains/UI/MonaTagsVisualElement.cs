using Mona.SDK.Brains.Core.ScriptableObjects;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaTagsVisualElement : VisualElement
    {
        private IMonaTags _monaTags;
        private ListView _monaTagListView;

        public MonaTagsVisualElement()
        {
            style.flexDirection = FlexDirection.Column;

            var container = new VisualElement();
            var s = container.style;
            s.paddingTop = s.paddingBottom = s.paddingLeft = s.paddingRight = 5;
            Add(container);

            _monaTagListView = new ListView(null, 20, () => new MonaTagEditItemVisualElement(_monaTags), (elem, i) => {
                var renderer = ((MonaTagEditItemVisualElement)elem);
                    renderer.SetValue(i, _monaTags.AllTags[i]);
             });
            _monaTagListView.showAddRemoveFooter = true;
            _monaTagListView.style.flexGrow = 1;
            _monaTagListView.reorderMode = ListViewReorderMode.Animated;
            _monaTagListView.reorderable = true;
            _monaTagListView.selectionType = SelectionType.Multiple;
            _monaTagListView.itemsAdded += (elems) =>
            {
                IMonaTagItem item = null;
                foreach (var e in elems)
                {
                    _monaTags.AllTags[e] = (IMonaTagItem)new MonaTagItem("Default", false, true);
                    item = _monaTags.AllTags[e];
                }

                _monaTags.AllTags.Sort((a, b) =>
                {
                    var p = a.IsEditable.CompareTo(b.IsEditable);
                    if (p == 0) return a.Tag.CompareTo(b.Tag);
                    else return p;
                });

                _monaTagListView.Rebuild();

                _monaTagListView.schedule.Execute(() =>
                {
                    var idx = _monaTags.AllTags.IndexOf(item);
                    _monaTagListView.ScrollToItem(idx);
                    _monaTagListView.selectedIndex = idx;
                }).ExecuteLater(100);
            };
            container.Add(_monaTagListView);
        }

        public void SetMonaTags(IMonaTags monaTags)
        {
            _monaTags = monaTags;
            
            _monaTagListView.itemsSource = _monaTags.AllTags;
            _monaTags.AllTags.Sort((a, b) =>
            {
                var p = a.IsEditable.CompareTo(b.IsEditable);
                if (p == 0) return a.Tag.CompareTo(b.Tag);
                else return p;
            });

            _monaTagListView.Rebuild();
        }

    }
}