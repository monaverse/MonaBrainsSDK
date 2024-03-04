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
        public bool IsCore => _isCore;

        [SerializeField]
        private bool _isActive;
        public bool IsActive => _isActive;

        public bool HasAnimationTiles()
        {
            for(var i = 0;i < _instructions.Count; i++)
            {
                if (_instructions[i].HasAnimationTiles()) return true;
            }
            return false;
        }

        public bool HasRigidbodyTiles()
        {
            for (var i = 0; i < _instructions.Count; i++)
            {
                if (_instructions[i].HasRigidbodyTiles()) return true;
            }
            return false;
        }

        public bool HasUsePhysicsTileSetToTrue()
        {
            for (var i = 0; i < _instructions.Count; i++)
            {
                if (_instructions[i].HasUsePhysicsTileSetToTrue()) return true;
            }
            return false;
        }

        public MonaBrainPage()
        {

        }

        public MonaBrainPage(string name, bool isCore)
        {
            _name = name;
            _isCore = isCore;
        }

        public void SetIsCore(bool core)
        {
            _isCore = core;
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            for (var i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i];
                instruction.Preload(brain, this);
            }
        }

        public void SetActive(bool active)
        {
            if (_isActive != active)
            {
                _isActive = active;
                for (var i = 0; i < Instructions.Count; i++)
                {
                    var instruction = Instructions[i];
                    instruction.SetActive(active);
                }
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

        public void Pause()
        {
            for (var i = 0; i < _instructions.Count; i++)
                _instructions[i].Pause();
        }

        public void Resume()
        {
            for (var i = 0; i < _instructions.Count; i++)
                _instructions[i].Resume();
        }

        public void Unload()
        {
            for (var i = 0; i < _instructions.Count; i++)
                _instructions[i].Unload();
        }

        public void AddInstruction(Instruction instruction)
        {
            Instructions.Add(instruction);
        }
    }
}