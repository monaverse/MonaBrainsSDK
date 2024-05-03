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

namespace Mona.SDK.Brains.Tiles.Actions.Character
{
    [Serializable]
    public class ChangeAvatarInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "ChangeAvatar";
        public const string NAME = "Change Avatar Asset";
        public const string CATEGORY = "Character";
        public override Type TileType => typeof(ChangeAvatarInstructionTile);

        public bool IsAnimationTile => _target == MonaBrainBroadcastType.Self;

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

        public ChangeAvatarInstructionTile() { }

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

        public override InstructionTileResult Do()
        {
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
                case MonaBrainBroadcastType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
            }
            return null;
        }

        private InstructionTileResult ChangeAvatar(IMonaBody body)
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

        private GameObject _avatarLoader;
        private void LoadAvatarAtUrl(string url, IMonaBody body)
        {
            if (_avatarLoader == null)
                _avatarLoader = new GameObject("AvatarLoader");

            var loader = _avatarLoader.GetComponent<BrainsVrmLoader>();
            if (loader == null)
                loader = _avatarLoader.AddComponent<BrainsVrmLoader>();

            loader.Load(url, _importAnimation, (avatar) =>
            {
                if (avatar != null)
                {
                    var animator = avatar.GetComponent<Animator>();
                    if (animator == null)
                        animator = avatar.AddComponent<Animator>();
                    ParseHumanoid(avatar, animator);
                    try
                    {
                        LoadAvatar(animator, body, avatar);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{nameof(LoadAvatarAtUrl)} {e.Message} {e.StackTrace}");
                    }
                }
                Complete(InstructionTileResult.Success, true);
            });
        }

        //thank you gpt!
        private Dictionary<HumanBodyBones, string[]> _boneSynonyms = new Dictionary<HumanBodyBones, string[]> {
            { HumanBodyBones.Hips, new string[] { "Hips" } },
            { HumanBodyBones.LeftUpperLeg, new string[] { "LeftUpperLeg", "UpperLeg.L", "Upper_Leg.L", "L_UpperLeg" } },
            { HumanBodyBones.RightUpperLeg, new string[] { "RightUpperLeg", "UpperLeg.R", "Upper_Leg.R", "R_UpperLeg" } },
            { HumanBodyBones.LeftLowerLeg, new string[] { "LeftLowerLeg", "LowerLeg.L", "Lower_Leg.L", "L_LowerLeg" } },
            { HumanBodyBones.RightLowerLeg, new string[] { "RightLowerLeg", "LowerLeg.R", "Lower_Leg.R", "R_LowerLeg" } },
            { HumanBodyBones.LeftFoot, new string[] { "LeftFoot", "Foot.L", "L_Foot" } },
            { HumanBodyBones.RightFoot, new string[] { "RightFoot", "Foot.R", "R_Foot" } },
            { HumanBodyBones.Spine, new string[] { "Spine" } },
            { HumanBodyBones.Chest, new string[] { "Chest" } },
            { HumanBodyBones.UpperChest, new string[] { "UpperChest" } },
            { HumanBodyBones.Neck, new string[] { "Neck" } },
            { HumanBodyBones.Head, new string[] { "Head" } },
            { HumanBodyBones.LeftShoulder, new string[] { "LeftShoulder", "Shoulder.L", "ShoulderLeft", "Shoulder_Left", "L_Shoulder" } },
            { HumanBodyBones.RightShoulder, new string[] { "RightShoulder", "Shoulder.R", "ShoulderRight", "Shoulder_Right", "R_Shoulder" } },
            { HumanBodyBones.LeftUpperArm, new string[] { "LeftUpperArm", "UpperArm.L", "Upper_Arm.L", "UpperArmLeft", "UpperArm_Left", "Upper_Arm_Left", "L_UpperArm" } },
            { HumanBodyBones.RightUpperArm, new string[] { "RightUpperArm", "UpperArm.R", "Upper_Arm.R", "UpperArmRight", "UpperArm_Right", "Upper_Arm_Right", "R_UpperArm" } },
            { HumanBodyBones.LeftLowerArm, new string[] { "LeftLowerArm", "LowerArm.L", "Lower_Arm.L", "LowerArmLeft", "LowerArm_Left", "Lower_Arm_Left", "L_LowerArm" } },
            { HumanBodyBones.RightLowerArm, new string[] { "RightLowerArm", "LowerArm.R", "Lower_Arm.R", "LowerArmRight", "LowerArm_Right", "Lower_Arm_Right", "R_LowerArm" } },
            { HumanBodyBones.LeftHand, new string[] { "LeftHand", "Hand.L", "Left_Hand", "Hand_Left", "L_Hand" } },
            { HumanBodyBones.RightHand, new string[] { "RightHand", "Hand.R", "Right_Hand", "Hand_Right", "R_Hand" } },
            { HumanBodyBones.LeftToes, new string[] { "LeftToes", "Toes.L", "Left_Toes", "Toes_Left", "L_Toes" } },
            { HumanBodyBones.RightToes, new string[] { "RightToes", "Toes.R", "Right_Toes", "Toes_Right", "R_Toes" } },
            { HumanBodyBones.LeftEye, new string[] { "LeftEye", "Eye.L", "Left_Eye", "Toes_Eye", "L_Eye" } },
            { HumanBodyBones.RightEye, new string[] { "RightEye", "Eye.R", "Right_Eye", "Toes_Eye", "R_Eye" } },
            { HumanBodyBones.Jaw, new string[] { "Jaw" } },
            { HumanBodyBones.LeftThumbProximal, new string[] { "LeftThumbProximal", "ThumbProximal.L", "ThumbProximal.Left", "Thumb_Proximal_Left", "L_ThumbProximal" } },
            { HumanBodyBones.RightThumbProximal, new string[] { "RightThumbProximal", "ThumbProximal.R", "ThumbProximal.Right", "Thumb_Proximal_Right", "R_ThumbProximal" } },
            { HumanBodyBones.LeftThumbIntermediate, new string[] { "LeftThumbIntermediate", "ThumbIntermediate.L", "ThumbIntermediate.Left", "Thumb_Intermediate_Left", "L_ThumbIntermediate" } },
            { HumanBodyBones.RightThumbIntermediate, new string[] { "RightThumbIntermediate", "ThumbIntermediate.R", "ThumbIntermediate.Right", "Thumb_Intermediate_Right", "R_ThumbIntermediate" } },
            { HumanBodyBones.LeftThumbDistal, new string[] { "LeftThumbDistal", "ThumbDistal.L", "ThumbDistal.Left", "Thumb_Distal_Left", "L_ThumbDistal" } },
            { HumanBodyBones.RightThumbDistal, new string[] { "RightThumbDistal", "ThumbDistal.R", "ThumbDistal.Right", "Thumb_Distal_Right", "R_ThumbDistal" } },
            { HumanBodyBones.LeftIndexProximal, new string[] { "LeftIndexProximal", "IndexProximal.L", "IndexProximal.Left", "Index_Proximal_Left", "L_IndexProximal" } },
            { HumanBodyBones.RightIndexProximal, new string[] { "RightIndexProximal", "IndexProximal.R", "IndexProximal.Right", "Index_Proximal_Right", "R_IndexProximal" } },
            { HumanBodyBones.LeftIndexIntermediate, new string[] { "LeftIndexIntermediate", "IndexIntermediate.L", "IndexIntermediate.Left", "Index_Intermediate_Left", "L_IndexIntermediate" } },
            { HumanBodyBones.RightIndexIntermediate, new string[] { "RightIndexIntermediate", "IndexIntermediate.R", "IndexIntermediate.Right", "Index_Intermediate_Right", "R_IndexIntermediate" } },
            { HumanBodyBones.LeftIndexDistal, new string[] { "LeftIndexDistal", "IndexDistal.L", "IndexDistal.Left", "Index_Distal_Left", "L_IndexDistal" } },
            { HumanBodyBones.RightIndexDistal, new string[] { "RightIndexDistal", "IndexDistal.R", "IndexDistal.Right", "Index_Distal_Right", "R_IndexDistal" } },
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

        private void ParseHumanoid(GameObject avatar, Animator animator)
        {
            if(animator.avatar == null)
            {
                var transforms = new List<Transform>(avatar.GetComponentsInChildren<Transform>());
                var bones = Enum.GetValues(typeof(HumanBodyBones));
                var skeleton = new Dictionary<HumanBodyBones, Transform>();

                try
                {
                    foreach (int bone in bones)
                    {
                        var syns = _boneSynonyms[(HumanBodyBones)bone];
                        for (var i = 0; i < syns.Length; i++)
                        {
                            var syn = syns[i];
                            var t = transforms.Find(x => x.name.Equals(syn, StringComparison.OrdinalIgnoreCase));
                            if (t != null)
                            {
                                skeleton[(HumanBodyBones)bone] = t;
                                break;
                            }
                            else
                            {
                                t = transforms.Find(x => x.name.ContainsInsensitive(syn));
                                if (t != null)
                                {
                                    skeleton[(HumanBodyBones)bone] = t;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"{nameof(ParseHumanoid)} {e.Message}");
                    return;
                }

                if (skeleton.ContainsKey(HumanBodyBones.Hips) &&
                    (skeleton.ContainsKey(HumanBodyBones.LeftUpperLeg) ||
                    skeleton.ContainsKey(HumanBodyBones.RightUpperLeg) ||
                    skeleton.ContainsKey(HumanBodyBones.RightUpperArm) ||
                    skeleton.ContainsKey(HumanBodyBones.LeftUpperArm)) &&
                    skeleton.ContainsKey(HumanBodyBones.Head) &&
                    skeleton.ContainsKey(HumanBodyBones.Spine))

                {

                    var description = AvatarDescription.Create(skeleton).ToHumanDescription(avatar.transform);
                    var avatarInst = AvatarBuilder.BuildHumanAvatar(avatar, description);

                    animator.avatar = avatarInst;
                }
            }
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

                _monaAnimationController.ReuseController = _reuseController;
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
                                var t = transforms.Find(x => x.name.StartsWith(tag, StringComparison.OrdinalIgnoreCase));
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

            var playerId = _brain.Player.GetPlayerIdByBody(_brain.Body);
            if(playerId > -1)
                MonaEventBus.Trigger<MonaPlayerChangeAvatarEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_CHANGE_AVATAR_EVENT), new MonaPlayerChangeAvatarEvent(playerId, _avatarInstance));
            MonaEventBus.Trigger<MonaChangeAvatarEvent>(new EventHook(MonaCoreConstants.ON_CHANGE_AVATAR_EVENT), new MonaChangeAvatarEvent(_avatarInstance));

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