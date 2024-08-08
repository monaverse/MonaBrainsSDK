﻿using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using System.Collections.Generic;
using Unity.Profiling;

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
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.DefinedAsset)]
        [BrainPropertyMonaAsset(typeof(IMonaMaterialAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private string _monaAssetName = null;
        [BrainPropertyValueName(nameof(MonaAsset), typeof(IMonaVariablesStringValue))] public string MonaAssetName { get => _monaAssetName; set => _monaAssetName = value; }

        [SerializeField] private string _monaAssetGroup = null;
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.IndexInCollection)]
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.NextFromCollection)]
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.RandomFromCollection)]
        [BrainPropertyMonaAsset(typeof(IMonaMaterialAssetItem), useProviders: true)] public string MonaAssetProvider { get => _monaAssetGroup; set => _monaAssetGroup = value; }

        [SerializeField] private string _monaAssetProviderName = null;
        [BrainPropertyValueName(nameof(MonaAssetProvider), typeof(IMonaVariablesStringValue))] public string MonaAssetProviderName { get => _monaAssetProviderName; set => _monaAssetProviderName = value; }

        [SerializeField] private float _atIndex;
        [SerializeField] private string _atIndexName;
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.IndexInCollection)]
        [BrainProperty(true)] public float AtIndex { get => _atIndex; set => _atIndex = value; }
        [BrainPropertyValueName("AtIndex", typeof(IMonaVariablesFloatValue))] public string AtIndexName { get => _atIndexName; set => _atIndexName = value; }

        [SerializeField] private MonaAssetGroupType _assetToUse = MonaAssetGroupType.DefinedAsset;
        [BrainPropertyEnum(false)] public MonaAssetGroupType AssetToUse { get => _assetToUse; set => _assetToUse = value; }

        [SerializeField] private bool _includeAttached = false;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetMaterialType.OnSelectTarget)]
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

        [SerializeField] private bool _onlyBodies = true;
        [BrainProperty(false)] public bool OnlyBodies { get => _onlyBodies; set => _onlyBodies = value; }

        private List<Texture> _textures = new List<Texture>();
        private Renderer[] _renderers;
        private Material[] _materials;

        private IMonaBrain _brain;
        private IMonaMaterialAssetItem _materialAsset;

        public ChangeMaterialInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
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

        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(ChangeMaterialInstructionTile)}.{nameof(Do)}");

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_monaAssetName))
                _monaAsset = _brain.Variables.GetString(_monaAssetName);

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_atIndexName))
                _atIndex = _brain.Variables.GetFloat(_atIndexName);

            if (!string.IsNullOrEmpty(_monaAssetProviderName))
                _monaAssetGroup = _brain.Variables.GetString(_monaAssetProviderName);

            switch (_assetToUse)
            {
                case MonaAssetGroupType.DefinedAsset:
                    _materialAsset = (IMonaMaterialAssetItem)_brain.GetMonaAsset(_monaAsset);
                    break;
                case MonaAssetGroupType.IndexInCollection:
                    var providerIndex = _brain.GetMonaAssetProvider(_monaAssetGroup);
                    _materialAsset = (IMonaMaterialAssetItem)providerIndex.GetMonaAssetByIndex((int)_atIndex);
                    break;
                case MonaAssetGroupType.NextFromCollection:
                    var providerNext = _brain.GetMonaAssetProvider(_monaAssetGroup);
                    _materialAsset = (IMonaMaterialAssetItem)providerNext.TakeTopCardOffDeck(false);
                    break;
                case MonaAssetGroupType.RandomFromCollection:
                    var providerRandom = _brain.GetMonaAssetProvider(_monaAssetGroup);
                    _materialAsset = (IMonaMaterialAssetItem)providerRandom.TakeTopCardOffDeck(true);
                    break;
            }

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
                case MonaBrainTargetMaterialType.OnSelectTarget:
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
            var tagBodies = MonaBodyFactory.FindByTag(_targetTag);

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

        private Dictionary<string, Material> _cachedMaterials = new Dictionary<string, Material>();
        private void LoadMaterial(IMonaBody body, Material material, bool sharedMaterial)
        {
            if (_assetToUse == MonaAssetGroupType.DefinedAsset && (!_cachedMaterials.ContainsKey(_monaAsset) || _cachedMaterials[_monaAsset] == null))
            {
                _cachedMaterials[_monaAsset] = (Material)GameObject.Instantiate(_materialAsset.Value);
                material = _cachedMaterials[_monaAsset];
            }

            if (_onlyBodies)
            {
                if (sharedMaterial)
                    body.SetBodyMaterial(material, true);
                else
                    body.SetBodyMaterial(material);
            }
            else
            {
                if (sharedMaterial)
                    body.SetSharedMaterial(material);
                else
                    body.SetMaterial(material);
            }
        }

        private void OperateOnTextures(IMonaBody body, bool storeTextures)
        {
            if (!PreserveTexture)
                return;

            if (storeTextures)
                _textures.Clear();

            string textureSlot = storeTextures ? _sourceTextureSlot : _newTextureSlot;

            _renderers = _onlyBodies ? body.BodyRenderers : body.Renderers;

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