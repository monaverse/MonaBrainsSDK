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
    public class TeleportToPositionInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "TeleportToPositionInstructionTile";
        public const string NAME = "Teleport To Position";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(TeleportToPositionInstructionTile);

        [SerializeField] private Vector3 _value;
        [SerializeField] private string[] _valueValueName = new string[4];
        [BrainProperty(true)] public Vector3 Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesVector3Value))] public string[] ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private bool _setToLocal = false;
        [BrainProperty(false)] public bool SetToLocal { get => _setToLocal; set => _setToLocal = value; }

        private IMonaBrain _brain;

        public TeleportToPositionInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (HasVector3Values(_valueValueName))
                _value = GetVector3Value(_brain, _valueValueName);

            //Debug.Log($"{nameof(TeleportToPositionInstructionTile)} {_value} {_valueValueName}");
            if (_brain != null)
            {
                _brain.Body.TeleportPosition(_value, true, _setToLocal);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}