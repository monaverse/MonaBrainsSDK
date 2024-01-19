using Mona.Brains.Core.Brain;

namespace Mona.Brains.Core.Events
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