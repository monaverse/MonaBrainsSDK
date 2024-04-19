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
    public class ChangeTextureInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "ChangeTexture";
        public const string NAME = "Change Texture";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ChangeTextureInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaTextureAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private string _textureSlot = "_MainTex";
        [BrainProperty(false)] public string TextureSlot { get => _textureSlot; set => _textureSlot = value; }

        [SerializeField] private bool _sharedMaterial = false;
        [BrainProperty(false)] public bool SharedMaterial { get => _sharedMaterial; set => _sharedMaterial = value; }
        
        private IMonaBrain _brain;
        private IMonaTextureAssetItem _textureAsset;

        public ChangeTextureInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            SetupTexture();
        }

        private void SetupTexture()
        {
            _textureAsset = (IMonaTextureAssetItem)_brain.GetMonaAsset(_monaAsset);
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
                if (_textureAsset.Value != null)
                {
                    ChangeTexture(body, _textureAsset.Value, _textureSlot, _sharedMaterial);
                    return Complete(InstructionTileResult.Success);
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        private void ChangeTexture(IMonaBody body, Texture texture, string textureSlot, bool sharedMaterial)
        {
            body.SetTexture(texture, textureSlot, sharedMaterial);
        }

        public override void Unload(bool destroy = false)
        {
            base.Unload();
        }

    }
}