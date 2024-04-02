using UnityEngine;
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
    public class SpawnNextAssetInstructionTile : SpawnInstructionTile
    {
        public new const string ID = "SpawnNextAsset";
        public new const string NAME = "Spawn Asset";
        public new const string CATEGORY = "General";
        public override Type TileType => typeof(SpawnNextAssetInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaBodyAssetItem), useProviders:true)] public string MonaAssetProvider { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private bool _shuffled;
        [BrainProperty(true)] public bool Shuffled { get => _shuffled; set => _shuffled = value; }

        protected override List<IMonaBodyAssetItem> GetPreloadAssets()
        {
            return _brain.GetMonaAssetProvider(_monaAsset).AllAssets.ConvertAll<IMonaBodyAssetItem>(x => (IMonaBodyAssetItem)x);
        }

        protected override IMonaBodyAssetItem GetAsset()
        {
            //Debug.Log($"{nameof(GetAsset)} spawn next asset instruction tile");
            var provider = _brain.GetMonaAssetProvider(_monaAsset);
            return (IMonaBodyAssetItem)provider.TakeTopCardOffDeck(_shuffled);
        }

    }
}