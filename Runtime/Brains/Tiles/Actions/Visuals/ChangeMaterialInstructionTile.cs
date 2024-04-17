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
using VRM;
using UniHumanoid;
using Mona.SDK.Brains.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{
    [Serializable]
    public class ChangeMaterialInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "ChangeMaterial";
        public const string NAME = "Change Material Asset";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ChangeMaterialInstructionTile);

        public bool IsAnimationTile => true;

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaMaterialAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private bool _sharedMaterial = false;
        [BrainProperty(false)] public bool SharedMaterial { get => _sharedMaterial; set => _sharedMaterial = value; }

        private IMonaBrain _brain;
        private IMonaMaterialAssetItem _materialAsset;

        public ChangeMaterialInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            SetupMaterial();
        }

        private void SetupMaterial()
        {
            _materialAsset = (IMonaMaterialAssetItem)_brain.GetMonaAsset(_monaAsset);
        }

        private IMonaBody GetTarget()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            var body = GetTarget();
            if (body != null)
            {
                if (_materialAsset.Value != null)
                {
                    LoadMaterial(body, _materialAsset.Value, _sharedMaterial);
                    return Complete(InstructionTileResult.Success);
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        private void LoadMaterial(IMonaBody body, Material material, bool sharedMaterial)
        {
            if (sharedMaterial)
                body.SetSharedMaterial(material);
            else
                body.SetMaterial(GameObject.Instantiate(_materialAsset.Value));
        }

        public override void Unload()
        {
            base.Unload();
        }

    }
}