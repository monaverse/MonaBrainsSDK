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
using VRM;
using UniHumanoid;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.Assets;
using Mona.SDK.Core.Utils;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Mona.SDK.Brains.Tiles.Actions.Character
{
    [Serializable]
    public class ChangeSpaceInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "ChangeSpace";
        public const string NAME = "Change Space";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ChangeSpaceInstructionTile);

        public bool IsAnimationTile => false;

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Self;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private bool _useUri = false;
        [BrainProperty(false)] public bool UseUrl { get => _useUri; set => _useUri = value; }

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyShow("UseUrl", false)]
        [BrainPropertyMonaAsset(typeof(IMonaAvatarAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField] private string _monaAssetName = null;
        [BrainPropertyValueName(nameof(MonaAsset), typeof(IMonaVariablesStringValue))] public string MonaAssetName { get => _monaAssetName; set => _monaAssetName = value; }

        [SerializeField] private string _assetUri = null;
        [BrainPropertyShow("UseUrl", true)]
        [BrainProperty(true)]
        public string AssetUrl { get => _assetUri; set => _assetUri = value; }

        [SerializeField] private string _assetUrlName = null;
        [BrainPropertyValueName(nameof(AssetUrl), typeof(IMonaVariablesStringValue))] public string AssetUrlName { get => _assetUrlName; set => _assetUrlName = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _eulerAngles = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        [SerializeField] private bool _includeAttached = true;
        [SerializeField] private string _includeAttachedName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnHitTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.AllSpawnedByMe)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

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
                    case MonaBrainBroadcastType.Children:
                        return false;
                    case MonaBrainBroadcastType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        private IMonaBrain _brain;
        private Transform _root;
        private IMonaAnimationController _monaAnimationController;
        private IMonaAvatarAssetItem _avatarAsset;
        private Animator _avatarInstance;
        private List<Transform> _wearableTransforms;

        private Action<MonaBodyAnimationControllerChangedEvent> OnAnimationControllerChanged;

        public ChangeSpaceInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            SetupAnimation();
        }

        private void HandleAnimationControllerChanged(MonaBodyAnimationControllerChangedEvent evt)
        {
            SetupAnimation();
        }

        private void SetupAnimation()
        {
            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    SetupOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    SetupAnimation(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    SetupOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    SetupAnimation(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    SetupOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    //if (ModifyAllAttached)
                    //    SetupOnWholeEntity(targetBody);
                    //else
                        SetupAnimation(targetBody);
                    break;
            }

        }

        private void SetupOnTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                //if (ModifyAllAttached)
                //    ModifyOnWholeEntity(tagBodies[i]);
                //else
                SetupAnimation(tagBodies[i]);
            }
        }

        private void SetupOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ChangeSpace(topBody);
            SetupAnimation(topBody);
        }

        private void SetupOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                ChangeSpace(children[i]);
                SetupAnimation(children[i]);
            }
        }

        private void SetupOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetupAnimation(spawned[i]);
                else
                    ChangeSpace(spawned[i]);
            }
        }

        private void SetupAnimation(IMonaBody body)
        {
            OnAnimationControllerChanged = HandleAnimationControllerChanged;
            MonaEventBus.Register<MonaBodyAnimationControllerChangedEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, body), OnAnimationControllerChanged);

            if (body.Transform.Find("Root") != null)
                _monaAnimationController = body.Transform.Find("Root").GetComponent<IMonaAnimationController>();
            else
            {
                var children = body.Children();
                for (var i = 0; i < children.Count; i++)
                {
                    var root = children[i].Transform.Find("Root");
                    if (root != null)
                    {
                        _monaAnimationController = root.GetComponent<IMonaAnimationController>();
                        if (_monaAnimationController != null) break;
                    }
                }
            }

            if (_monaAnimationController != null)
                _avatarInstance = _monaAnimationController.Animator;

        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    ModifyOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    ChangeSpace(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    ModifyOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    ChangeSpace(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    ModifyOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    //if (ModifyAllAttached)
                    //    ModifyOnWholeEntity(targetBody);
                    //else
                        ChangeSpace(targetBody);

                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private void ModifyOnTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                //if (ModifyAllAttached)
                //    ModifyOnWholeEntity(tagBodies[i]);
                //else
                    ChangeSpace(tagBodies[i]);
            }
        }

        private void ModifyOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ChangeSpace(topBody);
            ModifyOnChildren(topBody);
        }

        private void ModifyOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                ChangeSpace(children[i]);
                ModifyOnChildren(children[i]);
            }
        }

        private void ModifyOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(spawned[i]);
                else
                    ChangeSpace(spawned[i]);
            }
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
                case MonaBrainBroadcastType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
            }
            return null;
        }

        private InstructionTileResult ChangeSpace(IMonaBody body)
        {
            if (body != null)
            {
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                if (!string.IsNullOrEmpty(_assetUrlName))
                    _assetUri = _brain.Variables.GetString(_assetUrlName);

                if (!string.IsNullOrEmpty(_assetUri))
                {
                    _avatarAsset = (IMonaAvatarAssetItem)(new MonaAvatarAsset()
                    {
                        Url = _assetUri
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(_monaAssetName))
                        _monaAsset = _brain.Variables.GetString(_monaAssetName);

                    _avatarAsset = (IMonaAvatarAssetItem)_brain.GetMonaAsset(_monaAsset);
                }

                if (_avatarAsset.Value != null)
                {
                    var avatar = GameObject.Instantiate(_avatarAsset.Value);
                    var animator = avatar.GetComponent<Animator>();
                    if (animator == null)
                        animator = avatar.AddComponent<Animator>();
                    LoadAvatar(animator, body, avatar.gameObject);
                    return Complete(InstructionTileResult.Success);
                }
                else if (!string.IsNullOrEmpty(_avatarAsset.Url))
                {
                    LoadAvatarAtUrl(_avatarAsset.Url, body);
                    return Complete(InstructionTileResult.Running);
                }
            }
            return InstructionTileResult.Failure;
        }

        private async void LoadSpace(string url)
        {
            Debug.Log($"{nameof(LoadSpace)}.{nameof(LoadSpace)} - Loading Space...");

            
           // await UnloadLoadedScenesAsync();

            //_animatorProxy.ClearObjects();

            var scenesLoadedSuccessfully = await LoadScenes();

            //Core.SpaceLoaded = scenesLoadedSuccessfully;

            /*if (!scenesLoadedSuccessfully)
            {
                _sendOnSpaceLoadFailedUseCase.Send(loadSpaceRequest.SpaceId);
                return;
            }

            LoadSpawnPoint(loadSpaceRequest.SpawnType, loadSpaceRequest.SpawnCollider);

            _loadSessionUseCase.Handle();

            _eventBus.Fire(new LoadArtifactsRequestEvent());

            await _joinSessionBehavior.JoinOrCreateSession(
                spaceId: loadSpaceRequest.SpaceId,
                hangoutId: loadSpaceRequest.HangoutId,
                maxConcurrentUsers: loadSpaceRequest.MaxConcurrentUsers);

            Util.DisableComponents(FindObjectsOfType<PostProcessVolume>(), new string[] { "PostprocessVolume" });

            LoadPlatforms();
            LoadGlbFromQueryString();

            _sendOnSpaceLoadedUseCase.Handle(loadSpaceRequest.SpaceId);
            */
        }

        private async Task<bool> LoadScenes()
        {
            /*await Task.WhenAll(
                LoadScene(Core.TargetSpaceUrl, MonaConstants.SCENE_SPACE),
                LoadScene(Core.TargetPortalsUrl, MonaConstants.SCENE_PORTALS),
                LoadScene(Core.TargetArtifactsUrl, MonaConstants.SCENE_ARTIFACTS)
            );

            if (_sceneLoadedFlags[MonaConstants.SCENE_SPACE] && _sceneLoadedFlags[MonaConstants.SCENE_PORTALS] && _sceneLoadedFlags[MonaConstants.SCENE_ARTIFACTS])
            {
                Debug.Log($"{nameof(SpaceLoadPresenter)}.{nameof(LoadScenes)} - All Scenes Loaded!");
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(MonaConstants.SCENE_SPACE));
                return true;
            }
            Debug.Log($"{nameof(SpaceLoadPresenter)}.{nameof(LoadScenes)} - A scene failed to load - Space Scene: {_sceneLoadedFlags[MonaConstants.SCENE_SPACE]}, Artifact Scene: {_sceneLoadedFlags[MonaConstants.SCENE_PORTALS]}, Portal Scene: {_sceneLoadedFlags[MonaConstants.SCENE_ARTIFACTS]}");
            */
            return false;
        }

        private async Task LoadScene(string assetBundleUrl, string sceneName)
        {
            var bundle = await DownloadBundleAsync(assetBundleUrl, sceneName);
            if (bundle != null) await LoadAssetBundleAsync(bundle, sceneName);
        }

        private async Task<AssetBundle> DownloadBundleAsync(string assetBundleUrl, string sceneName)
        {
            //Debug.Log($"{nameof(SpaceLoadPresenter)}.{nameof(DownloadBundleAsync)} - Downloading Bundle ({sceneName}): {assetBundleUrl}");
            /*
#if !UNITY_WEBGL
            await UniTask.WaitUntil(() => Caching.ready);
#endif
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
            {
                await request.SendWebRequest();

                while (!request.isDone)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                    _sendOnSceneDownloadProgressUseCase.Send(sceneName, (int)(request.downloadProgress * 100));
                }

                _sendOnSceneDownloadProgressUseCase.Send(sceneName, (int)(request.downloadProgress * 100));

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerAssetBundle.GetContent(request);
                }
                else
                {
                    var ex = $"{nameof(SpaceLoadPresenter)}.{nameof(DownloadBundleAsync)} - Failed to download {sceneName} from {assetBundleUrl}, Error:{request.error}, Response Code:{request.responseCode}";
                    if (request.error != null) SentrySdk.CaptureException(new Exception(ex));
                    Debug.LogError(ex);
                }
            }*/
            return null;
        }

        private async Task LoadAssetBundleAsync(AssetBundle bundle, string sceneName)
        {
            /*Debug.Log($"{nameof(SpaceLoadPresenter)}.{nameof(LoadAssetBundleAsync)} - Loading Scene {sceneName} from Bundle");

            var task = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await new WaitUntil(() => task.isDone);

            _sceneLoadedFlags[sceneName] = true;

            bundle.Unload(false);*/
        }

        private GameObject _avatarLoader;
        private void LoadAvatarAtUrl(string url, IMonaBody body)
        {
            if (_avatarLoader == null)
                _avatarLoader = new GameObject("AvatarLoader");

            var loader = _avatarLoader.GetComponent<BrainsVrmLoader>();
            if (loader == null)
                loader = _avatarLoader.AddComponent<BrainsVrmLoader>();

            loader.Load(url, (avatar) =>
            {
                if (avatar != null)
                {
                    var animator = avatar.GetComponent<Animator>();
                    if (animator == null)
                        animator = avatar.AddComponent<Animator>();
                    LoadAvatar(animator, body, avatar);
                }
                Complete(InstructionTileResult.Success, true);
            });
        }

        private void LoadAvatar(Animator animator, IMonaBody body, GameObject avatarGameObject)
        {
            _avatarInstance = animator;
            var root = body.Transform.Find("Root");

            for (var i = 0; i < root.childCount; i++)
                GameObject.DestroyImmediate(root.GetChild(i).gameObject);

            avatarGameObject.transform.position = Vector3.zero;
            avatarGameObject.transform.rotation = Quaternion.identity;
            avatarGameObject.transform.localScale = Vector3.one;

            avatarGameObject.transform.SetParent(root);

            var parent = body;
            while (parent != null)
            {
                parent.CacheRenderers();
                parent = parent.Parent;
            }

            if (_monaAnimationController != null)
            {

                _monaAnimationController.SetAnimator(_avatarInstance);

                var parts = new List<IMonaBodyPart>(_avatarInstance.transform.GetComponentsInChildren<IMonaBodyPart>());
                var transforms = new List<Transform>(_avatarInstance.transform.GetComponentsInChildren<Transform>());

                var boneMappings = _avatarInstance.GetComponent<VRMHumanoidDescription>();
                if (boneMappings != null)
                {
                    var avatarTransforms = new List<Transform>(_avatarInstance.transform.GetComponentsInChildren<Transform>());

                    AvatarDescription description = boneMappings.GetDescription(out bool isCreated);
                    var avatar = description.ToHumanDescription(_avatarInstance.transform);

                    for (var i = 0; i < avatar.human.Length; i++)
                    {
                        var tag = avatar.human[i].humanName;
                        if (_brain.MonaTagSource.HasTag(tag))
                        {
                            var part = parts.Find(x => x.HasMonaTag(tag));
                            if (part == null)
                            {
                                var t = transforms.Find(x => x.name == avatar.human[i].boneName);
                                if (t != null)
                                {
                                    var newPart = t.AddComponent<MonaBodyPart>();
                                    newPart.AddTag(tag);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < (int)HumanBodyBones.LastBone; i++)
                    {
                        var tag = ((HumanBodyBones)i).ToString();
                        if (_brain.MonaTagSource.HasTag(tag))
                        {
                            var part = parts.Find(x => x.HasMonaTag(tag));
                            if (part == null)
                            {
                                var t = transforms.Find(x => x.name == tag);
                                if (t != null)
                                {
                                    var newPart = t.AddComponent<MonaBodyPart>();
                                    newPart.AddTag(tag);
                                }
                            }
                        }
                    }
                }

                while (body != null)
                {
                    MonaEventBus.Trigger<MonaBodyAnimationControllerChangeEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGE_EVENT, body), new MonaBodyAnimationControllerChangeEvent(_avatarInstance));
                    body = body.Parent;
                }
            }

            avatarGameObject.transform.localScale = Vector3.one;
            avatarGameObject.transform.localPosition = _offset;
            avatarGameObject.transform.localRotation = Quaternion.Euler(_eulerAngles);

            root.localScale = _scale;

            var bounds = GetBounds(_avatarInstance.gameObject);
            var extents = bounds.extents * 2f;
            var max = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
            var maxScale = Mathf.Max(Mathf.Max(_scale.x, _scale.y), _scale.z);
            var scale = maxScale / max;

            root.localScale = _scale * scale;

            Debug.Log($"{_avatarInstance} {_offset} scale {_scale} {avatarGameObject.transform.position} brain body {_brain.Body.Transform.position}");

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

        public override void Unload(bool destroy = false)
        {
            base.Unload();

            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            //if(_avatarInstance != null)
            //    GameObject.Destroy(_avatarInstance.transform.gameObject);
            _avatarInstance = null;
        }

    }
}