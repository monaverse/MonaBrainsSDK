using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
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
    public class OnCanNotSeeInstructionTile : InstructionTile, ITriggerInstructionTile, IOnNearInstructionTile, 
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IInstructionTileActivate,
        IPauseableInstructionTile
    {
        public const string ID = "OnCanNotSee";
        public const string NAME = "Can Not See";
        public const string CATEGORY = "Vision";
        public override Type TileType => typeof(OnCanNotSeeInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private float _fieldOfView = 45f;
        [SerializeField] private string _fieldOfViewValueName;
        [BrainProperty(true)] public float FieldOfView { get => _fieldOfView; set => _fieldOfView = value; }
        [BrainPropertyValueName("FieldOfView")] public string FieldOfViewValueName { get => _fieldOfViewValueName; set => _fieldOfViewValueName = value; }

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;
        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance")] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        private IMonaBrain _brain;
        private SphereColliderTriggerBehaviour _collider;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter, MonaTriggerType.OnTriggerExit, MonaTriggerType.OnFieldOfViewChanged };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnCanNotSeeInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page)
        {
            _brain = brainInstance;
            if (_collider == null)
            {
                _collider = _brain.GameObject.AddComponent<SphereColliderTriggerBehaviour>();
                _collider.SetBrain(_brain);
                _collider.SetPage(page);
                _collider.SetMonaTag(_tag);
                _collider.MonitorFieldOfView(false);
                UpdateActive();
            }
        }

        public void SetActive(bool active)
        {
            if(_active != active)
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
            if (!string.IsNullOrEmpty(_distanceValueName))
                _distance = _brain.State.GetFloat(_distanceValueName);

            if (!string.IsNullOrEmpty(_fieldOfViewValueName))
                _fieldOfView = _brain.State.GetFloat(_fieldOfViewValueName);

            _collider.SetRadius(_distance);
            var body = _collider.FindForwardMostBodyWithMonaTagOutsideFieldOfView(_tag, _fieldOfView);
            if (body != null)
            {
                //Debug.Log($"{nameof(OnNearInstructionTile)}.{nameof(Do)} found: {body}");
                _brain.State.Set(MonaBrainConstants.RESULT_TARGET, body);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}