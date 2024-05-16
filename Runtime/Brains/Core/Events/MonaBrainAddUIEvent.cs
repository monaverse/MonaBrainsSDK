using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainAddUIEvent
    {
        public IMonaBrain Brain;
        public MonaBrainAddUIEvent(IMonaBrain brain)
        {
            Brain = brain;
        }
    }
}