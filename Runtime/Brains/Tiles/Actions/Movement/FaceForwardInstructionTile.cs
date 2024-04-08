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
    public class FaceForwardInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "FaceForward";
        public const string NAME = "Face Forward";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(FaceForwardInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.None;

        [BrainProperty(false)] public bool LookStraightAhead { get => _lookStraightAhead; set => _lookStraightAhead = value; }

        public FaceForwardInstructionTile() { }

        private Quaternion _startRotation;

        protected override void StartRotation()
        {
            _startRotation = _brain.Body.GetRotation();
        }

        private Vector3 _lastPosition;
        private Quaternion _look;
        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress, bool immediate)
        {
            var fwd = _brain.Body.GetVelocity();
            var last = _lastPosition;
            _lastPosition = _brain.Body.GetPosition();

            if (fwd.magnitude < .1f || Vector3.Distance(last, _brain.Body.GetPosition()) < .01f)
                return Quaternion.identity;
            else
            {
                fwd.Normalize();

                if (_lookStraightAhead)
                    fwd.y = 0;

                if (fwd.magnitude < .2f)
                    return Quaternion.identity;

                //Debug.Log($"{nameof(FaceForwardInstructionTile)} {moveType} {fwd} {_bodyInput.MoveValue}");
                var rot = _brain.Body.GetRotation();
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
}