using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Mona.SDK.Brains.Core.Events
{
    // Make sure the equality comparer doesn't use boxing
    public class InstructionEventComparer : IEqualityComparer<InstructionEvent>
    {
        public bool Equals(InstructionEvent x, InstructionEvent y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(InstructionEvent obj)
        {
            return obj.GetHashCode();
        }
    }

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

        public override bool Equals(object obj)
        {
            if (!(obj is InstructionEvent other))
            {
                return false;
            }

            return Equals(other);
        }

        public bool Equals(InstructionEvent other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = hash * 23 + BrainEventType.GetHashCode();
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + (Instruction?.GetHashCode() ?? 0);
                hash = hash * 23 + TriggerType.GetHashCode();
                hash = hash * 23 + (Message?.GetHashCode() ?? 0);
                hash = hash * 23 + (Sender?.GetHashCode() ?? 0);
                hash = hash * 23 + Frame.GetHashCode();
                hash = hash * 23 + (Name?.GetHashCode() ?? 0);
                hash = hash * 23 + (Value?.GetHashCode() ?? 0);

                return hash;
            }
        }

        public static bool operator ==(InstructionEvent a, InstructionEvent b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(InstructionEvent a, InstructionEvent b)
        {
            return !(a == b);
        }

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