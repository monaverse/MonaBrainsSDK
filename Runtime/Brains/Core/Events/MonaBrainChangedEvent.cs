using Mona.Brains.Core.Brain;

namespace Mona.Brains.Core.Events
{
    public struct MonaBrainChangedEvent
    {
        public string Name;
        public IMonaBrain Value;

        public MonaBrainChangedEvent(string name, IMonaBrain value)
        {
            Name = name;
            Value = value;
        }
    }
}