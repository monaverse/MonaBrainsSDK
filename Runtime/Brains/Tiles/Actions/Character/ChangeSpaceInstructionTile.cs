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
using Mona.SDK.Brains.ThirdParty.Redcode.Awaiting;
using Mona.SDK.Brains.Actions.Visuals.Enums;

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

        [SerializeField] private MonaChangeSpaceType _spaceType = MonaChangeSpaceType.ReplaceSpaces;
        [BrainPropertyEnum(true)] public MonaChangeSpaceType SpaceType { get => _spaceType; set => _spaceType = value; }

        [SerializeField] private string _assetUri = null;
        [BrainProperty(true)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.ReplaceSpaces)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.AddSpace)]
        public string AssetUrl { get => _assetUri; set => _assetUri = value; }

        [SerializeField] private string _assetUrlName = null;
        [BrainPropertyValueName(nameof(AssetUrl), typeof(IMonaVariablesStringValue))] public string AssetUrlName { get => _assetUrlName; set => _assetUrlName = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.ReplaceSpaces)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.AddSpace)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _eulerAngles = Vector3.zero;
        [BrainProperty(false)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.ReplaceSpaces)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.AddSpace)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.ReplaceSpaces)]
        [BrainPropertyShow(nameof(SpaceType), (int)MonaChangeSpaceType.AddSpace)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

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
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            return ChangeSpace(_brain.Body);
        }

        private InstructionTileResult ChangeSpace(IMonaBody body)
        {
            if (body != null)
            {
                if (_spaceType == MonaChangeSpaceType.RemoveSpaces)
                {
                    RemoveSpaces();
                    return Complete(InstructionTileResult.Running);
                }
                else
                {
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
            }
            return InstructionTileResult.Failure;
        }

        private async void RemoveSpaces()
        {
            await UnloadLoadedScenesAsync();
            Complete(InstructionTileResult.Success, true);
        }

        private async void LoadSpace(string url, IMonaBody body)
        {
            Debug.Log($"{nameof(LoadSpace)}.{nameof(LoadSpace)} - Loading Space...");

            if(_spaceType == MonaChangeSpaceType.ReplaceSpaces)
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

            Complete(InstructionTileResult.Success, true);
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
            while (!Caching.ready) await new WaitForSeconds(.1f);
#endif
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
            {
                request.SetRequestHeader("Access-Control-Allow-Origin", "*");

                await SendWebRequestAsync(request);

                while (!request.isDone)
                {
                    await new WaitForSeconds(.5f);
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

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;

            var task = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!task.isDone) await new WaitForSeconds(.1f);

            _sceneLoadedFlags[sceneName] = true;

            bundle.Unload(false);
        }

        private List<Scene> _loadedScenes = new List<Scene>();

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _loadedScenes.Add(scene);
            Debug.Log("OnSceneLoaded: " + scene.name);
            Debug.Log(mode);

            MonaEventBus.Trigger<MonaChangeSpaceEvent>(new EventHook(MonaCoreConstants.ON_CHANGE_SPACE_EVENT), new MonaChangeSpaceEvent(scene));
        }

        private async Task UnloadLoadedScenesAsync()
        {
            if (_sceneLoadedFlags[MonaBrainConstants.SCENE_SPACE])
            {
                for (var i = _loadedScenes.Count-1; i >= 0; i--)
                {
                    var scene = _loadedScenes[i];

                    var task = SceneManager.UnloadSceneAsync(scene);

                    while (!task.isDone) await new WaitForSeconds(.1f);

                    _loadedScenes.RemoveAt(i);
                }

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