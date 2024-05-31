﻿using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class IsGroundedInstructionTile : InstructionTile, IInstructionTileWithPreload,
        IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile,
        ITickAfterInstructionTile
    {
        public const string ID = "IsGrounded";
        public const string NAME = "Is Grounded";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(IsGroundedInstructionTile);

        private IMonaBrain _brain;

        [SerializeField] private GroundingObjectFilter _filter = GroundingObjectFilter.Any;
        [BrainPropertyEnum(true)] public GroundingObjectFilter Target { get => _filter; set => _filter = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(Target), (int)GroundingObjectFilter.Tag)]
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        public IsGroundedInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null && _brain.Body.Grounded)
            {
                if(_filter == GroundingObjectFilter.Tag)
                {
                    if(_brain.Body.CurrentGroundingObject != null && _brain.Body.CurrentGroundingObject.MonaBody != null && _brain.Body.CurrentGroundingObject.MonaBody.HasMonaTag(_tag))
                        return Complete(InstructionTileResult.Success);
                    return Complete(InstructionTileResult.Failure);
                }
                
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}