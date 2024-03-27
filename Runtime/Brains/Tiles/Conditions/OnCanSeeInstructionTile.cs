using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Behaviours;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Mona.SDK.Brains.Tiles.Conditions.Behaviours.SphereColliderTriggerBehaviour;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnCanSeeInstructionTile : InstructionTile, ITriggerInstructionTile, IOnNearInstructionTile,
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IActivateInstructionTile,
        IPauseableInstructionTile, IRigidbodyInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "OnCanSee";
        public const string NAME = "Can See";
        public const string CATEGORY = "Vision";
        public override Type TileType => typeof(OnCanSeeInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private float _fieldOfView = 45f;
        [SerializeField] private string _fieldOfViewValueName;
        [BrainProperty(true)] public float FieldOfView { get => _fieldOfView; set => _fieldOfView = value; }
        [BrainPropertyValueName("FieldOfView", typeof(IMonaVariablesFloatValue))] public string FieldOfViewValueName { get => _fieldOfViewValueName; set => _fieldOfViewValueName = value; }

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;
        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        private IMonaBrain _brain;
        private SphereColliderTriggerBehaviour _collider;
        private GameObject _gameObject;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter, MonaTriggerType.OnTriggerExit, MonaTriggerType.OnFieldOfViewChanged };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnCanSeeInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            _firstTile = _instruction.InstructionTiles.IndexOf(this) == 0;

            if (_collider == null)
            {
                _collider = _brain.GameObject.AddComponent<SphereColliderTriggerBehaviour>();
                _collider.SetBrain(_brain);
                _collider.SetPage(page);
                _collider.SetMonaTag(_tag);
                _collider.MonitorFieldOfView(true);
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
            if (!_active) return;
            if (_collider != null)
                _collider.SetActive(_active);
        }

        public void Pause()
        {
            if (_collider != null)
                _collider.SetActive(false);
        }

        public bool Resume()
        {
            UpdateActive();
            return false;
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
                _distance = _brain.Variables.GetFloat(_distanceValueName);

            if (!string.IsNullOrEmpty(_fieldOfViewValueName))
                _fieldOfView = _brain.Variables.GetFloat(_fieldOfViewValueName);

            _collider.SetRadius(_distance);
            var bodies = _collider.FindForwardMostBodyWithMonaTagInFieldOfView(_tag, _fieldOfView);

            if (bodies != null)
            {
                FilterBodiesOnInstruction(bodies);

                Debug.Log($"{nameof(OnCanSeeInstructionTile)} can see {_bodies.Count}");

                if (_bodies.Count > 0)
                {
                    var body = _bodies[0];

                    //if(_brain.LoggingEnabled) 
                    //    Debug.Log($"{nameof(OnNearInstructionTile)}.{nameof(Do)} found: {body} {_distance}", _brain.Body.Transform.gameObject);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, body);
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }


    }
}