using Mona.SDK.Brains.Core.Brain;
using System;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaGlobalBrainRunnerVisualElement : VisualElement, IDisposable
    {
        private MonaGlobalBrainRunner _globalRunner;

        private VisualElement _root;
        private VisualElement _instructionContainer;

        private ListView _brainsListView;

        public MonaGlobalBrainRunnerVisualElement()
        {
            _root = new VisualElement();

            _brainsListView = new ListView(null, 120, () => new MonaBrainReferenceVisualElement(_globalRunner), (elem, i) => ((MonaBrainReferenceVisualElement)elem).SetValue(i, _globalRunner.BrainGraphs[i]));
            _brainsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _brainsListView.showFoldoutHeader = true;
            _brainsListView.headerTitle = "Local Player Brain Graphs";
            _brainsListView.showAddRemoveFooter = true;
            _brainsListView.reorderMode = ListViewReorderMode.Animated;
            _brainsListView.reorderable = true;
            _brainsListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _globalRunner.BrainGraphs[e] = null;
                }
            };
            Add(_brainsListView);

            _instructionContainer = new VisualElement();
            _root.Add(_instructionContainer);

            Add(_root);
        }

        public void SetRunner(MonaGlobalBrainRunner runner)
        {
            _globalRunner = runner;
            
            _brainsListView.itemsSource = _globalRunner.BrainGraphs;
            _brainsListView.Rebuild();

        }

        public void Dispose()
        {
            _brainsListView.Clear();
        }
    }
}