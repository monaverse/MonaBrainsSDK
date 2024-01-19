using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
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