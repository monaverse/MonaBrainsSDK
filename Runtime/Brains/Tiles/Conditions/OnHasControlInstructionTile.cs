using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnHasControlInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "OnHasControl";
        public const string NAME = "Has Control";
        public const string CATEGORY = "Multiplayer";
        public override Type TileType => typeof(OnHasControlInstructionTile);

        private IMonaBrain _brain;

        [SerializeField] private bool _negate;
        [SerializeField] private string _negateName;
        [BrainProperty(true)] public bool Negate { get => _negate; set => _negate = value; }
        [BrainPropertyValueName("Negate", typeof(IMonaVariablesBoolValue))] public string NegateName { get => _negateName; set => _negateName = value; }

        public OnHasControlInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var neg = _negate;
            if (!string.IsNullOrEmpty(_negateName))
                neg = _brain.Variables.GetBool(_negateName);

            //if(_brain.LoggingEnabled) Debug.Log($"{nameof(OnHasControlInstructionTile)} {_brain.Body.HasControl()}", _brain.Body.Transform.gameObject);
            if ((!neg && _brain.Body.HasControl()) || (neg && !_brain.Body.HasControl()))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_CONTROLLED);
        }
    }
}