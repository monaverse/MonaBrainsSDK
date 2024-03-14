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
    public class BindPositionYInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "BindPositionY";
        public const string NAME = "Bind Zenith / Nadir";
        public const string CATEGORY = "Position Bounds";
        public override Type TileType => typeof(BindPositionYInstructionTile);

        [SerializeField] private float _min = -10f;
        [SerializeField] private string _minName;
        [SerializeField] private float _max = 10f;
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

        public BindPositionYInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            _brain.Body.PositionBounds.y.Bind(_min, _max);
            return Complete(InstructionTileResult.Success);
        }
    }
}
