using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class BindToRadiusInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "BindToRadius";
        public const string NAME = "Bind To Radius";
        public const string CATEGORY = "Position Bounds";
        public override Type TileType => typeof(BindToRadiusInstructionTile);

        [SerializeField] private float _radius = 5f;
        [SerializeField] private string _radiusName;
        [BrainProperty(true)] public float Radius { get => _radius; set => _radius = value; }
        [BrainPropertyValueName("Radius", typeof(IMonaVariablesFloatValue))] public string RaiusName { get => _radiusName; set => _radiusName = value; }

        [SerializeField] private Vector3 _origin;
        [SerializeField] private string[] _originName = new string[4];
        [BrainProperty(true)] public Vector3 Origin { get => _origin; set => _origin = value; }
        [BrainPropertyValueName("Origin", typeof(IMonaVariablesVector3Value))] public string[] OriginName { get => _originName; set => _originName = value; }

        private IMonaBrain _brain;

        public BindToRadiusInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_radiusName))
                _radius = _brain.Variables.GetFloat(_radiusName);

            if (HasVector3Values(_originName))
                _origin = GetVector3Value(_brain, _originName);

            _brain.Body.PositionBounds.radius.Bind(_radius, _origin);
            return Complete(InstructionTileResult.Success);
        }
    }
}
