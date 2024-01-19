using Mona.SDK.Brains.Core.Brain;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainRunnerVisualElement : VisualElement
    {
        private MonaBrainRunner _runner;

        private VisualElement _root;
        private VisualElement _instructionContainer;

        private ListView _brainsListView;
        private bool _began;

        public MonaBrainRunnerVisualElement()
        {
            _root = new VisualElement();

            _brainsListView = new ListView(null, 120, () => new MonaBrainReferenceVisualElement(_runner), (elem, i) => ((MonaBrainReferenceVisualElement)elem).SetValue(i, _runner.BrainGraphs[i]));
            _brainsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _brainsListView.showFoldoutHeader = true;
            _brainsListView.headerTitle = "Mona Brain Graphs";
            _brainsListView.showAddRemoveFooter = true;
            _brainsListView.reorderMode = ListViewReorderMode.Animated;
            _brainsListView.reorderable = true;
            _brainsListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _runner.BrainGraphs[e] = null;
                }
            };
            Add(_brainsListView);

            _instructionContainer = new VisualElement();
            _root.Add(_instructionContainer);

            Add(_root);
        }

        public void SetRunner(MonaBrainRunner runner)
        {
            _runner = runner;
            _runner.OnBegin += HandleBegin;

            if (_runner.BrainGraphs.Count == 0)
                _runner.BrainGraphs.Add(null);

            _brainsListView.itemsSource = _runner.BrainGraphs;
            _brainsListView.Rebuild();

            if (_runner.Began)
                HandleBegin(runner);
        }

        private void HandleBegin(IMonaBrainRunner runner)
        {
            //Debug.Log($"{nameof(HandleBegin)}");
            _instructionContainer.Clear();
            for (var i = 0; i < _runner.BrainInstances.Count; i++)
                _instructionContainer.Add(new MonaBrainInstanceVisualElement(_runner.BrainInstances[i]));
        }

    }
}