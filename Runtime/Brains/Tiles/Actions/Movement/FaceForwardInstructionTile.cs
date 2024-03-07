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

        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress)
        {
            var fwd = _brain.Body.GetVelocity();
            fwd.Normalize();

            if (fwd.magnitude < Mathf.Epsilon)
                return Quaternion.identity;
            else
            {
                if (_lookStraightAhead)
                    fwd.y = 0;

                var rot = _brain.Body.GetRotation();
                _brain.Body.SetRotation(Quaternion.Inverse(rot), true);
                if (progress >= 1f)
                    return Quaternion.RotateTowards(rot, Quaternion.LookRotation(fwd, Vector3.up), .2f);
                else
                    return Quaternion.RotateTowards(rot, Quaternion.LookRotation(fwd, Vector3.up), angle);
            }            
        }
    }
}