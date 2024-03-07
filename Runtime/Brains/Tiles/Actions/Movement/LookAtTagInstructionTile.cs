using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Body;
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

        private Quaternion _startRotation;

        protected override void StartRotation()
        {
            _startRotation = _brain.Body.GetRotation();
        }

        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress)
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
                    _brain.Body.SetRotation(Quaternion.Inverse(rot), true);
                    if(progress >= 1f)
                        return Quaternion.RotateTowards(rot, Quaternion.LookRotation(fwd, Vector3.up), .2f);
                    else
                        return Quaternion.RotateTowards(rot, Quaternion.LookRotation(fwd, Vector3.up), angle);
                }
            }
            else
            {
                return Quaternion.identity;
            }            
        }

    }
}