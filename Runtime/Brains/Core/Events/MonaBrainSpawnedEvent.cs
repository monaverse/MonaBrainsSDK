using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainSpawnedEvent
    {
        public IMonaBrain Brain;
        public MonaBrainSpawnedEvent(IMonaBrain brain)
        {
            Brain = brain;
        }
    }
}