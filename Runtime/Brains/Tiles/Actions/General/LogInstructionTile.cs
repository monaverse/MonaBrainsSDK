using Mona.SDK.Brains.Core.Enums;
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
                var value = _brain.State.GetValue(_messageValueName);
                if (value is IMonaStateStringValue) _message = ((IMonaStateStringValue)value).Value;
                if (value is IMonaStateFloatValue) _message = ((IMonaStateFloatValue)value).Value.ToString();
                if (value is IMonaStateBoolValue) _message = ((IMonaStateBoolValue)value).Value.ToString();
                if (value is IMonaStateVector2Value) _message = ((IMonaStateVector2Value)value).Value.ToString();
                if (value is IMonaStateVector3Value) _message = ((IMonaStateVector3Value)value).Value.ToString();
            }

            Debug.Log(_message);
            return Complete(InstructionTileResult.Success);
        }
    }
}