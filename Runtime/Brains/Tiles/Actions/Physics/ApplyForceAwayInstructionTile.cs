using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core.Body;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class ApplyForceAwayInstructionTile : ApplyForceLocalInstructionTile
    {
        public const string ID = "ApplyForceAway";
        public const string NAME = "Apply Force Away";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(ApplyForceAwayInstructionTile);

        public override PushDirectionType DirectionType => PushDirectionType.Away;

        [SerializeField] private float _yFactor = 1f;
        [BrainProperty(false)] public float YFactor { get => _yFactor; set => _yFactor = value; }

        protected override Vector3 GetDirectionVector(PushDirectionType moveType, IMonaBody body)
        {
            var dir = base.GetDirectionVector(moveType, body);
            dir.y *= _yFactor;
            return dir;
        }
    }
}