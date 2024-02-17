using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Network.Enums;
using System;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaGlobalBrainRunnerVisualElement : VisualElement, IDisposable
    {
        private MonaGlobalBrainRunner _globalRunner;

        private VisualElement _root;
        private VisualElement _instructionContainer;

        private EnumField _networkType;
        private ListView _brainsListView;

        public MonaGlobalBrainRunnerVisualElement()
        {
            _root = new VisualElement();


            _networkType = new EnumField("Network Type", MonaNetworkType.Shared);
            _networkType.RegisterValueChangedCallback(evt =>
            {
                if ((MonaNetworkType)evt.newValue != _globalRunner.NetworkSettings.NetworkType)
                {
                    _globalRunner.NetworkSettings.NetworkType = (MonaNetworkType)evt.newValue;
                }
            });
            _root.Add(_networkType);

            _brainsListView = new ListView(null, 120, () => new MonaBrainReferenceVisualElement(_globalRunner), (elem, i) => ((MonaBrainReferenceVisualElement)elem).SetValue(i, _globalRunner.PlayerBrainGraphs[i]));
            _brainsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _brainsListView.showFoldoutHeader = true;
            _brainsListView.headerTitle = "Assign Brain Graphs to Local Player";
            _brainsListView.showAddRemoveFooter = true;
            _brainsListView.reorderMode = ListViewReorderMode.Animated;
            _brainsListView.reorderable = true;
            _brainsListView.itemsAdded += (elems) =>
            {
                foreach (var e in elems)
                {
                    _globalRunner.PlayerBrainGraphs[e] = null;
                }
            };

            var space = new VisualElement();
            space.style.height = 30;
            _root.Add(space);

            _root.Add(_brainsListView);

            _instructionContainer = new VisualElement();
            _root.Add(_instructionContainer);

            Add(_root);
        }

        public void SetRunner(MonaGlobalBrainRunner runner)
        {
            _globalRunner = runner;
            
            _brainsListView.itemsSource = _globalRunner.PlayerBrainGraphs;
            _brainsListView.Rebuild();

            _networkType.value = _globalRunner.NetworkSettings.NetworkType;
        }

        public void Dispose()
        {
            _brainsListView.Clear();
        }
    }
}