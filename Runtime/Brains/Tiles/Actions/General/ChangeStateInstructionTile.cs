﻿using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Unity.Profiling;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ChangeStateInstructionTile : InstructionTile, IChangeStateInstructionTile, IActionInstructionTile, IActionStateEndInstructionTile, 
        INeedAuthorityInstructionTile
    {
        public const string ID = "ChangeState";
        public const string NAME = "Change State";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeStateInstructionTile);

        [SerializeField] private string _changeState;
        [SerializeField] private string _stateValueName;
        [BrainProperty] public string State { get => _changeState; set => _changeState = value; }
        
        [BrainPropertyValueName("State", typeof(IMonaVariablesStringValue))] public string StateValueName { get => _stateValueName; set => _stateValueName = value; }

        private IMonaBrain _brain;


        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(ChangeStateInstructionTile)}.{nameof(Do)}");

        public ChangeStateInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            _profilerDo.Begin();
            if (!string.IsNullOrEmpty(_stateValueName))
                _changeState = _brain.Variables.GetString(_stateValueName);

            _brain.BrainState = _changeState;
            //if(_brain.LoggingEnabled)
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}", _brain.Body.Transform.gameObject);
            _profilerDo.End();
            return Complete(InstructionTileResult.Success);
        }
    }
}