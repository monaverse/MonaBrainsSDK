using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainValueChangedEvent : IInstructionEvent
    {
        public InstructionEventTypes Type { get; set; }
        public IInstruction Instruction { get; set; }
        public string Name;
        public IMonaVariablesValue Value;

        public MonaBrainValueChangedEvent(string name, IMonaVariablesValue value)
        {
            Type = InstructionEventTypes.Value;
            Instruction = null;
            Name = name;
            Value = value;
        }
    }
}