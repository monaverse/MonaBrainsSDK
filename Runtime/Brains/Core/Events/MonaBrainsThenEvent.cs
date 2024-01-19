using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;

namespace Mona.Brains.Core.Events
{
    public struct MonaBrainsThenEvent
    {
        public string EventName;
        public InstructionTileResult Result;

        public MonaBrainsThenEvent(string eventName, InstructionTileResult result)
        {
            EventName = eventName;
            Result = result;
        }
    }
}