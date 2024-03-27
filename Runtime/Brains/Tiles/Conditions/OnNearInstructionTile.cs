using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Behaviours;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnNearInstructionTile : InstructionTile, ITriggerInstructionTile, IOnNearInstructionTile,
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IActivateInstructionTile,
        IPauseableInstructionTile, IPlayerTriggeredConditional, ITickAfterInstructionTile, IRigidbodyInstructionTile
    {
        public const string ID = "OnNear";
        public const string NAME = "Near";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(OnNearInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private float _distance = 2f;
        [SerializeField] private string _distanceValueName;
        [BrainProperty(true)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        [SerializeField] private float _fieldOfView = 360f;
        [SerializeField] private string _fieldOfViewValueName;
        [BrainProperty(false)] public float FieldOfView { get => _fieldOfView; set => _fieldOfView = value; }
        [BrainPropertyValueName("FieldOfView", typeof(IMonaVariablesFloatValue))] public string FieldOfViewValueName { get => _fieldOfViewValueName; set => _fieldOfViewValueName = value; }

        public bool PlayerTriggered => _brain.HasPlayerTag() || _brain.MonaTagSource.GetTag(_tag).IsPlayerTag;

        private IMonaBrain _brain;
        private SphereColliderTriggerBehaviour _collider;
        private GameObject _gameObject;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnNearInstructionTile() { }

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
                _collider.SetRadius(_distance);
                _collider.SetLocalPlayerOnly(PlayerTriggered);
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

        private void UpdateActive()
        {
            if (_brain != null && _brain.LoggingEnabled)
                Debug.Log($"{nameof(OnNearInstructionTile)}.{nameof(UpdateActive)} {_active}");
            if (_collider != null)
                _collider.SetActive(_active);
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
                _distance = _brain.Variables.GetFloat(_distanceValueName);

            if (!string.IsNullOrEmpty(_fieldOfViewValueName))
                _fieldOfView = _brain.Variables.GetFloat(_fieldOfViewValueName);

            _collider.SetRadius(_distance);
            var bodies = _collider.FindForwardMostBodyWithMonaTagInFieldOfView(_tag, _fieldOfView);
            //Debug.Log($"{nameof(OnNearInstructionTile)}.{nameof(Do)} chck on near: {_tag} {bodies?.Count}", _brain.Body.ActiveTransform.gameObject);
            if (bodies != null)
            {
                FilterBodiesOnInstruction(bodies);
                if (_bodies.Count > 0)
                {
                    var body = _bodies[0];
                    //if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(OnNearInstructionTile)}.{nameof(Do)} found: {_tag} {body} {_distance}", _brain.Body.ActiveTransform.gameObject);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, body);
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}