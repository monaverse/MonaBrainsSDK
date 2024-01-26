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
        private IMonaBrain _brain;

        [SerializeReference]
        public List<IInstruction> _instructions = new List<IInstruction>();
        public List<IInstruction> Instructions => _instructions;

        [SerializeField]
        private string _name;

        public string Name { get => _name; set => _name = value; }

        [SerializeField]
        private bool _isCore;
        public bool IsCore { get => _isCore; set => _isCore = value; }

        public MonaBrainPage()
        {

        }

        public MonaBrainPage(string name, bool isCore)
        {
            _name = name;
            _isCore = isCore;
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            for (var i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i];
                instruction.Preload(brain);
            }
        }

        public void ExecuteInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            for (var i = 0; i < Instructions.Count;i++)
            {
                var instruction = Instructions[i];
                instruction.Execute(eventType, evt);
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