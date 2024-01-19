using Mona.SDK.Brains.Core.Control;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaPageInstanceViewElement : VisualElement
    {
        private IMonaBrainPage _page;

        public MonaPageInstanceViewElement(IMonaBrainPage page)
        {
            _page = page;
            for(var i = 0;i < _page.Instructions.Count; i++)
            {
                Add(new MonaInstructionInstanceViewElement(page.Instructions[i], i));
            }
        }
    }
}