﻿using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Animation;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class SpawnAssetInstructionTile : SpawnInstructionTile
    {
        public new const string ID = "SpawnAsset";
        public new const string NAME = "Spawn Asset";
        public new const string CATEGORY = "General";
        public override Type TileType => typeof(SpawnAssetInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaBodyAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        protected override List<IMonaBodyAssetItem> GetPreloadAssets()
        {
            return new List<IMonaBodyAssetItem>() { (IMonaBodyAssetItem)_brain.GetMonaAsset(_monaAsset) };
        }

        protected override IMonaBodyAssetItem GetAsset()
        {
            return (IMonaBodyAssetItem)_brain.GetMonaAsset(_monaAsset);
        }
    }
}