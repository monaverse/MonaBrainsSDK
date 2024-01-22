using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Behaviours;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnCloseToTagInstructionTile : InstructionTile, ITriggerInstructionTile, IOnCloseToTagInstructionTile, IDisposable, IConditionInstructionTile
    {
        public const string ID = "OnCloseToTag";
        public const string NAME = "On Close To Tag";
        public const string CATEGORY = "Condition";
        public override Type TileType => typeof(OnCloseToTagInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private float _distance = 2f;
        [SerializeField] private string _distanceValueName;
        [BrainProperty(true)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance")] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        [SerializeField] private float _fieldOfView = 180f;
        [SerializeField] private string _fieldOfViewValueName;
        [BrainProperty(false)] public float FieldOfView { get => _fieldOfView; set => _fieldOfView = value; }
        [BrainPropertyValueName("FieldOfView")] public string FieldOfViewValueName { get => _fieldOfViewValueName; set => _fieldOfViewValueName = value; }

        private IMonaBrain _brain;
        private SphereColliderTriggerBehaviour _collider;
        private GameObject _gameObject;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnCloseToTagInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            if (_collider == null)
            {
                _collider = _brain.GameObject.AddComponent<SphereColliderTriggerBehaviour>();
                _collider.SetBrain(_brain);
            }
        }

        public void Dispose()
        {
            GameObject.Destroy(_collider);
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_distanceValueName))
                _distance = _brain.State.GetFloat(_distanceValueName);

            if (!string.IsNullOrEmpty(_fieldOfViewValueName))
                _fieldOfView = _brain.State.GetFloat(_fieldOfViewValueName);

            _collider.SetRadius(_distance);
            var body = _collider.FindForwardMostBodyWithMonaTagInFieldOfView(_tag, _fieldOfView);
            if (body != null)
            {
                Debug.Log($"{nameof(OnCloseToTagInstructionTile)}.{nameof(Do)} found: {body}");
                _brain.State.Set(MonaBrainConstants.RESULT_TARGET, body);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}