using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Core.Events
{
    public struct InstructionEvent : IInstructionEvent
    {
        public MonaBrainEventType BrainEventType { get; set; }
        public InstructionEventTypes Type { get; set; }
        public IInstruction Instruction { get; set; }
        public MonaTriggerType TriggerType { get; set; }
        public string Message { get; set; }
        public IMonaBrain Sender { get; set; }
        public int Frame { get; set; }

        public string Name { get; set; }
        public IMonaVariablesValue Value { get; set; }

        public InstructionEvent(string message, IMonaBrain sender, int frame)
        {
            BrainEventType = default;
            Type = default;
            Instruction = default;
            TriggerType = default;
            Message = default;
            Sender = default;
            Frame = default;
            Name = default;
            Value = default;

            Message = message;
            Sender = sender;
            Frame = frame;
            Type = InstructionEventTypes.Message;
        }

        public InstructionEvent(MonaTriggerType type)
        {
            BrainEventType = default;
            Type = default;
            Instruction = default;
            TriggerType = default;
            Message = default;
            Sender = default;
            Frame = default;
            Name = default;
            Value = default;

            TriggerType = type;
            Type = InstructionEventTypes.Trigger;
        }

        public InstructionEvent(string name, IMonaVariablesValue value)
        {
            BrainEventType = default;
            Type = default;
            Instruction = default;
            TriggerType = default;
            Message = default;
            Sender = default;
            Frame = default;
            Name = default;
            Value = default;

            Name = name;
            Value = value;
            Type = InstructionEventTypes.Value;
        }

        public InstructionEvent(InstructionEventTypes type)
        {
            BrainEventType = default;
            Type = default;
            Instruction = default;
            TriggerType = default;
            Message = default;
            Sender = default;
            Frame = default;
            Name = default;
            Value = default;

            Type = type;
        }

        public InstructionEvent(InstructionEventTypes type, IInstruction instruction)
        {
            BrainEventType = default;
            Type = default;
            Instruction = default;
            TriggerType = default;
            Message = default;
            Sender = default;
            Frame = default;
            Name = default;
            Value = default;

            
            Type = type;
            Instruction = instruction;
        }
    }
}