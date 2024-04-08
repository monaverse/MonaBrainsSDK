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

namespace Mona.SDK.Brains.Tiles.Actions.Character
{
    [Serializable]
    public class ChangeAvatarInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "ChangeAvatar";
        public const string NAME = "Change Avatar Asset";
        public const string CATEGORY = "Character";
        public override Type TileType => typeof(ChangeAvatarInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaAvatarAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _eulerAngles = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Rotation { get => _eulerAngles; set => _eulerAngles = value; }

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

            OnAnimationControllerChanged = HandleAnimationControllerChanged;
            EventBus.Register<MonaBodyAnimationControllerChangedEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            SetupAnimation();
        }

        private void HandleAnimationControllerChanged(MonaBodyAnimationControllerChangedEvent evt)
        {
            SetupAnimation();
        }

        private void SetupAnimation()
        {
            if (_brain.Root != null)
                _monaAnimationController = _brain.Root.GetComponent<IMonaAnimationController>();
            else
            {
                var children = _brain.Body.Children();
                for (var i = 0; i < children.Count; i++)
                {
                    var root = children[i].Transform.Find("Root");
                    if (root != null)
                    {
                        _monaAnimationController = _brain.Root.GetComponent<IMonaAnimationController>();
                        if (_monaAnimationController != null) break;
                    }
                }
            }
            _avatarAsset = (IMonaAvatarAssetItem)_brain.GetMonaAsset(_monaAsset);
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
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
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                if (_avatarAsset.Value != null)
                {
                    LoadAvatar(GameObject.Instantiate(_avatarAsset.Value));
                    return Complete(InstructionTileResult.Success);
                }
                else if (!string.IsNullOrEmpty(_avatarAsset.Url))
                {
                    LoadAvatarAtUrl(_avatarAsset.Url);
                    return Complete(InstructionTileResult.Running);
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        private void LoadAvatarAtUrl(string url)
        {
            var loader = _brain.Body.Transform.GetComponent<BrainsVrmLoader>();
            if (loader == null)
                loader = _brain.Body.Transform.AddComponent<BrainsVrmLoader>();

            loader.Load(url, (avatar) =>
            {
                if (avatar != null)
                {
                    var animator = avatar.GetComponent<Animator>();
                    if (animator == null)
                        animator = avatar.AddComponent<Animator>();
                    LoadAvatar(animator);
                }
                Complete(InstructionTileResult.Success, true);
            });
        }

        private void LoadAvatar(Animator animator)
        {
            _avatarInstance = animator;
            _avatarInstance.transform.SetParent(_brain.Root);
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

            var body = _brain.Body;
            while (body != null)
            {
                EventBus.Trigger<MonaBodyAnimationControllerChangeEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGE_EVENT, body), new MonaBodyAnimationControllerChangeEvent(_avatarInstance));
                body = body.Parent;
            }

            _avatarInstance.transform.localPosition = _offset;
            _avatarInstance.transform.localRotation = Quaternion.Euler(_eulerAngles);

            Debug.Log($"{_avatarInstance} {_offset} {_avatarInstance.transform.position} brain body {_brain.Body.Transform.position}");

            var playerId = _brain.Player.GetPlayerIdByBody(_brain.Body);
            if(playerId > -1)
                EventBus.Trigger<MonaPlayerChangeAvatarEvent>(new EventHook(MonaCoreConstants.ON_PLAYER_CHANGE_AVATAR_EVENT), new MonaPlayerChangeAvatarEvent(playerId, _avatarInstance));

            Debug.Log($"{_avatarInstance} {_offset} {_avatarInstance.transform.position} brain body1 {_brain.Body.Transform.position}");

        }

        public override void Unload()
        {
            base.Unload();

            EventBus.Unregister(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            if (_wearableTransforms != null)
            {
                for (var i = 0; i < _wearableTransforms.Count; i++)
                    GameObject.Destroy(_wearableTransforms[i].gameObject);
            }
            if(_avatarInstance != null)
                GameObject.Destroy(_avatarInstance.transform.gameObject);
            _avatarInstance = null;
        }

    }
}