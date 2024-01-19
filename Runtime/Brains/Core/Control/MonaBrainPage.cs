using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.Brains.Core.Control
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
            }
        }

        public void ExecuteInstructions(InstructionEventTypes eventType)
        {
            for (var i = 0; i < Instructions.Count;i++)
            {
                var instruction = Instructions[i];
                instruction.Execute(eventType);
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