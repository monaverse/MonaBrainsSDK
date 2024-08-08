using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{
    [Serializable]
    public class ChangeMeshInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "ChangeMesh";
        public const string NAME = "Change Mesh";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ChangeMeshInstructionTile);

        public bool IsAnimationTile => true;

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.DefinedAsset)]
        [BrainPropertyMonaAsset(typeof(IMonaMeshAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private string _monaAssetName = null;
        [BrainPropertyValueName(nameof(MonaAsset), typeof(IMonaVariablesStringValue))] public string MonaAssetName { get => _monaAssetName; set => _monaAssetName = value; }

        [SerializeField] private string _monaAssetGroup = null;
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.IndexInCollection)]
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.NextFromCollection)]
        [BrainPropertyShow(nameof(AssetToUse), (int)MonaAssetGroupType.RandomFromCollection)]
        [BrainPropertyMonaAsset(typeof(IMonaMeshAssetItem), useProviders:true)] public string MonaAssetProvider { get => _monaAssetGroup; set => _monaAssetGroup = value; }

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
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }

        private IMonaBrain _brain;
        private IMonaMeshAssetItem _meshAsset;

        public ChangeMeshInstructionTile() { }

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
                    case MonaBrainBroadcastType.Self:
                        return false;
                    case MonaBrainBroadcastType.Parent:
                        return false;
                    case MonaBrainBroadcastType.Parents:
                        return false;
                    case MonaBrainBroadcastType.Children:
                        return false;
                    case MonaBrainBroadcastType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_atIndexName))
                _atIndex = _brain.Variables.GetFloat(_atIndexName);

            if (!string.IsNullOrEmpty(_monaAssetProviderName))
                _monaAssetGroup = _brain.Variables.GetString(_monaAssetProviderName);

            switch (_assetToUse)
            {
                case MonaAssetGroupType.DefinedAsset:
                    _meshAsset = (IMonaMeshAssetItem)_brain.GetMonaAsset(_monaAsset);
                    break;
                case MonaAssetGroupType.IndexInCollection:
                    var providerIndex = _brain.GetMonaAssetProvider(_monaAssetGroup);
                    _meshAsset = (IMonaMeshAssetItem)providerIndex.GetMonaAssetByIndex((int)_atIndex);
                    break;
                case MonaAssetGroupType.NextFromCollection:
                    var providerNext = _brain.GetMonaAssetProvider(_monaAssetGroup);
                    _meshAsset = (IMonaMeshAssetItem)providerNext.TakeTopCardOffDeck(false);
                    break;
                case MonaAssetGroupType.RandomFromCollection:
                    var providerRandom = _brain.GetMonaAssetProvider(_monaAssetGroup);
                    _meshAsset = (IMonaMeshAssetItem)providerRandom.TakeTopCardOffDeck(true);
                    break;
            }

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    SetMeshOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    SetMeshOnWholeEntity(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    SetMeshOnParents(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    SetMeshOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    SetMeshOnBody(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    SetMeshOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        SetMeshOnWholeEntity(targetBody);
                    else
                        SetMeshOnBody(targetBody);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetTarget()
        {
            switch (_target)
            {
                case MonaBrainBroadcastType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainBroadcastType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainBroadcastType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainBroadcastType.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }

        private void SetMeshOnTag()
        {
            var tagBodies = MonaBodyFactory.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetMeshOnWholeEntity(tagBodies[i]);
                else
                    SetMeshOnBody(tagBodies[i]);
            }
        }

        private void SetMeshOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetMeshOnBody(topBody);
            SetMeshOnChildren(topBody);
        }

        private void SetMeshOnParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetMeshOnBody(parent);
            SetMeshOnParents(parent);
        }

        private void SetMeshOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetMeshOnBody(children[i]);
                SetMeshOnChildren(children[i]);
            }
        }

        private void SetMeshOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetMeshOnWholeEntity(spawned[i]);
                else
                    SetMeshOnBody(spawned[i]);
            }
        }

        private void SetMeshOnBody(IMonaBody body)
        {
            MeshFilter meshFilter = body.Transform.GetComponent<MeshFilter>();

            if (meshFilter == null)
                return;

            meshFilter.mesh = _meshAsset.Value;
        }

        public override void Unload(bool destroy = false)
        {
            base.Unload();
        }

    }
}