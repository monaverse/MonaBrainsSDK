using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnSelectInstructionTile : OnInteractInstructionTile
    {
        public new const string ID = "OnSelect";
        public new const string NAME = "Select";
        public new const string CATEGORY = "Input";
        public override Type TileType => typeof(OnSelectInstructionTile);

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;

        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }
        
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
                //if (_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(OnSelectInstructionTile)} setlocalinput {localInput.Ray.origin} {localInput.Ray.direction}");

                _brain.Body.SetLocalInput(localInput);
            }
        }

        public override InstructionTileResult Do()
        {
            if(_bodyInput.GetButton(MonaInputType.Action) == GetInputState())
            {
                if (Raycast(_bodyInput.Ray))
                    return Complete(InstructionTileResult.Success);
            }
            else if (_bodyInput.GetButton(MonaInputType.Action) != MonaInputState.None)
            {
               // if (_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(OnSelectInstructionTile)} button state inccorrect {_bodyInput.GetButton(MonaInputType.Action)} is not {GetInputState()}");
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }

        private bool Raycast(Ray ray)
        {
            var targetRayLayer = 1 << 8 | 1 << LayerMask.NameToLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER);
            targetRayLayer = ~targetRayLayer;

            RaycastHit hit;
            //if (_brain.LoggingEnabled)
             //   Debug.Log($"{nameof(OnSelectInstructionTile)} raycast {_bodyInput.Ray.origin} {_bodyInput.Ray.direction}");

            if (Physics.Raycast(ray.origin, ray.direction, out hit, _distance, targetRayLayer))
            {
                //if (_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(OnSelectInstructionTile)} {hit.point} {hit.collider}");

                var body = hit.collider.GetComponentInParent<IMonaBody>();
                if (_brain.LoggingEnabled && body != null)
                    Debug.Log($"selected body {body.ActiveTransform.name}", body.ActiveTransform.gameObject);

                if (body != null && body == _brain.Body)
                {
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_TARGET, body);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_POINT, hit.point);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_NORMAL, hit.normal);
                    return true;
                }
                else if (_brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET) != null)
                {
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_TARGET, (IMonaBody)null);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_POINT, Vector3.zero);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_NORMAL, Vector3.zero);
                }
            }
            else
            {

                ////if (_brain.LoggingEnabled)
                   // Debug.Log($"{nameof(OnSelectInstructionTile)}raycasdt didn't hit anything");
            }
            return false;
        }
    }
}