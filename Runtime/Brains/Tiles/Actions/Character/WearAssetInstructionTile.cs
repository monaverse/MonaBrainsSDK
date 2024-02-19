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

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag] public string Tag { get => _tag; set => _tag = value; }

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
            _wearableInstance = GameObject.Instantiate(_wearable.Value);
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private IMonaBody GetTarget()
        {
            if (_brain.MonaTagSource.GetTag(_tag).IsPlayerTag)
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
            var body = GetTarget();
            if(body != null)
            { 
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);

                _monaAnimationController.SetTPose(true);
                _brain.Runner.WaitFrame(PutOn);
            }
         
            return Complete(InstructionTileResult.Success);
        }

        private void PutOn()
        {
            var transforms = new List<Transform>(_brain.Body.Animator.transform.GetComponentsInChildren<Transform>());
            transforms.Remove(_brain.Body.Animator.transform);

            _wearableInstance.transform.SetParent(_brain.Body.Animator.transform);
            _wearableInstance.transform.localPosition = _offset;
            _wearableInstance.transform.localRotation = Quaternion.Euler(_eulerAngles);

            var wearableTransforms = new List<Transform>(_wearableInstance.GetComponentsInChildren<Transform>());
            wearableTransforms.Remove(_wearableInstance.transform);

            for(var i = 0;i < transforms.Count; i++)
            {
                var t = transforms[i];
                var wearableTransform = wearableTransforms.Find(x => x.transform.name == t.transform.name);
                if (wearableTransform == null) continue;
                wearableTransform.SetParent(t, true);
            }

            _monaAnimationController.SetTPose(false);
        }

    }
}