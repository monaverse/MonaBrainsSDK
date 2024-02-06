﻿using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class LogInstructionTile : InstructionTile, ILogInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "Log";
        public const string NAME = "Log";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(LogInstructionTile);

        [SerializeField] private string _message;
        [SerializeField] private string _messageValueName;

        [BrainProperty] public string Message { get => _message; set => _message = value; }
        [BrainPropertyValueName("Message")] public string MessageValueName { get => _messageValueName; set => _messageValueName = value; }

        public LogInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_messageValueName))
            {
                var variable = _brain.Variables.GetVariable(_messageValueName);
                if (variable is IMonaVariablesStringValue) _message = ((IMonaVariablesStringValue)variable).Value;
                if (variable is IMonaVariablesFloatValue) _message = ((IMonaVariablesFloatValue)variable).Value.ToString();
                if (variable is IMonaVariablesBoolValue) _message = ((IMonaVariablesBoolValue)variable).Value.ToString();
                if (variable is IMonaVariablesVector2Value) _message = ((IMonaVariablesVector2Value)variable).Value.ToString();
                if (variable is IMonaVariablesVector3Value) _message = ((IMonaVariablesVector3Value)variable).Value.ToString();
            }

            Debug.Log(_message);
            return Complete(InstructionTileResult.Success);
        }
    }
}