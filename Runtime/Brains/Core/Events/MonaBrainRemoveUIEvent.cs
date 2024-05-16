using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainRemoveUIEvent
    {
        public IMonaBrain Brain;
        public MonaBrainRemoveUIEvent(IMonaBrain brain)
        {
            Brain = brain;
        }
    }
}