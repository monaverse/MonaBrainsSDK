using UnityEngine.UIElements;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaBrainInstanceVisualElement : VisualElement
    {
        private IMonaBrain _brain;
        private VisualElement _brainContainer;
        private VisualElement _activePageContainer;
        private Label _activeStatePage;

        public MonaBrainInstanceVisualElement(IMonaBrain brain)
        {
            _brain = brain;
            _brain.OnStateChanged += HandleStateChanged;

            SetPadding(this, 5);
            SetMargin(this, 5);
            SetBorder(this, Color.black);

            var heading = new Label(brain.Name);
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            heading.style.fontSize = 14;
            Add(heading);

            _brainContainer = new VisualElement();
            SetPadding(_brainContainer, 5);

            _brainContainer.Add(new Label("Core Page"));
            _brainContainer.Add(new MonaPageInstanceViewElement(_brain.CorePage));

            _activeStatePage = new Label($"Active State Page:");
            _brainContainer.Add(_activeStatePage);

            _activePageContainer = new VisualElement();
            for (var i = 0;i < _brain.StatePages.Count; i++)
            {
                var page = _brain.StatePages[i];
                _activePageContainer.Add(new MonaPageInstanceViewElement(page));
            }

            _brainContainer.Add(_activePageContainer);
            Add(_brainContainer);

            HandleStateChanged(null, _brain);
        }

        private void HandleStateChanged(string state, IMonaBrain brain)
        {
            bool found = false;
            for (var i = 0;i < _brain.StatePages.Count; i++)
            {
                var page = _brain.StatePages[i];
                if (page.Name == state)
                {
                    found = true;
                    _activeStatePage.text = $"Active State Page: {page.Name}";
                    _activePageContainer.ElementAt(i).style.display = DisplayStyle.Flex;
                }
                else
                {
                    _activePageContainer.ElementAt(i).style.display = DisplayStyle.None;
                }
            }

            if(!found)
                _activeStatePage.text = $"Active State Page: None";

            if (_brain.StatePages.Count == 0)
                _activeStatePage.text = $"";
        }

        private void SetPadding(VisualElement elem, float padding)
        {
            elem.style.paddingLeft = elem.style.paddingTop = elem.style.paddingRight = elem.style.paddingBottom = padding;
        }

        private void SetMargin(VisualElement elem, float margin)
        {
            elem.style.marginLeft = elem.style.marginTop = elem.style.marginRight = elem.style.marginBottom = margin;
        }

        private void SetBorder(VisualElement elem, Color color)
        {
            elem.style.borderLeftColor = elem.style.borderTopColor = elem.style.borderRightColor = elem.style.borderBottomColor = color;
            elem.style.borderLeftWidth = elem.style.borderTopWidth = elem.style.borderRightWidth = elem.style.borderBottomWidth = 1;
        }
    }
}