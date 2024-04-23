using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using System;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaBrainReloadEvent : IInstructionEvent
    {
        public string Message { get; set; }
        public InstructionEventTypes Type { get; set; }
        public IInstruction Instruction { get; set; }
    }
}