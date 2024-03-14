using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class LookAtTagInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "LookAtTag";
        public const string NAME = "Look At Tag";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(LookAtTagInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.None;

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [BrainProperty(false)] public bool LookStraightAhead { get => _lookStraightAhead; set => _lookStraightAhead = value; }

        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Angles/Sec")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.SpeedOnly, "Angles/Sec")]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.SpeedOnly)]
        [BrainProperty(false)] public float Angle { get => _angle; set => _angle = value; }
        [BrainPropertyValueName("Angle", typeof(IMonaVariablesFloatValue))] public string AngleValueName { get => _angleValueName; set => _angleValueName = value; }

        private Quaternion _startRotation;

        protected override void StartRotation()
        {
            _startRotation = _brain.Body.GetRotation();
        }

        private Quaternion _look;
        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress, bool immediate)
        {
            var tags = MonaBody.FindByTag(_tag);
            IMonaBody body;
            if (tags.Count > 0)
            {
                tags.Sort((a, b) =>
                {
                    var dista = Vector3.Distance(_brain.Body.GetPosition(), a.GetPosition());
                    var distb = Vector3.Distance(_brain.Body.GetPosition(), b.GetPosition());
                    return dista.CompareTo(distb);
                });
                body = tags[0];
                var fwd = body.GetPosition() - _brain.Body.GetPosition();
                fwd.Normalize();

                if (_lookStraightAhead)
                    fwd.y = 0;
                
                if (fwd.magnitude < Mathf.Epsilon)
                    return Quaternion.identity;
                else
                {
                    if (_lookStraightAhead)
                        fwd.y = 0;

                    var rot = _brain.Body.GetRotation();
                    if (immediate)
                    {
                        _look = Quaternion.LookRotation(fwd, Vector3.up);
                        _brain.Body.TeleportRotation(_look, true);
                        return Quaternion.identity;
                    }
                    else
                    {
                        if(progress == 0f)
                            _look = Quaternion.LookRotation(fwd, Vector3.up);
                        _brain.Body.SetRotation(Quaternion.Inverse(rot));
                        return Quaternion.RotateTowards(rot, _look, angle);
                    }
                }
            }
            else
            {
                return Quaternion.identity;
            }            
        }

    }
}