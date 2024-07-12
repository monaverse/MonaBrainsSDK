using UnityEngine;
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
    public class SetRigidbodyConstraintsInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetRigidbodyConstraints";
        public const string NAME = "Freeze Constraints";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetRigidbodyConstraintsInstructionTile);

        [SerializeField] private bool _xPosition = false;
        [SerializeField] private string _xPositionValueName;
        [BrainProperty(true)] public bool XPosition { get => _xPosition; set => _xPosition = value; }
        [BrainPropertyValueName(nameof(XPosition), typeof(IMonaVariablesBoolValue))] public string XPositionValueName { get => _xPositionValueName; set => _xPositionValueName = value; }

        [SerializeField] private bool _yPosition = false;
        [SerializeField] private string _yPositionValueName;
        [BrainProperty(true)] public bool YPosition { get => _yPosition; set => _yPosition = value; }
        [BrainPropertyValueName(nameof(YPosition), typeof(IMonaVariablesBoolValue))] public string YPositionValueName { get => _yPositionValueName; set => _yPositionValueName = value; }

        [SerializeField] private bool _zPosition = false;
        [SerializeField] private string _zPositionValueName;
        [BrainProperty(true)] public bool ZPosition { get => _zPosition; set => _zPosition = value; }
        [BrainPropertyValueName(nameof(ZPosition), typeof(IMonaVariablesBoolValue))] public string ZPositionValueName { get => _zPositionValueName; set => _zRotationValueName = value; }

        [SerializeField] private bool _xRotation = false;
        [SerializeField] private string _xRotationValueName;
        [BrainProperty(true)] public bool XRotation { get => _xRotation; set => _xRotation = value; }
        [BrainPropertyValueName(nameof(XRotation), typeof(IMonaVariablesBoolValue))] public string XRotationValueName { get => _xRotationValueName; set => _xRotationValueName = value; }

        [SerializeField] private bool _yRotation = false;
        [SerializeField] private string _yRotationValueName;
        [BrainProperty(true)] public bool YRotation { get => _yRotation; set => _yRotation = value; }
        [BrainPropertyValueName(nameof(YRotation), typeof(IMonaVariablesBoolValue))] public string YRotationValueName { get => _yRotationValueName; set => _yRotationValueName = value; }

        [SerializeField] private bool _zRotation = false;
        [SerializeField] private string _zRotationValueName;
        [BrainProperty(true)] public bool ZRotation { get => _zRotation; set => _zRotation = value; }
        [BrainPropertyValueName(nameof(ZRotation), typeof(IMonaVariablesBoolValue))] public string ZRotationValueName { get => _zRotationValueName; set => _zRotationValueName = value; }

        private IMonaBrain _brain;
        private List<Collider> _colliders;

        public SetRigidbodyConstraintsInstructionTile() { }

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

            bool fx, fy, fz, px, py, pz;
            fx = _xRotation;
            fy = _yRotation;
            fz = _zRotation;
            px = _xPosition;
            py = _yPosition;
            pz = _zPosition;

            if (!string.IsNullOrEmpty(_xRotationValueName))
                fx = _brain.Variables.GetBool(_xRotationValueName);

            if (!string.IsNullOrEmpty(_yRotationValueName))
                fy = _brain.Variables.GetBool(_yRotationValueName);

            if (!string.IsNullOrEmpty(_zRotationValueName))
                fz = _brain.Variables.GetBool(_zRotationValueName);

            if (!string.IsNullOrEmpty(_xPositionValueName))
                px = _brain.Variables.GetBool(_xPositionValueName);

            if (!string.IsNullOrEmpty(_yPositionValueName))
                py = _brain.Variables.GetBool(_yPositionValueName);

            if (!string.IsNullOrEmpty(_zPositionValueName))
                pz = _brain.Variables.GetBool(_zPositionValueName);


            var constraints = RigidbodyConstraints.None;
            if (fx) constraints |= RigidbodyConstraints.FreezeRotationX;
            if (fy) constraints |= RigidbodyConstraints.FreezeRotationY;
            if (fz) constraints |= RigidbodyConstraints.FreezeRotationZ;
            if (px) constraints |= RigidbodyConstraints.FreezePositionX;
            if (py) constraints |= RigidbodyConstraints.FreezePositionY;
            if (pz) constraints |= RigidbodyConstraints.FreezePositionZ;

            if(_brain.Body.ActiveRigidbody != null)
                _brain.Body.ActiveRigidbody.constraints = constraints;

            return Complete(InstructionTileResult.Success);
        }
    }
}