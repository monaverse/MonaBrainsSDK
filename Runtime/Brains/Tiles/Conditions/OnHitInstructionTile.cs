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
    public class OnHitInstructionTile : InstructionTile, ITriggerInstructionTile, IOnHitInstructionTile,
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, IActivateInstructionTile,
        IPauseableInstructionTile, IPlayerTriggeredConditional
    {
        public const string ID = "OnHit";
        public const string NAME = "Hit";
        public const string CATEGORY = "Proximity";
        public override Type TileType => typeof(OnHitInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        public bool PlayerTriggered => _brain.HasPlayerTag() || _brain.MonaTagSource.GetTag(_tag).IsPlayerTag;

        private IMonaBrain _brain;
        private ColliderHitBehaviour _collider;
        private GameObject _gameObject;
        private bool _active;

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnCollisionEnter };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnHitInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page)
        {
            _brain = brainInstance;
            if (_collider == null)
            {
                _collider = _brain.GameObject.AddComponent<ColliderHitBehaviour>();
                _collider.SetBrain(_brain);
                _collider.SetPage(page);
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
                Debug.Log($"{nameof(OnHitInstructionTile)}.{nameof(UpdateActive)} {_active}");
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

            var bodies = _collider.BodiesThatHit;
            if (bodies.Count > 0)
            {
                var tagCollision = bodies[0];
                var body = tagCollision.Body;
                if (body != null)
                {
                    _collider.BodiesThatHit.Clear();
                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(OnHitInstructionTile)}.{nameof(Do)} found: {_tag} {body}", _brain.Body.ActiveTransform.gameObject);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, body);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_POINT, (tagCollision.Collision.contactCount > 0) ? tagCollision.Collision.contacts[0].point : Vector3.zero);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_NORMAL, (tagCollision.Collision.contactCount > 0) ? tagCollision.Collision.contacts[0].normal : Vector3.zero);
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}