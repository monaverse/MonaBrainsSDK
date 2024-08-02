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
    public class OnInAudienceInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "OnInAudience";
        public const string NAME = "In Audience";
        public const string CATEGORY = "Multiplayer";
        public override Type TileType => typeof(OnInAudienceInstructionTile);

        [SerializeField] private bool _negate;
        [SerializeField] private string _negateName;
        [BrainProperty(true)] public bool Negate { get => _negate; set => _negate = value; }
        [BrainPropertyValueName("Negate", typeof(IMonaVariablesBoolValue))] public string NegateName { get => _negateName; set => _negateName = value; }

        private IMonaBrain _brain;

        public OnInAudienceInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            //if(_brain.LoggingEnabled) Debug.Log($"{nameof(OnInAudienceInstructionTile)} {_brain.Body.HasControl()}", _brain.Body.Transform.gameObject);
            if (MonaGlobalBrainRunner.Instance.PlayerBody.Audience || !(MonaGlobalBrainRunner.Instance.PlayerBody.Audience && _negate))
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOT_CONTROLLED);
        }
    }
}