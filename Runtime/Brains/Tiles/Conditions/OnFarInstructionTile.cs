using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Behaviours;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Utils;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnFarInstructionTile : InstructionTile, ITriggerInstructionTile, IOnFarInstructionTile, 
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IActivateInstructionTile,
        IPauseableInstructionTile, IPlayerTriggeredConditional, ITickAfterInstructionTile, IRigidbodyInstructionTile,
        IOnBodyFilterInstructionTile
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
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        public bool PlayerTriggered => _brain.HasPlayerTag();

        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        private SphereColliderTriggerBehaviour _collider;
        private GameObject _gameObject;
        private Rigidbody _rigidbody;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter, MonaTriggerType.OnTriggerExit };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        private Action<MonaBodyRigidbodyChangedEvent> OnRigidbodyChanged;

        public OnFarInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _page = page;
            _instruction = instruction;

            _firstTile = _instruction.InstructionTiles.FindAll(x => x is IOnBodyFilterInstructionTile).IndexOf(this) == 0;

            OnRigidbodyChanged = HandleRigidbodyChanged;
            MonaEventBus.Register<MonaBodyRigidbodyChangedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_RIGIDBODY_CHANGED_EVENT, _brain.Body), OnRigidbodyChanged);

            OnRigidbodyChanged(new MonaBodyRigidbodyChangedEvent());
        }

        public void HandleRigidbodyChanged(MonaBodyRigidbodyChangedEvent evt)
        {
            if (_brain.Body.ActiveRigidbody == null)
                return;

            if (_collider != null && _rigidbody != _brain.Body.ActiveRigidbody)
            {
                GameObject.DestroyImmediate(_collider);
                _collider = null;
            }

            if (_collider == null)
            {
                _collider = _brain.GameObject.AddComponent<SphereColliderTriggerBehaviour>();
                _collider.SetBrain(_brain);
                _collider.SetPage(_page);
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

        public bool Resume()
        {
            UpdateActive();
            return false;
        }

        public override void Unload(bool destroy = false)
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

            _collider.SetRadius(_distance);
            var bodies = _collider.FindClosestOutOfRangeWithMonaTag(_tag);
            //if(_brain.LoggingEnabled) Debug.Log($"{nameof(OnFarInstructionTile)}.{nameof(Do)} chck on far: {_tag} {bodies?.Count} {_collider.FindClosestInRangeWithMonaTag(_tag)}", _brain.Body.ActiveTransform.gameObject);
            if (bodies != null && _collider.FindClosestInRangeWithMonaTag(_tag) == null)
            {
                FilterBodiesOnInstruction(bodies);
                if (bodies.Count > 0)
                {
                    var body = bodies[0];
                    //if(_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(OnFarInstructionTile)}.{nameof(Do)} {_distance} found: {body}");
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, body);
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}