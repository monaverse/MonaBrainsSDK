using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core;
using Mona.SDK.Core.Assets.Interfaces;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Animation;
using VRM;
using UniHumanoid;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;

namespace Mona.SDK.Brains.Tiles.Actions.Character
{
    [Serializable]
    public class WearAssetInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IAnimationInstructionTile
    {
        public const string ID = "WearAsset";
        public const string NAME = "Wear Asset";
        public const string CATEGORY = "Character";
        public override Type TileType => typeof(WearAssetInstructionTile);

        [SerializeField] private string _monaAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaWearableAssetItem))] public string MonaAsset { get => _monaAsset; set => _monaAsset = value; }

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
        private IMonaWearableAssetItem _wearable;
        private GameObject _wearableInstance;
        private List<Transform> _wearableTransforms;

        public WearAssetInstructionTile() { }

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

            _wearable = (IMonaWearableAssetItem)_brain.GetMonaAsset(_monaAsset);
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private IMonaBody GetTarget()
        {
            return _brain.Body;
        }

        private Action<MonaLateTickEvent> OnLateTick;
        public override InstructionTileResult Do()
        {
            var body = GetTarget();
            if (body != null)
            {
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                _monaAnimationController.SetTPose(true);

                OnLateTick = HandleLateTick;
                EventBus.Register<MonaLateTickEvent>(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), OnLateTick);
            }

            return Complete(InstructionTileResult.Success);
        }

        private void HandleLateTick(MonaLateTickEvent evt)
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), OnLateTick);

            var transforms = new List<Transform>(_brain.Body.Animator.transform.GetComponentsInChildren<Transform>());
            transforms.Remove(_brain.Body.Animator.transform);

            _wearableInstance = GameObject.Instantiate(_wearable.Value);
            _wearableTransforms = new List<Transform>(_wearableInstance.transform.GetComponentsInChildren<Transform>());
            _wearableTransforms.Remove(_wearableInstance.transform);

            _wearableInstance.transform.SetParent(_brain.Body.Animator.transform);
            _wearableInstance.transform.localPosition = _offset;
            _wearableInstance.transform.localRotation = Quaternion.Euler(_eulerAngles);

            var boneMappings = _brain.Body.Animator.GetComponent<VRMHumanoidDescription>();
            if (boneMappings != null)
            {
                var wearableTransforms = new List<Transform>(_wearableInstance.GetComponentsInChildren<Transform>());
                var avatarTransforms = new List<Transform>(_brain.Body.Animator.transform.GetComponentsInChildren<Transform>());

                AvatarDescription description = boneMappings.GetDescription(out bool isCreated);
                var avatar = description.ToHumanDescription(_brain.Body.Animator.transform);
                var skeletonBones = new List<SkeletonBone>(avatar.skeleton);


                for (var i = 0; i < avatar.human.Length;i++)
                {
                    var boneDef = avatar.human[i];
                    var humanBone = boneDef.humanName;
                    
                    var wearableBone = wearableTransforms.Find(x => x.name == boneDef.boneName || $"mixamorig:{x.name}" == boneDef.boneName);
                    if(wearableBone == null)
                        wearableBone = wearableTransforms.Find(x => x.name == boneDef.humanName);

                    var avatarBone = avatarTransforms.Find(x => x.name == boneDef.boneName);
                    if(wearableBone != null)
                    {
                        if(avatarBone != null)
                        {
                            wearableBone.position = avatarBone.position;
                            wearableBone.SetParent(avatarBone, true);
                        }
                    }
                }
            }
            else
            { 
                var wearableTransforms = new List<Transform>(_wearableInstance.GetComponentsInChildren<Transform>());
                wearableTransforms.Remove(_wearableInstance.transform);

                for (var i = 0; i < transforms.Count; i++)
                {
                    var t = transforms[i];
                    var wearableTransform = wearableTransforms.Find(x => x.transform.name == t.transform.name);
                    if (wearableTransform == null) continue;
                    wearableTransform.SetParent(t, true);
                }
            }

            _monaAnimationController.SetTPose(false);
        }

        public override void Unload()
        {
            base.Unload();
            if (_wearableTransforms != null)
            {
                for (var i = 0; i < _wearableTransforms.Count; i++)
                    GameObject.Destroy(_wearableTransforms[i].gameObject);
            }
            if(_wearableInstance != null)
                GameObject.Destroy(_wearableInstance.transform.gameObject);
            _wearableInstance = null;
        }

    }
}
