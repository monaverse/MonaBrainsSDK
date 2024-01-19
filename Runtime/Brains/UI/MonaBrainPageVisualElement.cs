using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Control;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.Brains.UIElements
{
    public class MonaBrainPageVisualElement : VisualElement, IDisposable
    {
        private IMonaBrain _brain;
        private IMonaBrainPage _page;

        private Label _label;
        private ListView _instructionListView;

        public MonaBrainPageVisualElement()
        {
            style.flexDirection = FlexDirection.Column;

            _instructionListView = new ListView(null, 120, () => new MonaInstructionVisualElement(), (elem, i) => BindInstructionItem((MonaInstructionVisualElement)elem, i));
            _instructionListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _instructionListView.showFoldoutHeader = false;
            _instructionListView.headerTitle = "Instructions";
            _instructionListView.showAddRemoveFooter = true;
            _instructionListView.reorderMode = ListViewReorderMode.Animated;
            _instructionListView.reorderable = true;
            _instructionListView.unbindItem += (elem, i) =>
            {
                if(elem is MonaInstructionVisualElement)
                    ((MonaInstructionVisualElement)elem).ClearInstruction();
            };
            _instructionListView.itemsAdded += (items) =>
            {
                foreach(var e in items)
                    _page.Instructions[e] = new Instruction();
            };
            Add(_instructionListView);
        }

        public void Dispose()
        {
            _instructionListView.itemsSource = null;
        }

        public void SetPage(IMonaBrain brain, IMonaBrainPage page)
        {
            _brain = brain;
            _page = page;
            if (_page == null) return;

            for (var i = _page.Instructions.Count - 1; i >= 0; i--)
            {
                if (_page.Instructions[i] == null)
                    _page.Instructions.RemoveAt(i);
            }

            _instructionListView.itemsSource = _page.Instructions;
            _instructionListView.Rebuild();
        }

        private void BindInstructionItem(MonaInstructionVisualElement elem, int i)
        {
            elem.SetInstruction(_brain, _page.Instructions[i]);
        }
    }
}