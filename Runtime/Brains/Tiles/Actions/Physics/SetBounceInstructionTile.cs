﻿using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class SetBounceInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetBounce";
        public const string NAME = "Set Bounce";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetBounceInstructionTile);

        [SerializeField] private float _value = 0.2f;
        [SerializeField] private string _valueValueName;
        [BrainProperty(true)] public float Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private IMonaBrain _brain;
        private List<Collider> _colliders;

        public SetBounceInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            if (!_brain.Body.HasCollider())
                _colliders = _brain.Body.AddCollider();
        }

        public override void Unload(bool destroy = false)
        {
            if (destroy)
            {
                if (_colliders != null)
                {
                    for (var i = 0; i < _colliders.Count; i++)
                        GameObject.Destroy(_colliders[i]);
                }
                _colliders = null;
            }
        }
        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);

            _brain.Body.SetBounce(_value);

            return Complete(InstructionTileResult.Success);
        }
    }
}