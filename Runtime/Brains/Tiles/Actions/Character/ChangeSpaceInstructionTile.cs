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
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.Assets;
using Mona.SDK.Core.Utils;
using System.Threading.Tasks;
using System.IO.Compression;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
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

        [SerializeField] private string _assetUri = null;
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

            _sceneLoadedFlags = new Dictionary<string, bool>();
            _sceneLoadedFlags[MonaBrainConstants.SCENE_SPACE] = false;

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
                                
                _avatarAsset = (IMonaAvatarAssetItem)(new MonaAvatarAsset()
                {
                    Url = _assetUri
                });
                
                if (!string.IsNullOrEmpty(_avatarAsset.Url))
                {
                    LoadSpace(_avatarAsset.Url, body);
                    return Complete(InstructionTileResult.Running);
                }
            }
            return InstructionTileResult.Failure;
        }

        private async void LoadSpace(string url, IMonaBody body)
        {
            Debug.Log($"{nameof(LoadSpace)}.{nameof(LoadSpace)} - Loading Space...");

            await UnloadLoadedScenesAsync();

            var scenesLoadedSuccessfully = await LoadScenes(url);

            var scene = GameObject.Find(MonaBrainConstants.SCENE_SPACE);


            if(scene != null)
            {
                scene.transform.localPosition = _offset;
                scene.transform.localRotation = Quaternion.Euler(_eulerAngles);
                Debug.Log($"{nameof(LoadSpace)} loaded {url}", scene);
            }
            else
            {
                Debug.Log($"{nameof(LoadSpace)} could not load scene {url}");
            }
        }

        private Dictionary<string, bool> _sceneLoadedFlags = new Dictionary<string, bool>();
        private async Task<bool> LoadScenes(string spaceUrl)
        {
            await LoadScene(spaceUrl, MonaBrainConstants.SCENE_SPACE);

            if (_sceneLoadedFlags[MonaBrainConstants.SCENE_SPACE])
            {
                Debug.Log($"{nameof(LoadScenes)} - All Scenes Loaded!");
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(MonaBrainConstants.SCENE_SPACE));



                return true;
            }

            Debug.Log($"{nameof(LoadScenes)} - A scene failed to load - Space Scene: {_sceneLoadedFlags[MonaBrainConstants.SCENE_SPACE]}");//, Artifact Scene: {_sceneLoadedFlags[MonaConstants.SCENE_PORTALS]}, Portal Scene: {_sceneLoadedFlags[MonaConstants.SCENE_ARTIFACTS]}");
            return false;
        }

        private async Task LoadScene(string assetBundleUrl, string sceneName)
        {
            var bundle = await DownloadBundleAsync(assetBundleUrl, sceneName);
            if (bundle != null) await LoadAssetBundleAsync(bundle, sceneName);
        }


        public async Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest unityWebRequest)
        {
            var isComplete = false;

            var coroutine = SendWebRequestCoroutine(unityWebRequest);
            RunCoroutine(coroutine, () => isComplete = true);

            while (!isComplete && !unityWebRequest.isDone)
                await Task.Yield();

            return unityWebRequest;
        }

        public IEnumerator SendWebRequestCoroutine(UnityWebRequest unityWebRequest)
        {
            yield return unityWebRequest.SendWebRequest();
        }

        private CoroutineRunnerBehaviour _coroutineRunnerBehaviour;

        public Coroutine RunCoroutine(IEnumerator coroutine, Action onComplete)
        {
            if (_coroutineRunnerBehaviour != null)
                return _coroutineRunnerBehaviour.Run(coroutine, onComplete);

            var coroutineRunnerGameObject = new GameObject("~CoroutineRunner+" + Guid.NewGuid());
            var coroutineRunnerBehaviour = coroutineRunnerGameObject.GetComponent<CoroutineRunnerBehaviour>();

            if (coroutineRunnerBehaviour == null)
                coroutineRunnerBehaviour = coroutineRunnerGameObject.AddComponent<CoroutineRunnerBehaviour>();

            _coroutineRunnerBehaviour = coroutineRunnerBehaviour;

            GameObject.DontDestroyOnLoad(_coroutineRunnerBehaviour);

            _coroutineRunnerBehaviour.gameObject.hideFlags = HideFlags.HideInHierarchy;

            return _coroutineRunnerBehaviour.Run(coroutine, onComplete);
        }

        private sealed class CoroutineRunnerBehaviour : MonoBehaviour
        {
            private IEnumerator Coroutine(IEnumerator coroutine, Action onComplete)
            {
                yield return StartCoroutine(coroutine);
                onComplete?.Invoke();
            }

            public Coroutine Run(IEnumerator coroutine, Action onComplete)
            {
                return StartCoroutine(Coroutine(coroutine, onComplete));
            }
        }

        private async Task<AssetBundle> DownloadBundleAsync(string assetBundleUrl, string sceneName)
        {
            Debug.Log($"{nameof(DownloadBundleAsync)} - Downloading Bundle ({sceneName}): {assetBundleUrl}");

#if !UNITY_WEBGL
            while (!Caching.ready) await Task.Delay(100);
#endif
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
            {
                await SendWebRequestAsync(request);

                while (!request.isDone)
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.5f));
                    Debug.Log($"downloading... {(int)(request.downloadProgress * 100)}");
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerAssetBundle.GetContent(request);
                }
                else
                {
                    var ex = $"{nameof(DownloadBundleAsync)} - Failed to download {sceneName} from {assetBundleUrl}, Error:{request.error}, Response Code:{request.responseCode}";
                    Debug.LogError(ex);
                }
            }
            return null;
        }

        private async Task LoadAssetBundleAsync(AssetBundle bundle, string sceneName)
        {
            Debug.Log($"{nameof(LoadAssetBundleAsync)} - Loading Scene {sceneName} from Bundle");

            var task = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!task.isDone) await Task.Delay(100);

            _sceneLoadedFlags[sceneName] = true;

            bundle.Unload(false);
        }

        private async Task UnloadLoadedScenesAsync()
        {
            if (_sceneLoadedFlags[MonaBrainConstants.SCENE_SPACE])
            {
                var task = SceneManager.UnloadSceneAsync(MonaBrainConstants.SCENE_SPACE);

                while (!task.isDone) await Task.Delay(100);

                _sceneLoadedFlags[MonaBrainConstants.SCENE_SPACE] = false;
            }
        }

        public override void Unload(bool destroy = false)
        {
            base.Unload();

            UnloadLoadedScenesAsync();

            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            //if(_avatarInstance != null)
            //    GameObject.Destroy(_avatarInstance.transform.gameObject);
            _avatarInstance = null;
        }

    }
}