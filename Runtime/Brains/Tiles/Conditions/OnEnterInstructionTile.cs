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
    public class OnEnterInstructionTile : InstructionTile, ITriggerInstructionTile, IOnEnterInstructionTile,
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IActivateInstructionTile,
        IPauseableInstructionTile, IPlayerTriggeredConditional
    {
        public const string ID = "OnEnter";
        public const string NAME = "Enter";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(OnEnterInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        public bool PlayerTriggered => _brain.HasPlayerTag() || _brain.MonaTagSource.GetTag(_tag).IsPlayerTag;

        private IMonaBrain _brain;
        private ColliderTriggerBehaviour _collider;
        private GameObject _gameObject;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnEnterInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page)
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
                    _collider.SetPage(page);
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
                Debug.Log($"{nameof(OnEnterInstructionTile)}.{nameof(UpdateActive)} {_active}");
            if (_collider != null)
                _collider.SetActive(_active);
        }

        public override void Unload(bool destroy = false)
        {
            if (_collider != null)
            {
                if (destroy)
                {
                    _collider.Dispose();
                    _collider = null;
                }
            }
        }

        public override InstructionTileResult Do()
        {
            if (_collider == null) return InstructionTileResult.Failure;
            //Debug.Log($"{nameof(OnEnterInstructionTile)}.{nameof(Do)} found: {_tag} {_collider.BodiesThatEntered.Count}", _brain.Body.ActiveTransform.gameObject);
            var bodies = _collider.BodiesThatEntered;
            if (bodies.Count > 0)
            {
                var body = bodies[0];
                if (body != null)
                {
                    _collider.BodiesThatEntered.Clear();
                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(OnEnterInstructionTile)}.{nameof(Do)} found: {_tag} {body}", _brain.Body.ActiveTransform.gameObject);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, body);
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}