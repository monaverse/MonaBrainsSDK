using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Control
{
    [Serializable]
    public class MonaBrainPage : IMonaBrainPage
    {
        [SerializeReference]
        public List<IInstruction> _instructions = new List<IInstruction>();
        public List<IInstruction> Instructions => _instructions;

        [SerializeField]
        private string _name;

        public string Name { get => _name; set => _name = value; }

        public bool _hasTickInstruction = false;

        public MonaBrainPage()
        {

        }

        public MonaBrainPage(string name)
        {
            _name = name;
        }

        public void Preload(IMonaBrain brain)
        {
            for (var i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i];
                instruction.Preload(brain);
                if (!instruction.HasConditional())
                    _hasTickInstruction = true;
            }
        }

        public void ExecuteInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            for (var i = 0; i < Instructions.Count;i++)
            {
                var instruction = Instructions[i];
                instruction.Execute(eventType, evt);
            }

            if(_hasTickInstruction)
            {
                EventBus.Trigger(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), new MonaTickEvent());
            }
        }

        public void Unload()
        {

        }

        public void AddInstruction(Instruction instruction)
        {
            Instructions.Add(instruction);
        }
    }
}