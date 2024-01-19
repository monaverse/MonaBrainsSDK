using Mona;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Unity.VisualScripting;

namespace Mona.Brains.VisualScripting.Units
{
    /// <summary>
    /// Executes the output ports in order.
    /// </summary>
    /// 
    [UnitTitle("PageInstructions")]
    [UnitSubtitle("Control")]
    [UnitCategory("MonaBrain\\Control")]
    [UnitOrder(13)]
    [TypeIcon(typeof(Sequence))]
    public sealed class PageInstructions : Unit
    {
        [SerializeAs(nameof(instructionCount))]
        private int _instructionCount = 2;

        /// <summary>
        /// The entry point for the sequence.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        [DoNotSerialize]
        [Inspectable, InspectorLabel("Instructions"), UnitHeaderInspectable("Instructions")]
        public int instructionCount
        {
            get => _instructionCount;
            set => _instructionCount = Mathf.Clamp(value, 1, 20);
        }

        [DoNotSerialize]
        public ReadOnlyCollection<ControlOutput> multiInstructionsControlOutputs { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);

            var _multiInstructionControlOutputs = new List<ControlOutput>();

            multiInstructionsControlOutputs = _multiInstructionControlOutputs.AsReadOnly();

            for (var i = 0; i < instructionCount; i++)
            {
                var output = ControlOutput(i.ToString());

                Succession(enter, output);

                _multiInstructionControlOutputs.Add(output);
            }
        }

        private ControlOutput Enter(Flow flow)
        {
            var stack = flow.PreserveStack();

            for(var i = 0;i < multiInstructionsControlOutputs.Count; i++)
            {
                flow.Invoke(multiInstructionsControlOutputs[i]);

                flow.RestoreStack(stack);
            }

            flow.DisposePreservedStack(stack);

            return null;
        }

        public void CopyFrom(Sequence source)
        {
            base.CopyFrom(source);
            instructionCount = source.outputCount;
        }
    }
}