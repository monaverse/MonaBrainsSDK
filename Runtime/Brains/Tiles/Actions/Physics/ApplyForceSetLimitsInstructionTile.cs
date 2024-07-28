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
    public class ApplyForceSetLimitsInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetForceLimits";
        public const string NAME = "Set Force Limits";
        public const string CATEGORY = "Adv Forces";
        public override Type TileType => typeof(ApplyForceSetLimitsInstructionTile);

        [SerializeField] private bool _limitForceVelocity = true;
        [SerializeField] private string _limitForceVelocityName;
        [BrainProperty(true)] public bool LimitForceVelocity { get => _limitForceVelocity; set => _limitForceVelocity = value; }
        [BrainPropertyValueName("LimitForceVelocity", typeof(IMonaVariablesBoolValue))] public string LimitForceVelocityName { get => _limitForceVelocityName; set => _limitForceVelocityName = value; }

        [SerializeField] private float _maxVelocity = 0.2f;
        [SerializeField] private string _maxVelocityName;
        [BrainPropertyShow(nameof(DisplayMaxVelocity), (int)DisplayType.Display)]
        [BrainProperty(true)] public float MaxVelocity { get => _maxVelocity; set => _maxVelocity = value; }
        [BrainPropertyValueName("MaxVelocity", typeof(IMonaVariablesFloatValue))] public string MaxVelocityName { get => _maxVelocityName; set => _maxVelocityName = value; }

        private IMonaBrain _brain;

        public DisplayType DisplayMaxVelocity => LimitForceVelocity ? DisplayType.Display : DisplayType.Hide;

        public ApplyForceSetLimitsInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_limitForceVelocityName))
                _limitForceVelocity = _brain.Variables.GetBool(_limitForceVelocityName);

            if (!string.IsNullOrEmpty(_maxVelocityName))
                _maxVelocity = _brain.Variables.GetFloat(_maxVelocityName);

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            Rigidbody rb = _brain.Body.ActiveRigidbody;

            if (!_limitForceVelocity)
            {
                rb.maxLinearVelocity = float.PositiveInfinity;
            }
            else if (!rb.isKinematic)
            {
                rb.maxLinearVelocity = _maxVelocity;
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, _maxVelocity);
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}