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
using Unity.Profiling;
using System.Threading.Tasks;
using Mona.SDK.Brains.ThirdParty.Redcode.Awaiting;

namespace Mona.SDK.Brains.Tiles.Actions.Character
{
    [Serializable]
    public class ChangeAvatarInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "ChangeAvatar";
        public const string NAME = "Change Avatar Asset";
        public const string CATEGORY = "Character";
        public override Type TileType => typeof(ChangeAvatarInstructionTile);

        public bool IsAnimationTile => _target == MonaBrainBroadcastType.Self || _target == MonaBrainBroadcastType.ThisBodyOnly;

        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(ChangeAvatarInstructionTile)}.{nameof(Do)}");
        //static readonly ProfilerMarker _profilerPreload = new ProfilerMarker($"MonaBrains.{nameof(ChangeAvatarInstructionTile)}.{nameof(Preload)}");


        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Self;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private bool _useUri = false;
        [BrainProperty(false)] public bool UseUrl { get => _useUri; set => _useUri = value; }

        [SerializeField] private bool _importAnimation = false;
        [BrainProperty(false)] public bool ImportAnimation { get => _importAnimation; set => _importAnimation = value; }

        [SerializeField] private bool _reuseController = true;
        [BrainProperty(false)] public bool ReuseAnimController { get => _reuseController; set => _reuseController = value; }

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

        [SerializeField]
        private int _poolSize = 1;
        [BrainProperty(false)]
        public int PoolSize { get => _poolSize; set => _poolSize = value; }

        [SerializeField] private bool _scaleToFit = true;
        [BrainProperty(false)] public bool ScaleToFit { get => _scaleToFit; set => _scaleToFit = value; }

        [SerializeField] private bool _bottomPivot = true;
        [BrainProperty(false)] public bool BottomPivot { get => _bottomPivot; set => _bottomPivot = value; }

        [SerializeField] private bool _importLights;
        [BrainProperty(false)] public bool ImportLights { get => _importLights; set => _importLights = value; }

        [SerializeField] private bool _removeAllColliders;
        [BrainProperty(false)] public bool RemoveAllColliders { get => _removeAllColliders; set => _removeAllColliders = value; }

        [SerializeField] private bool _includeAttached = true;
        [SerializeField] private string _includeAttachedName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnSelectTarget)]
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

        public ChangeAvatarInstructionTile() { }

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
            ///_profilerPreload.Begin();

            _brain = brainInstance;

            if (_avatarLoader == null)
                _avatarLoader = new GameObject("AvatarLoader");

            _urlLoader = _avatarLoader.GetComponent<BrainsGlbLoader>();
            if (_urlLoader == null)
                _urlLoader = _avatarLoader.AddComponent<BrainsGlbLoader>();

            SetupAnimation();

            //_profilerPreload.End();
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

            ChangeAvatar(topBody);
            SetupAnimation(topBody);
        }

        private void SetupOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                ChangeAvatar(children[i]);
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
                    ChangeAvatar(spawned[i]);
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

        private bool _shouldWait;
        private string _loadedUrl;
        private string _loadedAsset;

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_assetUrlName))
                _assetUri = _brain.Variables.GetString(_assetUrlName);

            if (_useUri)
            {
                if (string.IsNullOrEmpty(_assetUri))
                    return InstructionTileResult.Success;

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

            if (!string.IsNullOrEmpty(_assetUri))
            {
                if (_loadedUrl == _assetUri) 
                    return InstructionTileResult.Success;
            }
            else if (_loadedAsset == _monaAsset)
            {
                return InstructionTileResult.Success;
            }

            _shouldWait = true;
            //_profilerDo.Begin();
            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    ModifyOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    ChangeAvatar(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    ModifyOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    ChangeAvatar(_brain.Body);
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
                        ChangeAvatar(targetBody);

                    break;
            }

            //_profilerDo.End();
            Debug.Log($"{nameof(ChangeAvatarInstructionTile)} Running {_shouldWait}");
            return Complete(_shouldWait ? InstructionTileResult.Running : InstructionTileResult.Success);
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
                    ChangeAvatar(tagBodies[i]);
            }
        }

        private void ModifyOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ChangeAvatar(topBody);
            ModifyOnChildren(topBody);
        }

        private void ModifyOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                ChangeAvatar(children[i]);
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
                    ChangeAvatar(spawned[i]);
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
                case MonaBrainBroadcastType.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
            }
            return null;
        }

        private async void ChangeAvatar(IMonaBody body)
        {
            if (body != null)
            {
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                if (_avatarAsset.Value != null)
                {
                    var avatar = GameObject.Instantiate(_avatarAsset.Value);

                    if(_removeAllColliders)
                    {
                        var colliders = avatar.GetComponentsInChildren<Collider>(true);
                        for(var i = 0;i < colliders.Length; i++)
                            GameObject.DestroyImmediate(colliders[i]);
                    }

                    var avatarBody = avatar.GetComponent<IMonaBody>();

                    var root = body.Transform.Find("Root");
                    root.localScale = Vector3.one;
                    avatar.transform.SetParent(root);
                    avatar.transform.localPosition = Vector3.zero;

                    var frame = Time.frameCount;
                    if (avatarBody != null)
                    {
                        while (!avatarBody.Instantiated)
                        {
                            Debug.Log($"waiting to instantiate {avatarBody.Transform.name}");
                            await Task.Yield();
                        }
                    }
                    
                    if(Time.frameCount == frame)
                        _shouldWait = false;

                    if (!_importLights)
                    {
                        var lights = avatar.transform.GetComponentsInChildren<Light>(true);
                        for (var i = 0; i < lights.Length; i++)
                            lights[i].gameObject.SetActive(false);
                    }


                    var animator = avatar.GetComponent<Animator>();
                    
                    ParsedHumanoid parsed = new ParsedHumanoid();
                    if (animator == null || animator.avatar == null)
                        parsed = ParseHumanoid(avatar);

                    if ((animator == null || animator.avatar == null) && parsed.Avatar == null)
                    {
                        var pivot = new GameObject("Pivot-Brains");
                        avatar.transform.SetParent(pivot.transform);
                        avatar.transform.localScale = Vector3.one;
                        avatar.transform.localPosition = Vector3.zero;
                        avatar = pivot;

                        var bounds = GetBounds(avatar);
                        if (_bottomPivot)
                        {
                            var offsetY = Vector3.up * (bounds.center.y - bounds.extents.y);
                            avatar.transform.localPosition = offsetY;
                        }
                        else
                            avatar.transform.localPosition = Vector3.zero;
                    }

                    if (animator == null)
                        animator = avatar.AddComponent<Animator>();

                    if (parsed.Avatar != null)
                        animator.avatar = parsed.Avatar;

                    LoadAvatar(animator, body, avatar);

                    _loadedAsset = _avatarAsset.PrefabId;
                    _loadedUrl = null;

                    if(_shouldWait)
                        Complete(InstructionTileResult.Success, true);

                }
                else if (!string.IsNullOrEmpty(_avatarAsset.Url))
                {
                    LoadAvatarAtUrl(_avatarAsset.Url, body);
                }
            }

        }

        private GameObject _avatarLoader;
        private BrainsGlbLoader _urlLoader;
        private void LoadAvatarAtUrl(string url, IMonaBody body)
        {
            if (url == body.SkinId)
            {
                var avatarBody = body.Skin.GetComponent<IMonaBody>();
                _brain.Variables.Set(MonaBrainConstants.RESULT_LAST_SKIN, avatarBody);
                _brain.Variables.Set(MonaBrainConstants.RESULT_LAST_SPAWNED, avatarBody);

                _shouldWait = false;
                return;
            }

            var frame = Time.frameCount;
            _urlLoader.Load(url, _importAnimation, async (avatar) =>
            {
                if (avatar != null)
                {
                    if (_removeAllColliders)
                    {
                        var colliders = avatar.GetComponentsInChildren<Collider>(true);
                        for (var i = 0; i < colliders.Length; i++)
                            GameObject.DestroyImmediate(colliders[i]);
                    }

                    var bodies = avatar.GetComponentsInChildren<IMonaBody>();
                    for(var i = 0;i < bodies.Length; i++)
                    {
                        if (bodies[i].GetActive() && !bodies[i].Instantiated)
                        {
                            Debug.Log($"waiting to instantiate {bodies[i].Transform.name}");
                            await Task.Yield();
                        }
                    }

                    Debug.Log($"READY TO GO {avatar.gameObject.name}");
                    avatar.SetActive(true);

                    if (!_importLights)
                    {
                        var lights = avatar.transform.GetComponentsInChildren<Light>(true);
                        for (var i = 0; i < lights.Length; i++)
                            lights[i].gameObject.SetActive(false);
                    }

                    var rb = avatar.GetComponentInChildren<IMonaBody>(true);
                    if (rb != null)
                        rb.RemoveRigidbody();

                    var animator = avatar.GetComponent<Animator>();

                    ParsedHumanoid parsed = new ParsedHumanoid();
                    if (animator == null || animator.avatar == null)
                        parsed = ParseHumanoid(avatar);

                    if ((animator == null || animator.avatar == null) && parsed.Avatar == null && avatar.name != "Pivot-Brains")
                    {
                        var pivot = new GameObject("Pivot-Brains");
                        avatar.transform.SetParent(pivot.transform);
                        avatar.transform.localScale = Vector3.one;
                        avatar.transform.localPosition = Vector3.zero;
                        avatar.transform.localRotation = Quaternion.identity;
                        avatar = pivot;

                        if (_bottomPivot)
                        {
                            var bounds = GetBounds(avatar);
                            var offsetY = Vector3.up * (bounds.center.y - bounds.extents.y);
                            avatar.transform.localPosition = offsetY;
                        }
                        else
                            avatar.transform.localPosition = Vector3.zero;
                    }

                    if (animator == null)
                        animator = avatar.AddComponent<Animator>();

                    if (parsed.Avatar != null)
                        animator.avatar = parsed.Avatar;

                    try
                    {
                        LoadAvatar(animator, body, avatar, parsed.Skeleton);
                        _loadedAsset = null;
                        _loadedUrl = url;
                        body.SkinId = url;
                        body.Skin = avatar;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{nameof(LoadAvatarAtUrl)} {e.Message} {e.StackTrace}");
                    }

                }

                if (Time.frameCount == frame)
                    _shouldWait = false;
                else
                    Complete(InstructionTileResult.Success, true);

                Debug.Log($"{nameof(ChangeAvatarInstructionTile)} Success");
            }, _poolSize);
        }

        //thank you gpt!
        private static Dictionary<HumanBodyBones, string[]> _boneSynonyms = new Dictionary<HumanBodyBones, string[]> {
            { HumanBodyBones.Hips, new string[] { "Hips", "mixamorig:Hips" } },
            { HumanBodyBones.LeftUpperLeg, new string[] { "LeftUpperLeg", "UpperLeg.L", "Upper_Leg.L", "L_UpperLeg", "mixamorig:LeftUpLeg" } },
            { HumanBodyBones.RightUpperLeg, new string[] { "RightUpperLeg", "UpperLeg.R", "Upper_Leg.R", "R_UpperLeg", "mixamorig:RightUpLeg" } },
            { HumanBodyBones.LeftLowerLeg, new string[] { "LeftLowerLeg", "LowerLeg.L", "Lower_Leg.L", "L_LowerLeg", "mixamorig:LeftLeg" } },
            { HumanBodyBones.RightLowerLeg, new string[] { "RightLowerLeg", "LowerLeg.R", "Lower_Leg.R", "R_LowerLeg", "mixamorig:RightLeg" } },
            { HumanBodyBones.LeftFoot, new string[] { "LeftFoot", "Foot.L", "L_Foot", "mixamorig:LeftFoot" } },
            { HumanBodyBones.RightFoot, new string[] { "RightFoot", "Foot.R", "R_Foot", "mixamorig:RightFoot" } },
            { HumanBodyBones.Spine, new string[] { "Spine", "mixamorig:Spine" } },
            { HumanBodyBones.Chest, new string[] { "Chest", "mixamorig:Spine1" } },
            { HumanBodyBones.UpperChest, new string[] { "UpperChest", "mixamorig:Spine2" } },
            { HumanBodyBones.Neck, new string[] { "Neck", "mixamorig:Neck" } },
            { HumanBodyBones.Head, new string[] { "Head", "mixamorig:Head" } },
            { HumanBodyBones.LeftShoulder, new string[] { "LeftShoulder", "Shoulder.L", "ShoulderLeft", "Shoulder_Left", "L_Shoulder", "mixamorig:LeftShoulder" } },
            { HumanBodyBones.RightShoulder, new string[] { "RightShoulder", "Shoulder.R", "ShoulderRight", "Shoulder_Right", "R_Shoulder", "mixamorig:RightShoulder" } },
            { HumanBodyBones.LeftUpperArm, new string[] { "LeftUpperArm", "UpperArm.L", "Upper_Arm.L", "UpperArmLeft", "UpperArm_Left", "Upper_Arm_Left", "L_UpperArm", "mixamorig:LeftArm" } },
            { HumanBodyBones.RightUpperArm, new string[] { "RightUpperArm", "UpperArm.R", "Upper_Arm.R", "UpperArmRight", "UpperArm_Right", "Upper_Arm_Right", "R_UpperArm", "mixamorig:RightArm" } },
            { HumanBodyBones.LeftLowerArm, new string[] { "LeftLowerArm", "LowerArm.L", "Lower_Arm.L", "LowerArmLeft", "LowerArm_Left", "Lower_Arm_Left", "L_LowerArm", "mixamorig:LeftForeArm" } },
            { HumanBodyBones.RightLowerArm, new string[] { "RightLowerArm", "LowerArm.R", "Lower_Arm.R", "LowerArmRight", "LowerArm_Right", "Lower_Arm_Right", "R_LowerArm", "mixamorig:RightForeArm" } },
            { HumanBodyBones.LeftHand, new string[] { "LeftHand", "Hand.L", "Left_Hand", "Hand_Left", "L_Hand", "mixamorig:LeftHand" } },
            { HumanBodyBones.RightHand, new string[] { "RightHand", "Hand.R", "Right_Hand", "Hand_Right", "R_Hand", "mixamorig:RightHand" } },
            { HumanBodyBones.LeftToes, new string[] { "LeftToes", "Toes.L", "Left_Toes", "Toes_Left", "L_Toes", "mixamorig:LeftToeBase" } },
            { HumanBodyBones.RightToes, new string[] { "RightToes", "Toes.R", "Right_Toes", "Toes_Right", "R_Toes", "mixamorig:RightToeBase" } },
            { HumanBodyBones.LeftEye, new string[] { "LeftEye", "Eye.L", "Left_Eye", "Toes_Eye", "L_Eye" } },
            { HumanBodyBones.RightEye, new string[] { "RightEye", "Eye.R", "Right_Eye", "Toes_Eye", "R_Eye" } },
            { HumanBodyBones.Jaw, new string[] { "Jaw" } },
            { HumanBodyBones.LeftThumbProximal, new string[] { "LeftThumbProximal", "ThumbProximal.L", "ThumbProximal.Left", "Thumb_Proximal_Left", "L_ThumbProximal" } },
            { HumanBodyBones.RightThumbProximal, new string[] { "RightThumbProximal", "ThumbProximal.R", "ThumbProximal.Right", "Thumb_Proximal_Right", "R_ThumbProximal" } },
            { HumanBodyBones.LeftThumbIntermediate, new string[] { "LeftThumbIntermediate", "ThumbIntermediate.L", "ThumbIntermediate.Left", "Thumb_Intermediate_Left", "L_ThumbIntermediate" } },
            { HumanBodyBones.RightThumbIntermediate, new string[] { "RightThumbIntermediate", "ThumbIntermediate.R", "ThumbIntermediate.Right", "Thumb_Intermediate_Right", "R_ThumbIntermediate" } },
            { HumanBodyBones.LeftThumbDistal, new string[] { "LeftThumbDistal", "ThumbDistal.L", "ThumbDistal.Left", "Thumb_Distal_Left", "L_ThumbDistal" } },
            { HumanBodyBones.RightThumbDistal, new string[] { "RightThumbDistal", "ThumbDistal.R", "ThumbDistal.Right", "Thumb_Distal_Right", "R_ThumbDistal" } },
            { HumanBodyBones.LeftIndexProximal, new string[] { "LeftIndexProximal", "IndexProximal.L", "IndexProximal.Left", "Index_Proximal_Left", "L_IndexProximal", "mixamorig:LeftHandIndex2" } },
            { HumanBodyBones.RightIndexProximal, new string[] { "RightIndexProximal", "IndexProximal.R", "IndexProximal.Right", "Index_Proximal_Right", "R_IndexProximal", "mixamorig:RightHandIndex2" } },
            { HumanBodyBones.LeftIndexIntermediate, new string[] { "LeftIndexIntermediate", "IndexIntermediate.L", "IndexIntermediate.Left", "Index_Intermediate_Left", "L_IndexIntermediate", "mixamorig:LeftHandIndex1" } },
            { HumanBodyBones.RightIndexIntermediate, new string[] { "RightIndexIntermediate", "IndexIntermediate.R", "IndexIntermediate.Right", "Index_Intermediate_Right", "R_IndexIntermediate", "mixamorig:RightHandIndex1" } },
            { HumanBodyBones.LeftIndexDistal, new string[] { "LeftIndexDistal", "IndexDistal.L", "IndexDistal.Left", "Index_Distal_Left", "L_IndexDistal", "mixamorig:LeftHandIndex3" } },
            { HumanBodyBones.RightIndexDistal, new string[] { "RightIndexDistal", "IndexDistal.R", "IndexDistal.Right", "Index_Distal_Right", "R_IndexDistal", "mixamorig:RightHandIndex3" } },
            { HumanBodyBones.LeftMiddleProximal, new string[] { "LeftMiddleProximal", "MiddleProximal.L", "MiddleProximal.Left", "Middle_Proximal_Left", "L_MiddleProximal" } },
            { HumanBodyBones.RightMiddleProximal, new string[] { "RightMiddleProximal", "MiddleProximal.R", "MiddleProximal.Right", "Middle_Proximal_Right", "R_MiddleProximal" } },
            { HumanBodyBones.LeftMiddleIntermediate, new string[] { "LeftMiddleIntermediate", "MiddleIntermediate.L", "MiddleIntermediate.Left", "Middle_Intermediate_Left", "L_MiddleIntermediate" } },
            { HumanBodyBones.RightMiddleIntermediate, new string[] { "RightMiddleIntermediate", "MiddleIntermediate.R", "MiddleIntermediate.Right", "Middle_Intermediate_Right", "R_MiddleIntermediate" } },
            { HumanBodyBones.LeftMiddleDistal, new string[] { "LeftMiddleDistal", "MiddleDistal.L", "MiddleDistal.Left", "Middle_Distal_Left", "L_MiddleDistal" } },
            { HumanBodyBones.RightMiddleDistal, new string[] { "RightMiddleDistal", "MiddleDistal.R", "MiddleDistal.Right", "Middle_Distal_Right", "R_MiddleDistal" } },
            { HumanBodyBones.LeftRingProximal, new string[] { "LeftRingProximal", "RingProximal.L", "RingProximal.Left", "Ring_Proximal_Left", "L_RingProximal" } },
            { HumanBodyBones.RightRingProximal, new string[] { "RightRingProximal", "RingProximal.R", "RingProximal.Right", "Ring_Proximal_Right", "R_RingProximal" } },
            { HumanBodyBones.LeftRingIntermediate, new string[] { "LeftRingIntermediate", "RingIntermediate.L", "RingIntermediate.Left", "Ring_Intermediate_Left", "L_RingIntermediate" } },
            { HumanBodyBones.RightRingIntermediate, new string[] { "RightRingIntermediate", "RingIntermediate.R", "RingIntermediate.Right", "Ring_Intermediate_Right", "R_RingIntermediate" } },
            { HumanBodyBones.LeftRingDistal, new string[] { "LeftRingDistal", "RingDistal.L", "RingDistal.Left", "Ring_Distal_Left", "L_RingDistal" } },
            { HumanBodyBones.RightRingDistal, new string[] { "RightRingDistal", "RingDistal.R", "RingDistal.Right", "Ring_Distal_Right", "R_RingDistal" } },
            { HumanBodyBones.LeftLittleProximal, new string[] { "LeftLittleProximal", "LittleProximal.L", "LittleProximal.Left", "Little_Proximal_Left", "L_LittleProximal" } },
            { HumanBodyBones.RightLittleProximal, new string[] { "RightLittleProximal", "LittleProximal.R", "LittleProximal.Right", "Little_Proximal_Right", "R_LittleProximal" } },
            { HumanBodyBones.LeftLittleIntermediate, new string[] { "LeftLittleIntermediate", "LittleIntermediate.L", "LittleIntermediate.Left", "Little_Intermediate_Left", "L_LittleIntermediate" } },
            { HumanBodyBones.RightLittleIntermediate, new string[] { "RightLittleIntermediate", "LittleIntermediate.R", "LittleIntermediate.Right", "Little_Intermediate_Right", "R_LittleIntermediate" } },
            { HumanBodyBones.LeftLittleDistal, new string[] { "LeftLittleDistal", "LittleDistal.L", "LittleDistal.Left", "Little_Distal_Left", "L_LittleDistal" } },
            { HumanBodyBones.RightLittleDistal, new string[] { "RightLittleDistal", "LittleDistal.R", "LittleDistal.Right", "Little_Distal_Right", "R_LittleDistal" } },
            { HumanBodyBones.LastBone, new string[] { "LastBone" } }
        };

        public struct ParsedHumanoid
        {
            public Dictionary<HumanBodyBones, Transform> Skeleton;
            public Avatar Avatar;
        }

        public static ParsedHumanoid ParseHumanoid(GameObject avatar)
        {

            var transforms = new List<Transform>(avatar.GetComponentsInChildren<Transform>());
            var bones = Enum.GetValues(typeof(HumanBodyBones));
            var skeleton = new Dictionary<HumanBodyBones, Transform>();

            try
            {
                foreach (int bone in bones)
                {
                    var syns = _boneSynonyms[(HumanBodyBones)bone];
                    var found = false;
                    for (var i = 0; i < syns.Length; i++)
                    {
                        var syn = syns[i];
                        var t = transforms.Find(x => x.name.Equals(syn, StringComparison.OrdinalIgnoreCase));
                        if (t != null)
                        {
                            skeleton[(HumanBodyBones)bone] = t;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        var t = transforms.Find(x => x.name.ContainsInsensitive(syns[0]));
                        if (t != null)
                        {
                            skeleton[(HumanBodyBones)bone] = t;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(ParseHumanoid)} {e.Message}");
                return default;
            }

            Avatar avatarInst = null;
            if (skeleton.ContainsKey(HumanBodyBones.Hips) &&
                (skeleton.ContainsKey(HumanBodyBones.LeftUpperLeg) ||
                skeleton.ContainsKey(HumanBodyBones.RightUpperLeg) ||
                skeleton.ContainsKey(HumanBodyBones.RightUpperArm) ||
                skeleton.ContainsKey(HumanBodyBones.LeftUpperArm)) &&
                skeleton.ContainsKey(HumanBodyBones.Head) &&
                skeleton.ContainsKey(HumanBodyBones.Spine))

            {

                var description = AvatarDescription.Create(skeleton).ToHumanDescription(avatar.transform);
                description.upperLegTwist = 1;
                avatarInst = AvatarBuilder.BuildHumanAvatar(avatar, description);
            }

            return new ParsedHumanoid() { Avatar = avatarInst, Skeleton = skeleton };

        }

        private void LoadAvatar(Animator animator, IMonaBody body, GameObject avatarGameObject, Dictionary<HumanBodyBones, Transform> skeleton = null)
        {
            _avatarInstance = animator;

            var root = body.Transform.Find("Root");

            if (!string.IsNullOrEmpty(body.SkinId))
            {
                Debug.Log($"{nameof(ChangeAvatarInstructionTile)} {nameof(LoadAvatar)} skin id was loaded, return it to pool {body.SkinId}");
                _urlLoader.ReturnToPool(body.SkinId, body.Skin);
                body.SkinId = null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                if (body.Skin != root.GetChild(i).gameObject)
                {
                    Debug.Log($"{nameof(ChangeAvatarInstructionTile)} {nameof(LoadAvatar)} destroy previous skin {body.SkinId} other: {root.GetChild(i).gameObject}");
                    GameObject.DestroyImmediate(root.GetChild(i).gameObject);
                }
            }

            root.localScale = Vector3.one;
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;

            avatarGameObject.transform.SetParent(root);

            avatarGameObject.transform.position = Vector3.zero;
            avatarGameObject.transform.rotation = Quaternion.identity;
            avatarGameObject.transform.localScale = Vector3.one;

            var parent = body;
            while (parent != null)
            {
                parent.CacheRenderers();
                parent = parent.Parent;
            }

            if (_monaAnimationController != null)
            {

                _monaAnimationController.ReuseController = _reuseController;
                _monaAnimationController.SetAnimator(_avatarInstance);

                var parts = new List<IMonaBodyPart>(_avatarInstance.transform.GetComponentsInChildren<IMonaBodyPart>());
                var transforms = new List<Transform>(_avatarInstance.transform.GetComponentsInChildren<Transform>());

                var boneMappings = _avatarInstance.GetComponent<VRMHumanoidDescription>();
                if (boneMappings != null)
                {
                    var avatarTransforms = new List<Transform>(_avatarInstance.transform.GetComponentsInChildren<Transform>());

                    var avatarPart = _avatarInstance.transform.GetComponent<IMonaBody>();
                    if (avatarPart == null)
                    {
                        avatarPart = _avatarInstance.transform.AddComponent<MonaBody>();
                        avatarPart.SyncType = MonaBodyNetworkSyncType.NotNetworked;
                        avatarPart.AddTag("Avatar");
                    }

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
                                    newPart.SyncType = MonaBodyNetworkSyncType.NotNetworked;
                                    newPart.AddTag(tag);
                                }
                            }
                        }
                    }
                }
                else if (skeleton != null)
                {
                    foreach (var pair in skeleton)
                    {
                        var tag = ((HumanBodyBones)pair.Key).ToString();
                        if (_brain.MonaTagSource.HasTag(tag))
                        {
                            var part = parts.Find(x => x.HasMonaTag(tag));
                            if (part == null)
                            {
                                var t = pair.Value;
                                var newPart = t.AddComponent<MonaBodyPart>();
                                newPart.SyncType = MonaBodyNetworkSyncType.NotNetworked;
                                newPart.AddTag(tag);
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
                                var t = transforms.Find(x => x.name.StartsWith(tag, StringComparison.OrdinalIgnoreCase));
                                if (t != null)
                                {
                                    var newPart = t.AddComponent<MonaBodyPart>();
                                    newPart.SyncType = MonaBodyNetworkSyncType.NotNetworked;
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

            var avatarBody = avatarGameObject.GetComponentInChildren<IMonaBody>();
            _brain.Variables.Set(MonaBrainConstants.RESULT_LAST_SKIN, avatarBody);
            _brain.Variables.Set(MonaBrainConstants.RESULT_LAST_SPAWNED, avatarBody);

            RecalculateSize(avatarGameObject, root, animator.avatar != null);
                                       
            Debug.Log($"{_avatarInstance} {_offset} scale {_scale} {avatarGameObject.transform.position} brain body {_brain.Body.Transform.position}");

            var playerId = _brain.Player.GetPlayerIdByBody(_brain.Body);
            if(playerId > -1)
                MonaEventBus.Trigger<MonaPlayerChangeAvatarEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_CHANGE_AVATAR_EVENT), new MonaPlayerChangeAvatarEvent(playerId, _avatarInstance));
            MonaEventBus.Trigger<MonaChangeAvatarEvent>(new EventHook(MonaCoreConstants.ON_CHANGE_AVATAR_EVENT), new MonaChangeAvatarEvent(_avatarInstance));

        }

        private void RecalculateSize(GameObject avatarGameObject, Transform root, bool isAvatar)
        {

            var bounds = GetBounds(avatarGameObject.gameObject);
            var extents = bounds.extents * 2f;
            var avatarFactor = isAvatar ? .2f : 1f;

            extents.x *= avatarFactor;
            extents.z *= avatarFactor;

            Debug.Log($"extents: {extents} scale:{_scale}");

            var boxScale = new Vector3(_scale.x / extents.x, _scale.y / extents.y, _scale.z / extents.z);
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
                    if (extents[i] > _scale[i])
                    {
                        var d = extents[i] - _scale[i];
                        if (d > mostOverlapDistance)
                        {
                            mostOverlap = i;
                            mostOverlapDistance = d;
                        }
                    }
                }
            }

            if (mostOverlap > -1)
                maxBoxScale *= _scale[mostOverlap] / extents[mostOverlap];

            extents = bounds.extents * 2f * maxBoxScale;
            Debug.Log($"extents: {extents} {maxBoxScale} orig {bounds.extents * 2f} d:{mostOverlapDistance} i:{mostOverlap}");
            //var max = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
            //var maxScale = Mathf.Max(Mathf.Max(_scale.x, _scale.y), _scale.z);
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

            if (_scaleToFit)
            {
                root.localScale = Vector3.one * maxBoxScale;
                root.localPosition = (-offsetY) * maxBoxScale + _offset;
            }
            else
            {
                root.localScale = _scale;
                root.localPosition = (-offsetY) * _scale.y + _offset;
            }

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