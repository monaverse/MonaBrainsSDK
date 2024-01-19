using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
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