using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Interfaces;

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
            ProcessButton(_localInputs.Player.Action);

            if (_currentLocalInputState != MonaInputState.None && _currentLocalInputState == GetInputState())
            {
                MonaLocalRayInput input = new MonaLocalRayInput();
                input.Type = MonaInputType.Action;
                input.State = _currentLocalInputState;

                var mouse = Mouse.current.position.ReadValue();
                input.Value = _brain.Player.PlayerCamera.Transform.GetComponent<Camera>().ScreenPointToRay(new Vector3(mouse.x, mouse.y, 0f));

                _brain.Body.SetLocalInput(input);
            }
        }

        public override InstructionTileResult Do()
        {
            if (_bodyInputs != null)
            {
                for (var i = 0; i < _bodyInputs.Count; i++)
                {
                    var input = _bodyInputs[i];
                    if (input is IMonaLocalRayInput && input.Type == MonaInputType.Action && input.State == GetInputState())
                    {
                        if(Raycast((IMonaLocalRayInput)input))
                            return Complete(InstructionTileResult.Success);
                    }
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }

        private bool Raycast(IMonaLocalRayInput input)
        {
            var targetRayLayer = 1 << 8 | 1 << LayerMask.NameToLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER);
            targetRayLayer = ~targetRayLayer;

            RaycastHit hit;
            if (Physics.Raycast(input.Value.origin, input.Value.direction, out hit, _distance, targetRayLayer))
            {
                var body = hit.collider.GetComponentInParent<IMonaBody>();
                if (_brain.LoggingEnabled && body != null)
                    Debug.Log($"selected body {body.ActiveTransform.name}", body.ActiveTransform.gameObject);

                if (body != null && body.HasMonaTag(_monaTag))
                {
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
            return false;
        }

    }
}