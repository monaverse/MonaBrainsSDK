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
    public class ChangeMaterialInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "ChangeMaterial";
        public const string NAME = "Change Material Asset";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ChangeMaterialInstructionTile);

        public bool IsAnimationTile => true;

        [SerializeField] private MonaBrainTargetMaterialType _target = MonaBrainTargetMaterialType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainTargetMaterialType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaMaterialAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private string _monaAssetName = null;
        [BrainPropertyValueName(nameof(MonaAsset), typeof(IMonaVariablesStringValue))] public string MonaAssetName { get => _monaAssetName; set => _monaAssetName = value; }

        [SerializeField] private bool _includeAttached = false;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.OnHitTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }

        [SerializeField] private bool _sharedMaterial = false;
        [BrainProperty(false)] public bool SharedMaterial { get => _sharedMaterial; set => _sharedMaterial = value; }

        [SerializeField] private bool _preserveTexture = false;
        [BrainProperty(false)] public bool PreserveTexture { get => !SharedMaterial ? _preserveTexture : false; set => _preserveTexture = value; }

        [SerializeField] private string _sourceTextureSlot = "_MainTex";
        [BrainProperty(false)] public string SourceTextureSlot { get => _sourceTextureSlot; set => _sourceTextureSlot = value; }

        [SerializeField] private string _newTextureSlot = "_MainTex";
        [BrainProperty(false)] public string NewTextureSlot { get => _newTextureSlot; set => _newTextureSlot = value; }

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

        private bool ModifyAllAttached
        {
            get
            {
                switch (_target)
                {
                    case MonaBrainTargetMaterialType.Self:
                        return false;
                    case MonaBrainTargetMaterialType.Parent:
                        return false;
                    case MonaBrainTargetMaterialType.Parents:
                        return false;
                    case MonaBrainTargetMaterialType.Children:
                        return false;
                    case MonaBrainTargetMaterialType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_monaAssetName))
                _monaAsset = _brain.Variables.GetString(_monaAssetName);

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            switch (_target)
            {
                case MonaBrainTargetMaterialType.Tag:
                    SetMaterialOnTag();
                    break;
                case MonaBrainTargetMaterialType.Self:
                    SetMaterialOnWholeEntity(_brain.Body);
                    break;
                case MonaBrainTargetMaterialType.Skybox:
                    RenderSettings.skybox = _materialAsset.Value;
                    break;
                case MonaBrainTargetMaterialType.Parents:
                    SetMaterialOnParents(_brain.Body);
                    break;
                case MonaBrainTargetMaterialType.Children:
                    SetMatieralOnChildren(_brain.Body);
                    break;
                case MonaBrainTargetMaterialType.ThisBodyOnly:
                    SetMaterialOnBody(_brain.Body);
                    break;
                case MonaBrainTargetMaterialType.AllSpawnedByMe:
                    SetMaterialOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        SetMaterialOnWholeEntity(targetBody);
                    else
                        SetMaterialOnBody(targetBody);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetTarget()
        {
            switch (_target)
            {
                case MonaBrainTargetMaterialType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainTargetMaterialType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainTargetMaterialType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainTargetMaterialType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainTargetMaterialType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainTargetMaterialType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainTargetMaterialType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainTargetMaterialType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }

        private void SetMaterialOnTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetMaterialOnWholeEntity(tagBodies[i]);
                else
                    SetMaterialOnBody(tagBodies[i]);
            }
        }

        private void SetMaterialOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetMaterialOnBody(topBody);
            SetMatieralOnChildren(topBody);
        }

        private void SetMaterialOnParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetMaterialOnBody(parent);
            SetMaterialOnParents(parent);
        }

        private void SetMatieralOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetMaterialOnBody(children[i]);
                SetMatieralOnChildren(children[i]);
            }
        }

        private void SetMaterialOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetMaterialOnWholeEntity(spawned[i]);
                else
                    SetMaterialOnBody(spawned[i]);
            }
        }

        private void SetMaterialOnBody(IMonaBody body)
        {
            OperateOnTextures(body, true);
            LoadMaterial(body, _materialAsset.Value, _sharedMaterial);
            OperateOnTextures(body, false);
        }

        private void LoadMaterial(IMonaBody body, Material material, bool sharedMaterial)
        {
            if (sharedMaterial)
                body.SetBodyMaterial(material, true);
            else
                body.SetBodyMaterial(GameObject.Instantiate(_materialAsset.Value));
        }

        private void OperateOnTextures(IMonaBody body, bool storeTextures)
        {
            if (!PreserveTexture)
                return;

            if (storeTextures)
                _textures.Clear();

            string textureSlot = storeTextures ? _sourceTextureSlot : _newTextureSlot;

            _renderers = body.BodyRenderers;

            for (int i = 0; i < _renderers.Length; i++)
            {
                _materials = _renderers[i].materials;

                for (int j = 0; j < _materials.Length; j++)
                {
                    if (storeTextures)
                    {
                        if (!_materials[j].HasTexture(textureSlot))
                            continue;

                        Texture texture = _materials[j].GetTexture(textureSlot);

                        if (texture == null)
                            continue;

                        _textures.Add(texture);
                    }
                    else if (_textures.Count > 0)
                    {
                        _materials[j].SetTexture(textureSlot, _textures[0]);
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