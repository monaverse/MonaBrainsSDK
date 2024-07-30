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
    public class ApplyTorqueSetLimitsInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetTorqueLimits";
        public const string NAME = "Set Torque Limits";
        public const string CATEGORY = "Adv Torque";
        public override Type TileType => typeof(ApplyTorqueSetLimitsInstructionTile);

        [SerializeField] private bool _limitTorqueVelocity = true;
        [SerializeField] private string _limitTorqueVelocityName;
        [BrainProperty(true)] public bool LimitTorqueVelocity { get => _limitTorqueVelocity; set => _limitTorqueVelocity = value; }
        [BrainPropertyValueName("LimitTorqueVelocity", typeof(IMonaVariablesBoolValue))] public string LimitTorqueVelocityName { get => _limitTorqueVelocityName; set => _limitTorqueVelocityName = value; }

        [SerializeField] private float _maxVelocity = 2f;
        [SerializeField] private string _maxVelocityName;
        [BrainPropertyShow(nameof(DisplayMaxVelocity), (int)DisplayType.Display)]
        [BrainProperty(true)] public float MaxVelocity { get => _maxVelocity; set => _maxVelocity = value; }
        [BrainPropertyValueName("MaxVelocity", typeof(IMonaVariablesFloatValue))] public string MaxVelocityName { get => _maxVelocityName; set => _maxVelocityName = value; }

        private IMonaBrain _brain;

        public DisplayType DisplayMaxVelocity => LimitTorqueVelocity ? DisplayType.Display : DisplayType.Hide;

        public ApplyTorqueSetLimitsInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_limitTorqueVelocityName))
                LimitTorqueVelocity = _brain.Variables.GetBool(_limitTorqueVelocityName);

            if (!string.IsNullOrEmpty(_maxVelocityName))
                _maxVelocity = _brain.Variables.GetFloat(_maxVelocityName);

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            Rigidbody rb = _brain.Body.ActiveRigidbody;

            if (!_limitTorqueVelocity)
            {
                rb.maxAngularVelocity = float.PositiveInfinity;
            }
            else if (!rb.isKinematic)
            {
                rb.maxAngularVelocity = _maxVelocity;
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, _maxVelocity);
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}