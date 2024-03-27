using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainValueChangedEvent : IInstructionEvent
    {
        public string Name;
        public IMonaVariablesValue Value;

        public MonaBrainValueChangedEvent(string name, IMonaVariablesValue value)
        {
            Name = name;
            Value = value;
        }
    }
}