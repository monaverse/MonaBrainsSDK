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
            _monaTagListView = new ListView(null, 20, () => new MonaTagEditItemVisualElement(_monaTags), (elem, i) => ((MonaTagEditItemVisualElement)elem).SetValue(i, _monaTags.AllTags[i]));
            _monaTagListView.showFoldoutHeader = true;
            _monaTagListView.headerTitle = "Mona Tags";
            _monaTagListView.showAddRemoveFooter = true;
            _monaTagListView.reorderMode = ListViewReorderMode.Animated;
            _monaTagListView.reorderable = true;
            _monaTagListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _monaTags.AllTags[e] = (IMonaTagItem)new MonaTagItem("Default", false, true);
                }
            };
            Add(_monaTagListView);
        }

        public void SetMonaTags(IMonaTags monaTags)
        {
            _monaTags = monaTags;
            _monaTagListView.itemsSource = _monaTags.AllTags;
            _monaTagListView.Rebuild();
        }

    }
}