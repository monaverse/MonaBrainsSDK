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
    public class OnFarInstructionTile : InstructionTile, ITriggerInstructionTile, IOnFarInstructionTile, 
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IInstructionTileActivate,
        IPauseableInstructionTile
    {
        public const string ID = "OnFar";
        public const string NAME = "Far";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(OnFarInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private float _distance = 2f;
        [SerializeField] private string _distanceValueName;
        [BrainProperty(true)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance")] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        private IMonaBrain _brain;
        private SphereColliderTriggerBehaviour _collider;
        private GameObject _gameObject;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerExit };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnFarInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            if (_collider == null)
            {
                _collider = _brain.GameObject.AddComponent<SphereColliderTriggerBehaviour>();
                _collider.SetBrain(_brain);
                _collider.SetMonaTag(_tag);
                UpdateActive();
            }
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (_collider != null)
                _collider.SetActive(_active);
        }

        public void Pause()
        {
            if (_collider != null)
                _collider.SetActive(false);
        }

        public void Resume()
        {
            UpdateActive();
        }

        public override void Unload()
        {
            if (_collider != null)
            {
                _collider.Dispose();
                GameObject.Destroy(_collider);
            }
        }

        public override InstructionTileResult Do()
        {
            if (_collider == null) return InstructionTileResult.Failure;

            if (!string.IsNullOrEmpty(_distanceValueName))
                _distance = _brain.State.GetFloat(_distanceValueName);

            _collider.SetRadius(_distance);
            var body = _collider.FindClosestOutOfRangeWithMonaTag(_tag);
            if (body != null && _collider.FindClosestInRangeWithMonaTag(_tag) == null)
            {
                Debug.Log($"{nameof(OnFarInstructionTile)}.{nameof(Do)} found: {body}");
                _brain.State.Set(MonaBrainConstants.RESULT_TARGET, body);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}