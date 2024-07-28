using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class SetPhysicsBlendingInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetPhysicsBlending";
        public const string NAME = "Set Physics Blending";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetPhysicsBlendingInstructionTile);

        [SerializeField] private bool _blendForceAndMovement = true;
        [SerializeField] private string _blendForceAndMovementName;
        [BrainProperty(true)] public bool BlendForceAndMovement { get => _blendForceAndMovement; set => _blendForceAndMovement = value; }
        [BrainPropertyValueName("BlendForceAndMovement", typeof(IMonaVariablesBoolValue))] public string BlendForceAndMovementName { get => _blendForceAndMovementName; set => _blendForceAndMovementName = value; }

        [SerializeField] private bool _blendTorqueAndMovement = true;
        [SerializeField] private string _blendTorqueAndMovementName;
        [BrainProperty(true)] public bool BlendTorqueAndMovement { get => _blendTorqueAndMovement; set => _blendTorqueAndMovement = value; }
        [BrainPropertyValueName("BlendTorqueAndMovement", typeof(IMonaVariablesBoolValue))] public string BlendTorqueAndMovementName { get => _blendTorqueAndMovementName; set => _blendTorqueAndMovementName = value; }

        private IMonaBrain _brain;

        public SetPhysicsBlendingInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_blendForceAndMovementName))
                _blendForceAndMovement = _brain.Variables.GetBool(_blendForceAndMovementName);

            if (!string.IsNullOrEmpty(_blendTorqueAndMovementName))
                _blendTorqueAndMovement = _brain.Variables.GetBool(_blendTorqueAndMovementName);

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _brain.Body.BlendLinearForcesAndKinematics = _blendForceAndMovement;
            _brain.Body.BlendAngularForcesAndKinematics = _blendTorqueAndMovement;

            return Complete(InstructionTileResult.Success);
        }
    }
}