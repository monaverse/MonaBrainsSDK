using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
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
    public class OnExitInstructionTile : InstructionTile, ITriggerInstructionTile, IOnExitInstructionTile, 
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IActivateInstructionTile,
        IPauseableInstructionTile, IPlayerTriggeredConditional
    {
        public const string ID = "OnExit";
        public const string NAME = "Exit";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(OnExitInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        public bool PlayerTriggered => _brain.HasPlayerTag() || _brain.MonaTagSource.GetTag(_tag).IsPlayerTag;

        private IMonaBrain _brain;
        private ColliderTriggerBehaviour _collider;
        private GameObject _gameObject;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerExit };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnExitInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            if (_collider == null)
            {
                var colliders = _brain.GameObject.GetComponents<ColliderTriggerBehaviour>();
                var found = false;
                for (var i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].MonaTag == _tag && colliders[i].Brain == _brain)
                    {
                        _collider = colliders[i];
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    _collider = _brain.GameObject.AddComponent<ColliderTriggerBehaviour>();
                    _collider.SetBrain(_brain);
                    _collider.SetMonaTag(_tag);
                    _collider.SetLocalPlayerOnly(PlayerTriggered);
                }
                UpdateActive();
            }

            _brain.Body.AddRigidbody();
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

        public override void Unload()
        {
            if (_collider != null)
            {
                _collider.Dispose();
                GameObject.Destroy(_collider);
                _collider = null;
            }
        }

        public override InstructionTileResult Do()
        {
            if (_collider == null) return InstructionTileResult.Failure;

            var bodies = _collider.BodiesThatLeft;
            var bodyExists = bodies.Count > 0;
            if(bodyExists)
            {
                var body = bodies[0];
                if (body != null)
                {
                    _collider.BodiesThatLeft.Clear();
                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(OnExitInstructionTile)}.{nameof(Do)}");
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, body);
                }
            }

            if (bodyExists)
                return Complete(InstructionTileResult.Success);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}