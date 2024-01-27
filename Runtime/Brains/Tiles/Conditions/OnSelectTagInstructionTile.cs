using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;

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
        
        protected override MonaInputState _inputState { get => MonaInputState.Pressed; }

        public override void Preload(IMonaBrain brainInstance)
        {
            base.Preload(brainInstance);

            var colliders = _brain.GameObject.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0)
            {
                _brain.GameObject.AddComponent<BoxCollider>();
            }

        }

        public override InstructionTileResult Do()
        {
            if (_currentInputState == _inputState)
            {
                var targetRayLayer = 1 << 8 | 1 << LayerMask.NameToLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER);
                targetRayLayer = ~targetRayLayer;
                var mouse = Mouse.current.position.ReadValue();
                var world = _brain.Player.PlayerCamera.Transform.GetComponent<Camera>().ScreenPointToRay(new Vector3(mouse.x, mouse.y, 0f));
                RaycastHit hit;
                if (Physics.Raycast(world.origin, world.direction, out hit, _distance, targetRayLayer))
                {
                    var body = hit.collider.GetComponentInParent<IMonaBody>();
                    if (body != null && body.HasMonaTag(_monaTag))
                    {
                        _brain.State.Set(MonaBrainConstants.RESULT_HIT_TARGET, body);
                        _brain.State.Set(MonaBrainConstants.RESULT_HIT_POINT, hit.point);
                        _brain.State.Set(MonaBrainConstants.RESULT_HIT_NORMAL, hit.normal);
                        return Complete(InstructionTileResult.Success);
                    }
                    else
                    {
                        _brain.State.Set(MonaBrainConstants.RESULT_HIT_TARGET, (IMonaBody)null);
                        _brain.State.Set(MonaBrainConstants.RESULT_HIT_POINT, Vector3.zero);
                        _brain.State.Set(MonaBrainConstants.RESULT_HIT_NORMAL, Vector3.zero);
                    }
                    return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_HIT);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}