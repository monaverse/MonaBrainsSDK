using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class LookDirectionInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "LookDirection";
        public const string NAME = "Look Direction";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(LookDirectionInstructionTile);

        [SerializeField] private Vector3 _directionValue;
        [SerializeField] private string[] _directionValueValueName = new string[4];

        [BrainProperty(true)] public Vector3 DirectionValue { get => _directionValue; set => _directionValue = value; }
        [BrainPropertyValueName(nameof(DirectionValue), typeof(IMonaVariablesVector3Value))] public string[] DirectionValueName { get => _directionValueValueName; set => _directionValueValueName = value; }

        private Quaternion _look;
        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress, bool immediate)
        {
            var fwd = _directionValue;
            var rot = _brain.Body.GetRotation();
            if (HasVector3Values(_directionValueValueName))
                fwd = GetVector3Value(_brain, _directionValueValueName);

            if (immediate)
            {
                _look = Quaternion.LookRotation(fwd, Vector3.up);
                _brain.Body.TeleportRotation(_look, true);
                return Quaternion.identity;
            }
            else
            {
                if (progress == 0f)
                    _look = Quaternion.LookRotation(fwd, Vector3.up);
                _brain.Body.SetRotation(Quaternion.Inverse(rot));
                return Quaternion.RotateTowards(rot, _look, angle);
            }                   
        }
    }
}