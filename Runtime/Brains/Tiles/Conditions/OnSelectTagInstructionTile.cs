using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using System;
using UnityEngine;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Input.Enums;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnSelectTagInstructionTile : OnInteractInstructionTile
    {
        public new const string ID = "OnSelectTag";
        public new const string NAME = "Select Tag";
        public new const string CATEGORY = "Input";
        public override Type TileType => typeof(OnSelectTagInstructionTile);

        [SerializeField] private string _monaTag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _monaTag; set => _monaTag = value; }

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;

        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance")] public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }
        
        protected override MonaInputState GetInputState() => MonaInputState.Pressed;

        public override void Preload(IMonaBrain brainInstance)
        {
            base.Preload(brainInstance);
            AddTargetCollider();
        }

        private void AddTargetCollider()
        {
            var colliders = _brain.GameObject.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                var collider = _brain.GameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
            }
        }

        protected override void ProcessLocalInput()
        {
            var localInput = _brainInput.ProcessInput(_brain.LoggingEnabled, MonaInputType.Action, GetInputState());

            if (localInput.GetButton(MonaInputType.Action) == GetInputState())
            {
                _brain.Body.SetLocalInput(localInput);
            }
        }

        public override InstructionTileResult Do()
        {
            if (_bodyInput.GetButton(MonaInputType.Action) == GetInputState())
            {
                if (Raycast(_bodyInput.Ray))
                    return Complete(InstructionTileResult.Success);
            }
            else if (_bodyInput.GetButton(MonaInputType.Action) != MonaInputState.None)
            {
                if (_brain.LoggingEnabled)
                    Debug.Log($"{nameof(OnSelectTagInstructionTile)} button state incorrect {_bodyInput.GetButton(MonaInputType.Action)} is not {GetInputState()}");
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }

        private bool Raycast(Ray ray)
        {
            var targetRayLayer = 1 << 8 | 1 << LayerMask.NameToLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER);
            targetRayLayer = ~targetRayLayer;

            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, _distance, targetRayLayer))
            {
                var body = hit.collider.GetComponentInParent<IMonaBody>();
                if (_brain.LoggingEnabled && body != null)
                    Debug.Log($"{nameof(OnSelectTagInstructionTile)} selected body {body.ActiveTransform.name}", body.ActiveTransform.gameObject);

                if (body != null && body.HasMonaTag(_monaTag))
                {
                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(OnSelectTagInstructionTile)} clicked body with tag {_monaTag} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _brain.State.Set(MonaBrainConstants.RESULT_HIT_TARGET, body);
                    _brain.State.Set(MonaBrainConstants.RESULT_HIT_POINT, hit.point);
                    _brain.State.Set(MonaBrainConstants.RESULT_HIT_NORMAL, hit.normal);
                    return true;
                }
                else
                {
                    _brain.State.Set(MonaBrainConstants.RESULT_HIT_TARGET, (IMonaBody)null);
                    _brain.State.Set(MonaBrainConstants.RESULT_HIT_POINT, Vector3.zero);
                    _brain.State.Set(MonaBrainConstants.RESULT_HIT_NORMAL, Vector3.zero);
                }
            }
            else
            {
                if (_brain.LoggingEnabled)
                    Debug.Log($"{nameof(OnSelectTagInstructionTile)} raycast hit nothing");
            }
            return false;
        }

    }
}