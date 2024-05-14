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

        [SerializeField] private string _monaAssetName = null;
        [BrainPropertyValueName(nameof(MonaAsset), typeof(IMonaVariablesStringValue))] public string MonaAssetName { get => _monaAssetName; set => _monaAssetName = value; }

        protected override List<IMonaBodyAssetItem> GetPreloadAssets()
        {
            return new List<IMonaBodyAssetItem>() { (IMonaBodyAssetItem)_brain.GetMonaAsset(_monaAsset) };
        }

        IMonaBodyAssetItem _lastAsset;
        string _lastAssetName;

        protected override IMonaBodyAssetItem GetAsset()
        {
            if (!string.IsNullOrEmpty(_monaAssetName))
                _monaAsset = _brain.Variables.GetString(_monaAssetName);

            if(_lastAssetName != _monaAsset)
                _lastAsset = (IMonaBodyAssetItem)_brain.GetMonaAsset(_monaAsset);

            _lastAssetName = _monaAsset;

            return _lastAsset;
        }
    }
}