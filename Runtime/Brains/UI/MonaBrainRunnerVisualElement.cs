using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.ScriptableObjects;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainRunnerVisualElement : VisualElement, IDisposable
    {
        private MonaBrainRunner _runner;

        private VisualElement _root;
        private VisualElement _instructionContainer;

        private ListView _brainsListView;
        private ListView _brainUrlsListView;
        private Button _importButton;

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

            _brainUrlsListView = new ListView(null, 120, () => new MonaBrainUrlReferenceVisualElement(_runner), (elem, i) => ((MonaBrainUrlReferenceVisualElement)elem).SetValue(i, _runner.BrainUrls[i]));
            _brainUrlsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _brainUrlsListView.showFoldoutHeader = true;
            _brainUrlsListView.headerTitle = "Mona Brain Graph Urls";
            _brainUrlsListView.showAddRemoveFooter = true;
            _brainUrlsListView.reorderMode = ListViewReorderMode.Animated;
            _brainUrlsListView.reorderable = true;
            _brainUrlsListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _runner.BrainUrls[e] = null;
                }
            };
            Add(_brainUrlsListView);

            _importButton = new Button();
            _importButton.text = "Import and Edit Brains";
            _importButton.clicked += () =>
            {
                _runner.LoadBrainGraphs(_runner.BrainUrls, (brains) =>
                {
#if UNITY_EDITOR
                    if (!AssetDatabase.IsValidFolder("Assets/Brains"))
                        AssetDatabase.CreateFolder("Assets", "Brains");

                    if (!AssetDatabase.IsValidFolder("Assets/Brains/Imported"))
                        AssetDatabase.CreateFolder("Assets/Brains", "Imported");

                    for (var i = 0; i < _runner.BrainGraphs.Count; i++)
                    {
                        var brain = (MonaBrainGraph)_runner.BrainGraphs[i];
                        if (brain == null) continue;
                        
                        var name = brain.Name;
                        if (string.IsNullOrEmpty(name))
                            name = brain.name;

                        AssetDatabase.CreateAsset(brain, "Assets/Brains/Imported/" + _runner.gameObject.name + "_" + name + ".asset");
                        AssetDatabase.SaveAssets();
                    }

                    //_runner.BrainUrls.Clear();
                    Refresh();
#endif
                });
            };
            Add(_importButton);

            _instructionContainer = new VisualElement();
            _root.Add(_instructionContainer);

            Add(_root);
        }

        public void SetRunner(MonaBrainRunner runner)
        {
            _runner = runner;
            _runner.OnBegin += HandleBegin;

            Refresh();
        }

        private void Refresh()
        {
            if (_runner.BrainGraphs.Count == 0)
                _runner.BrainGraphs.Add(null);

            _brainsListView.itemsSource = _runner.BrainGraphs;
            _brainsListView.Rebuild();

            _brainUrlsListView.style.display = DisplayStyle.Flex;
            _importButton.style.display = DisplayStyle.Flex;

            _brainUrlsListView.itemsSource = _runner.BrainUrls;
            _brainUrlsListView.Rebuild();
            
            if (_runner.Began)
                HandleBegin(_runner);
        }

        public void Dispose()
        {
            _brainsListView.Clear();
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