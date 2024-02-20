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

        public ChangeAvatarInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            SetupWearable();
        }

        private void SetupWearable()
        {
            _root = _brain.Root;
            _monaAnimationController = _root.GetComponent<IMonaAnimationController>();
            _monaAnimationController.SetBrain(_brain);

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

            var boneMappings = _brain.Body.Animator.GetComponent<VRMHumanoidDescription>();
            if (boneMappings != null)
            {
                var avatarTransforms = new List<Transform>(_brain.Body.Animator.transform.GetComponentsInChildren<Transform>());

                AvatarDescription description = boneMappings.GetDescription(out bool isCreated);
                var avatar = description.ToHumanDescription(_brain.Body.Animator.transform);

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

            _avatarInstance.transform.localPosition = _offset;
            _avatarInstance.transform.localRotation = Quaternion.Euler(_eulerAngles);
        }

        public override void Unload()
        {
            base.Unload();
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