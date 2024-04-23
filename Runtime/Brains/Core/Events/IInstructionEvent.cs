using System;
using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Events
{
    public interface IInstructionEvent
    {
        string Message { get; set; }
        IInstruction Instruction { get; set; }
        InstructionEventTypes Type { get; set; }
    }
}