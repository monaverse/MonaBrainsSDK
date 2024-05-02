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

        [SerializeField] private bool _includeChildren = false;
        [BrainProperty(false)] public bool IncludeChildren { get => _includeChildren; set => _includeChildren = value; }

        [SerializeField] private bool _sharedMaterial = false;
        [BrainProperty(false)] public bool SharedMaterial { get => _sharedMaterial; set => _sharedMaterial = value; }

        [SerializeField] private bool _preserveTexture = false;
        [BrainProperty(false)] public bool PreserveTexture { get => !SharedMaterial ? _preserveTexture : false; set => _preserveTexture = value; }

        [SerializeField] private string _textureSlot = "_MainTex";
        [BrainProperty(false)] public string TextureSlot { get => _textureSlot; set => _textureSlot = value; }

        private List<Texture> _textures = new List<Texture>();
        private Renderer[] _renderers;
        private Material[] _materials;

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

        public override InstructionTileResult Do()
        {
            if (_brain == null || _materialAsset.Value == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            var body = _brain.Body;

            OperateOnTextures(body, true);
            LoadMaterial(body, _materialAsset.Value, _sharedMaterial);
            OperateOnTextures(body, false);

            return Complete(InstructionTileResult.Success);
        }

        private void LoadMaterial(IMonaBody body, Material material, bool sharedMaterial)
        {
            if (_includeChildren)
            {
                if (sharedMaterial)
                    body.SetSharedMaterial(material);
                else
                    body.SetMaterial(GameObject.Instantiate(_materialAsset.Value));
            }
            else
            {
                if (sharedMaterial)
                    body.SetBodyMaterial(material, true);
                else
                    body.SetBodyMaterial(GameObject.Instantiate(_materialAsset.Value));
            }
        }

        private void OperateOnTextures(IMonaBody body, bool storeTextures)
        {
            if (!PreserveTexture)
                return;

            if (storeTextures)
                _textures.Clear();

            _renderers = _includeChildren ? body.Renderers : body.BodyRenderers;

            for (int i = 0; i < _renderers.Length; i++)
            {
                _materials = _renderers[i].materials;

                for (int j = 0; j < _materials.Length; j++)
                {
                    if (storeTextures)
                    {
                        Texture texture = _materials[j].GetTexture(_textureSlot);

                        if (texture == null)
                            continue;

                        _textures.Add(texture);
                    }
                    else if (_textures.Count > 0)
                    {
                        _materials[j].SetTexture(_textureSlot, _textures[0]);
                        _textures.RemoveAt(0);
                    }
                }
            }
        }

        public override void Unload(bool destroy = false)
        {
            base.Unload();
        }

    }
}