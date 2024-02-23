using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainTickEvent : IInstructionEvent
    {
        public InstructionEventTypes Type;
        public IInstruction Instruction;

        public MonaBrainTickEvent(InstructionEventTypes type)
        {
            Type = type;
            Instruction = null;
        }

        public MonaBrainTickEvent(InstructionEventTypes type, IInstruction instruction)
        {
            Type = type;
            Instruction = instruction;
        }
    }
}