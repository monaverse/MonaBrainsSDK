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
using Mona.SDK.Core.Utils;
using Unity.Profiling;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.ThirdParty.Redcode.Awaiting;
using Mona.SDK.Brains.Tiles.Actions.Character;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class SpawnInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "Spawn";
        public const string NAME = "Spawn Asset";
        public const string CATEGORY = "Spawning";
        public override Type TileType => typeof(SpawnInstructionTile);

        static readonly ProfilerMarker _profilerPreload = new ProfilerMarker($"MonaBrains.{nameof(SpawnAssetInstructionTile)}.{nameof(Preload)}");
        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(SpawnAssetInstructionTile)}.{nameof(Do)}");

        [SerializeField] private float _poolCount = 5;
        [SerializeField] private string _poolCountName;

        [BrainProperty(true)]
        public float PoolCount { get => _poolCount; set => _poolCount = value; }
        [BrainPropertyValueName("PoolCount", typeof(IMonaVariablesFloatValue))]
        public string PoolCountName { get => _poolCountName; set => _poolCountName = value; }

        [SerializeField] private LocationType _location;
        [BrainPropertyEnum(false)] public LocationType Location { get => _location; set => _location = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(Location), (int)LocationType.OtherWithTag)]
        [BrainPropertyShow(nameof(Location), (int)LocationType.OtherWithTagPart)]
        [BrainPropertyMonaTag(false)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField]
        private string _part = "Default";
        [BrainPropertyMonaTag(false)]
        [BrainPropertyShow(nameof(Location), (int)LocationType.MyPart)]
        [BrainPropertyShow(nameof(Location), (int)LocationType.OtherWithTagPart)]
        public string Part { get => _part; set => _part = value; }

        [SerializeField] private bool _spawnAsChild;
        [SerializeField] private string _spawnAsChildName;
        [BrainProperty(false)] public bool SpawnAsChild { get => _spawnAsChild; set => _spawnAsChild = value; }
        [BrainPropertyValueName("SpawnAsChild", typeof(IMonaVariablesBoolValue))] public string SpawnAsChildName { get => _spawnAsChildName; set => _spawnAsChildName = value; }

        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private string[] _offsetName = new string[4];
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesVector3Value))]
        public string[] OffsetName { get => _offsetName; set => _offsetName = value; }

        [SerializeField] private Vector3 _eulerAngles = Vector3.zero;
        [SerializeField] private string[] _eulerAnglesName = new string[4];

        [BrainProperty(false)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }
        [BrainPropertyValueName("Rotation", typeof(IMonaVariablesVector3Value))]
        public string[] EulerAnglesName { get => _eulerAnglesName; set => _eulerAnglesName = value; }

        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private string[] _scaleName = new string[4];
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }
        [BrainPropertyValueName("Scale", typeof(IMonaVariablesVector3Value))]
        public string[] ScaleName { get => _scaleName; set => _scaleName = value; }

        [SerializeField] private bool _scaleToFit;
        [BrainProperty(false)] public bool ScaleToFit { get => _scaleToFit; set => _scaleToFit = value; }

        [SerializeField] private bool _bottomPivot;
        [BrainProperty(false)] public bool BottomPivot { get => _bottomPivot; set => _bottomPivot = value; }

        [SerializeField] private bool _importLights;
        [BrainProperty(false)] public bool ImportLights { get => _importLights; set => _importLights = value; }

        [SerializeField] private bool _hidden;
        [BrainProperty(false)] public bool Hidden { get => _hidden; set => _hidden = value; }

        [SerializeField] private bool _spawnOnEmpty;
        [BrainProperty(false)] public bool SpawnOnEmpty { get => _spawnOnEmpty; set => _spawnOnEmpty = value; }

        [SerializeField] private bool _destroyOnDisable;
        [BrainProperty(false)] public bool DestroyOnDisable { get => _destroyOnDisable; set => _destroyOnDisable = value; }

        [SerializeField] private bool _enableAll;
        [BrainProperty(false)] public bool EnableAll { get => _enableAll; set => _enableAll = value; }

        [SerializeField] private bool _ownedByMe;
        [BrainProperty(false)] public bool OwnedByMe { get => _ownedByMe; set => _ownedByMe = value; }

        [SerializeField] private bool _networked = true;
        [BrainProperty(false)] public bool Networked { get => _networked; set => _networked = value; }

        protected IMonaBrain _brain;
        private Transform _defaultParent;
        private IMonaBodyAssetItem _item;
        private List<IMonaBody> _equipmentInstances = new List<IMonaBody>();
        private Dictionary<string, List<IMonaBody>> _pool = new Dictionary<string, List<IMonaBody>>();
        private bool _shouldSpawn = true;

        public enum LocationType
        {
            Me,
            MyPart,
            OtherWithTag,
            OtherWithTagPart,
            LastSpawnedByMe = 80
        }

        public SpawnInstructionTile() { }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public void Preload(IMonaBrain brainInstance) 
        {
            _profilerPreload.Begin();
            _brain = brainInstance;
            _defaultParent = GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform;

            if (_glbLoader == null)
                _glbLoader = new GameObject("GlbLoader");

            _urlLoader = _glbLoader.GetComponent<BrainsGlbLoader>();
            if (_urlLoader == null)
                _urlLoader = _glbLoader.AddComponent<BrainsGlbLoader>();

            SetupSpawnable();
            _profilerPreload.End();
        }

        protected virtual List<IMonaBodyAssetItem> GetPreloadAssets()
        {
            return null;
        }

        protected virtual IMonaBodyAssetItem GetAsset()
        {
            return null;
        }

        private void SetupSpawnable()
        {
            if (!_shouldSpawn) return;

            _shouldSpawn = false;

            //Debug.Log($"{nameof(SetupSpawnable)} spawn asset instruction tile");
            var items = GetPreloadAssets();

            int poolCount = !string.IsNullOrEmpty(_poolCountName) ? (int)Mathf.Ceil(_brain.Variables.GetFloat(_poolCountName)) : (int)Mathf.Ceil(_poolCount);

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null) return;
                if (_brain.Body.IsAttachedToRemotePlayer()) return;
                if (!_brain.Body.HasControl()) return;

                for (var j = 0; j < poolCount; j++)
                {
                    if (!string.IsNullOrEmpty(item.Url))
                        Spawn(item.PrefabId, item.Url);
                    else
                        Spawn(item.PrefabId, item.Value);
                }
            }
        }

        protected void Spawn(string prefabId, string url, bool disable = true, Action<GameObject> callback = null)
        {
            _urlLoader.Load(url, false, async (glb) =>
            {
                if (glb != null)
                {
                    glb.SetActive(true);

                    if (!_importLights)
                    {
                        var lights = glb.transform.GetComponentsInChildren<Light>(true);
                        for (var i = 0; i < lights.Length; i++)
                            lights[i].gameObject.SetActive(false);
                    }

                    var guid = Guid.NewGuid();
                    var body = glb.GetComponent<IMonaBody>();
                    if (body == null)
                        body = glb.AddComponent<MonaBody>();

                    body.SyncType = _networked ? MonaBodyNetworkSyncType.NetworkTransform : MonaBodyNetworkSyncType.NotNetworked;
                    body = MonaBodyBase.Spawn(guid, prefabId, 0, (MonaBody)body, false, Vector3.up*10000f, Quaternion.identity, _ownedByMe);

                    var bodies = glb.GetComponentsInChildren<IMonaBody>();
                    Debug.Log($"GENERATE GUID {guid} {prefabId}");
                    for (var j = 0; j < bodies.Length; j++)
                    {
                        var child = bodies[j];
                        if (child == body)
                        {
                            child.Transform.SetParent(_defaultParent);
                            _equipmentInstances.Add(child);

                            if (!_pool.ContainsKey(prefabId))
                                _pool.Add(prefabId, new List<IMonaBody>());

                            child.OnBodyDisabled += HandleBodyDisabled;
                            if (disable)
                            {
                                child.SetDisableOnLoad(true);
                                //Debug.Log($"{nameof(child.SetDisableOnLoad)}", child.Transform.gameObject);
                            }
                            if (_hidden)
                                child.SetVisible(false);
                        }
                    }
                }
                callback?.Invoke(glb);
            }, (int)_poolCount);
        }

        private GameObject _glbLoader;
        private BrainsGlbLoader _urlLoader;

        protected IMonaBody Spawn(string prefabId, MonaBody monaBody, bool disable = true)
        {

            if (!_importLights)
            {
                var lights = monaBody.Transform.GetComponentsInChildren<Light>(true);
                for (var i = 0; i < lights.Length; i++)
                    lights[i].gameObject.SetActive(false);
            }

            var guid = Guid.NewGuid();
            var body = MonaBodyBase.Spawn(guid, prefabId, 0, monaBody, true, Vector3.up*10000f, Quaternion.identity, _ownedByMe);
                body.SyncType = _networked ? MonaBodyNetworkSyncType.NetworkTransform : MonaBodyNetworkSyncType.NotNetworked;

            var bodies = body.Transform.GetComponentsInChildren<IMonaBody>();
            for (var j = 0; j < bodies.Length; j++)
            {
                var child = bodies[j];
                if (child == body)
                {
                    child.Transform.SetParent(_defaultParent);
                    _equipmentInstances.Add(child);

                    if (!_pool.ContainsKey(prefabId))
                        _pool.Add(prefabId, new List<IMonaBody>());

                    child.OnBodyDisabled += HandleBodyDisabled;
                    if (disable)
                    {
                        child.SetDisableOnLoad(true);
                        //Debug.Log($"{nameof(child.SetDisableOnLoad)}", child.Transform.gameObject);
                    }
                    if (_hidden)
                        child.SetVisible(false);
                }
            }
            return body;
        }

        private void HandleBodyDisabled(IMonaBody body)
        {
            body.OnAfterEnabled -= HandleAfterEnabled;

            if (_brain.SpawnedBodies.Contains(body))
                _brain.SpawnedBodies.Remove(body);

            if (_destroyOnDisable)
                body.Destroy();
            else
            {
                //Debug.Log($"{nameof(HandleBodyDisabled)} return to pool", body.Transform.gameObject);
                if (!_pool[((MonaBodyBase)body).PrefabId].Contains(body))
                    _pool[((MonaBodyBase)body).PrefabId].Add(body);
            }
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private IMonaBody GetBody()
        {
            var body = _brain.Body;

            switch (_location)
            {
                case LocationType.MyPart:
                    body = body.FindChildByTag(_part);
                    break;

                case LocationType.OtherWithTag:
                    return GetTargetBody();

                case LocationType.OtherWithTagPart:
                    var otherPartBody = GetTargetBody();

                    if (otherPartBody == null)
                        return null;

                    var otherPart = otherPartBody.FindChildByTag(_part);
                    return otherPart != null ? otherPart : otherPartBody;
                case LocationType.LastSpawnedByMe:
                    var lastSpawnedBody = _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                    return lastSpawnedBody != null ? lastSpawnedBody : body;
            }
            
            return body;
        }

        private IMonaBody GetTargetBody()
        {
            if (_brain.MonaTagSource.GetTag(_tag).IsPlayerTag && _brain.Player.PlayerBody != null)
            {
                return _brain.Player.PlayerBody;
            }
            else
            {
                var bodies = MonaBody.FindByTag(_tag);
                if (bodies != null && bodies.Count > 0)
                {
                    var body = bodies[0];
                    return body;
                }
            }

            return null;
        }

        public override InstructionTileResult Do()
        {
            _profilerDo.Begin();
            var body = GetBody();

            if (body == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_spawnAsChildName))
                _spawnAsChild = _brain.Variables.GetBool(_spawnAsChildName);

            if (_brain.HasPlayerTag(body.MonaTags))
                _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

            if (_enableAll)
            {
                int poolCount = !string.IsNullOrEmpty(_poolCountName) ? (int)Mathf.Ceil(_brain.Variables.GetFloat(_poolCountName)) : (int)Mathf.Ceil(_poolCount);
                _spawnOnEmpty = true;
                for (var i = 0; i < poolCount; i++)
                    EnableSpawn(i);
                _profilerDo.End();
                return InstructionTileResult.Running;
            }
            else
            {
                _profilerDo.End();
                return EnableSpawn(0);
            }
        }

        private InstructionTileResult EnableSpawn(int index)
        {
            var body = GetBody();
            var nextItem = GetAsset();

            if (nextItem == null)
                return Complete(InstructionTileResult.Failure);

            if (!_pool.ContainsKey(nextItem.PrefabId))
                _pool.Add(nextItem.PrefabId, new List<IMonaBody>());

            if (_pool[nextItem.PrefabId].Count < 1 && !_spawnOnEmpty)
                return Complete(InstructionTileResult.Failure);

            if (_pool[nextItem.PrefabId].Count == 0)
            {
                if (!string.IsNullOrEmpty(nextItem.Url))
                {
                    Spawn(nextItem.PrefabId, nextItem.Url, disable: false, (GameObject spawn) =>
                    {
                        if (spawn == null) return;

                        var poolItem = spawn.GetComponent<IMonaBody>();

                        if (poolItem == null)
                            poolItem = spawn.AddComponent<MonaBody>();
                        
                        ContinueEnableSpawn(body, poolItem, index);
                    });
                    return InstructionTileResult.Running;
                }
                else
                {
                    _pool[nextItem.PrefabId].Add(Spawn(nextItem.PrefabId, nextItem.Value, disable: false));
                }

            }

            var poolItem = _pool[nextItem.PrefabId][0];
            _pool[nextItem.PrefabId].RemoveAt(0);

            ContinueEnableSpawn(body, poolItem, index);
            return InstructionTileResult.Running;
        }

        private InstructionTileResult ContinueEnableSpawn(IMonaBody body, IMonaBody poolItem, int index)
        { 
            poolItem.ChildIndex = index;

            //Debug.Log($"{nameof(SpawnInstructionTile)} {poolItem}", poolItem.Transform.gameObject);

            if (poolItem.ActiveRigidbody != null)
                poolItem.ActiveRigidbody.WakeUp();

            Transform parent;

            if (_spawnAsChild)
                parent = body.Transform;
            else
                parent = _defaultParent;

            poolItem.SetTransformParent(parent);

            poolItem.Transform.localScale = Vector3.one;
            var parsed = ChangeAvatarInstructionTile.ParseHumanoid(poolItem.Transform.gameObject);

            RecalculateSize(poolItem, parsed.Avatar != null);

            poolItem.OnAfterEnabled -= HandleAfterEnabled;
            poolItem.OnAfterEnabled += HandleAfterEnabled;

            poolItem.SetActive(true);
            poolItem.SetVisible(false);

            var childBrains = poolItem.Transform.GetComponentsInChildren<IMonaBrainRunner>();
            for (var i = 0; i < childBrains.Length; i++)
                childBrains[i].CacheTransforms();

            if(!_hidden)
                poolItem.SetVisible(true);

            IMonaBody previouslySpawnedBody = _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);

            _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, poolItem);
            _brain.Variables.Set(MonaBrainConstants.RESULT_LAST_SPAWNED, poolItem);
            _brain.SpawnedBodies.Add(poolItem);
            SetSpawnerReferenceOnSpawned(poolItem, previouslySpawnedBody);

            if (previouslySpawnedBody != null)
                SetNextBodyReferenceOnPrevious(previouslySpawnedBody, poolItem);


            //Debug.Log($"{nameof(SpawnInstructionTile)} SPAWN COMPLETE: {poolItem}", poolItem.Transform.gameObject);

            MonaEventBus.Trigger<MonaChangeSpawnEvent>(new EventHook(MonaCoreConstants.ON_CHANGE_SPAWN_EVENT), new MonaChangeSpawnEvent(poolItem));

            return Complete(InstructionTileResult.Running);

        }


        private void RecalculateSize(IMonaBody avatar, bool isAvatar)
        {
            var offset = _offset;
            if (HasVector3Values(_offsetName))
                offset = GetVector3Value(_brain, _offsetName);

            var scale = _scale;
            if (HasVector3Values(_scaleName))
                scale = GetVector3Value(_brain, _scaleName);

            var eulerAngles = _eulerAngles;
            if (HasVector3Values(_eulerAnglesName))
                eulerAngles = GetVector3Value(_brain, _eulerAnglesName);

            var avatarGameObject = avatar.Transform.gameObject;
            var bounds = GetBounds(avatarGameObject.gameObject);
            var extents = bounds.extents * 2f;
            var avatarFactor = isAvatar ? .2f : 1f;

            extents.x *= avatarFactor;
            extents.z *= avatarFactor;

            Debug.Log($"extents: {extents} scale:{scale}");

            var boxScale = new Vector3(scale.x / extents.x, scale.y / extents.y, scale.z / extents.z);
            var index = 0;
            var max = extents.x;
            if (extents.y > max)
            {
                index = 1;
                max = extents.y;
            }
            if (extents.z > max) index = 2;

            var maxBoxScale = boxScale[index];
            extents *= maxBoxScale;
            Debug.Log($"extents: {extents} {maxBoxScale}");

            var mostOverlap = -1;
            var mostOverlapDistance = 0f;
            for (var i = 0; i < 3; i++)
            {
                if (index != i)
                {
                    if (extents[i] > scale[i])
                    {
                        var d = extents[i] - scale[i];
                        if (d > mostOverlapDistance)
                        {
                            mostOverlap = i;
                            mostOverlapDistance = d;
                        }
                    }
                }
            }

            if (mostOverlap > -1)
                maxBoxScale *= scale[mostOverlap] / extents[mostOverlap];

            extents = bounds.extents * 2f * maxBoxScale;
            Debug.Log($"extents: {extents} {maxBoxScale} orig {bounds.extents * 2f} d:{mostOverlapDistance} i:{mostOverlap}");
            //var max = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
            //var maxScale = Mathf.Max(Mathf.Max(scale.x, scale.y), _cale.z);
            //var scale = maxScale / max;



            var offsetY = Vector3.up * (bounds.center.y - bounds.extents.y);
            if (!_bottomPivot)
                offsetY = Vector3.zero;
            /*
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} center: {bounds.center}");
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} extents: {bounds.extents}");
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} local: {avatarGameObject.transform.localPosition}");
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} center to bottom: {(root.InverseTransformPoint(bounds.center).y - extents.y)}");
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} offsetY: {offsetY}");
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} scale: {scale}");
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} yscale factor: {extents.y / max}");
            */

            avatarGameObject.transform.localScale = Vector3.one;
            avatarGameObject.transform.localPosition = Vector3.zero;
            avatarGameObject.transform.localRotation = Quaternion.Euler(_eulerAngles);

            var childBrains = avatarGameObject.transform.GetComponentsInChildren<IMonaBrainRunner>();
            for (var i = 0; i < childBrains.Length; i++)
                childBrains[i].CacheTransforms();

            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} localPosition: {avatarGameObject.transform.localPosition}");

            Vector3 position = avatar.GetPosition() + avatar.GetRotation() * offset;
            Quaternion rotation = avatar.GetRotation() * Quaternion.Euler(eulerAngles);


            if (_scaleToFit)
            {
                scale = Vector3.one * maxBoxScale;
                avatar.TeleportScale(scale);
                avatar.TeleportPosition(position + (-offsetY) * maxBoxScale + offset);
            }
            else
            {
                avatar.TeleportScale(scale);
                avatar.TeleportPosition(position + (-offsetY) * scale.y + offset);
            }

            avatar.SetSpawnTransforms(position, rotation, scale, _spawnAsChild, true);

        }

        private Bounds GetBounds(GameObject go)
        {
            Bounds bounds;
            Renderer childRender;
            bounds = GetRenderBounds(go);
            if (bounds.extents.x == 0)
            {
                bounds = new Bounds(go.transform.position, Vector3.zero);
                foreach (Transform child in go.transform)
                {
                    childRender = child.GetComponent<Renderer>();
                    if (childRender)
                    {
                        bounds.Encapsulate(childRender.bounds);
                    }
                    else
                    {
                        bounds.Encapsulate(GetBounds(child.gameObject));
                    }
                }
            }
            return bounds;
        }

        private Bounds GetRenderBounds(GameObject go)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer render = go.GetComponent<Renderer>();
            if (render != null)
            {
                return render.bounds;
            }
            return bounds;
        }

        private void HandleAfterEnabled(IMonaBody body)
        {
            body.OnAfterEnabled -= HandleAfterEnabled;
            //Debug.Log($"{nameof(HandleAfterEnabled)} ready ", body.Transform.gameObject);
            Complete(InstructionTileResult.Success, true);
        }

        private void SetSpawnerReferenceOnSpawned(IMonaBody spawned, IMonaBody previouslySpawnedBody)
        {
            spawned.Spawner = _brain.Body;
            spawned.PoolBodyPrevious = previouslySpawnedBody;

            var children = spawned.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null)
                    SetSpawnerReferenceOnSpawned(children[i], previouslySpawnedBody);
            }
        }

        private void SetNextBodyReferenceOnPrevious(IMonaBody targetBody, IMonaBody spawned)
        {
            targetBody.PoolBodyNext = spawned;

            var children = targetBody.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != null)
                    SetNextBodyReferenceOnPrevious(children[i], spawned);
            }
        }

        public override void Unload(bool destroy = false)
        {
            //if(_brain.LoggingEnabled) //
            //Debug.Log($"{nameof(Unload)} spawn asset instruction tile unload");
            base.Unload();
            
            for (var i = 0; i < _equipmentInstances.Count; i++)
            {
                var instance = _equipmentInstances[i];
                if (instance == null) continue;
                if (destroy)
                {
                    instance.OnBodyDisabled -= HandleBodyDisabled;
                    instance.OnAfterEnabled -= HandleAfterEnabled;
                    if (!instance.Destroyed)
                        instance.Destroy();
                }
                else if(!instance.Destroyed)
                    instance.SetActive(false);
            }

            if (destroy)
            {
                _shouldSpawn = true;
                _brain.SpawnedBodies.Clear();
                _equipmentInstances.Clear();
                _pool.Clear();
            }
        }
    }
}