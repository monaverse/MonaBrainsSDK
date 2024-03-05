﻿using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class TeleportToRotationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "TeleportToRotationInstructionTile";
        public const string NAME = "Teleport To Rotation";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(TeleportToRotationInstructionTile);

        [SerializeField] private Vector3 _value;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public Vector3 Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesVector3Value))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;

        public TeleportToRotationInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        } 

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetVector3(_valueValueName);

            //Debug.Log($"{nameof(TeleportToRotationInstructionTile)} {_value} {_valueValueName}");
            if (_brain != null)
            {
                _brain.Body.TeleportRotation(Quaternion.Euler(_value), true);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}