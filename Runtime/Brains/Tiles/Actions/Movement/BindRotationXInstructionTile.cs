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
    public class BindRotationXInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "BindRotationX";
        public const string NAME = "Bind Pitch Up / Down";
        public const string CATEGORY = "Rotation Bounds";
        public override Type TileType => typeof(BindRotationXInstructionTile);

        [SerializeField] private float _min = -45f;
        [SerializeField] private string _minName;
        [SerializeField] private float _max = 45f;
        [SerializeField] private string _maxName;

        [BrainProperty(true)]
        public float Min { get => _min; set => _min = value; }

        [BrainPropertyValueName("Min", typeof(IMonaVariablesFloatValue))]
        public string MinName { get => _minName; set => _minName = value; }

        [BrainProperty(true)]
        public float Max { get => _max; set => _max = value; }

        [BrainPropertyValueName("Max", typeof(IMonaVariablesFloatValue))]
        public string MaxName { get => _maxName; set => _maxName = value; }

        private IMonaBrain _brain;

        public BindRotationXInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.RotationBounds.x.Bind(_min, _max);
            return Complete(InstructionTileResult.Success);
        }
    }
}
