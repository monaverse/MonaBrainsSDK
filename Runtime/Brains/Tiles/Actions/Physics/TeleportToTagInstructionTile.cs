using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class TeleportToTagInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "TeleportToTagInstructionTile";
        public const string NAME = "Teleport To Tag";
        public const string CATEGORY = "Adv Movement";
        public override Type TileType => typeof(TeleportToTagInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private string _offsetName;
        [BrainProperty(false)] public Vector3 Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesVector3Value))]
        public string OffsetName { get => _offsetName; set => _offsetName = value; }

        private IMonaBrain _brain;

        public TeleportToTagInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        } 

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var target = _brain.Body.GetClosestTag(_tag);

            if (target == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_TARGET);

            Vector3 offset = !string.IsNullOrEmpty(_offsetName) ?
                        _brain.Variables.GetVector3(_offsetName) : _offset;

            _brain.Body.TeleportPosition(target.GetPosition() + offset, true);
            return Complete(InstructionTileResult.Success);
        }
    }
}