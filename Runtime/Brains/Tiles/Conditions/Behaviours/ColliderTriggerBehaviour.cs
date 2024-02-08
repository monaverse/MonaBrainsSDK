using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network.Enums;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Behaviours
{
    public class ColliderTriggerBehaviour : MonoBehaviour, IDisposable
    {
        private Collider _collider;
        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        private string _monaTag;
        private Dictionary<IMonaBody, bool> _bodiesIndex = new Dictionary<IMonaBody, bool>();
        private List<IMonaBody> _bodies = new List<IMonaBody>();
        private List<IMonaBody> _bodiesThatLeft = new List<IMonaBody>();
        
        private Action<MonaTickEvent> OnTileTick;
        private Action<MonaBodySpawnedEvent> OnBodySpawned;
        private Action<MonaBodyDespawnedEvent> OnBodyDespawned;

        private bool _localPlayerOnly;

        private void Awake()
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            var found = false;
            for(var i = 0;i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if(collider.isTrigger)
                {
                    found = true;
                    _collider = collider;
                    break;
                }
            }

            if (!found)
            {
                _collider = gameObject.AddComponent<BoxCollider>();
                _collider.isTrigger = true;
            }

            OnBodySpawned = HandleBodySpawned;
            EventBus.Register(new EventHook(MonaCoreConstants.MONA_BODY_SPAWNED), OnBodySpawned);

            OnBodyDespawned = HandleBodyDespawned;
            EventBus.Register(new EventHook(MonaCoreConstants.MONA_BODY_DESPAWNED), OnBodyDespawned);
        }

        public void Dispose()
        {
            if (_collider != null)
                Destroy(_collider);
            _collider = null;

            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_SPAWNED), OnBodySpawned);
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_DESPAWNED), OnBodyDespawned);
            EventBus.Unregister(new EventHook(MonaCoreConstants.TICK_EVENT), OnTileTick);
        }

        public void SetBrain(IMonaBrain brain)
        {
            _brain = brain;
        }

        public void SetPage(IMonaBrainPage page)
        {
            _page = page;
        }

        public void SetMonaTag(string monaTag)
        {
            _monaTag = monaTag;
        }

        public void SetActive(bool active)
        {
            _collider.enabled = active;
        }

        public void SetLocalPlayerOnly(bool b)
        {
            _localPlayerOnly = b;
        }

        private void HandleBodySpawned(MonaBodySpawnedEvent evt)
        {
            IncludeIfInsideTrigger(evt.Body);
        }

        private void HandleBodyDespawned(MonaBodyDespawnedEvent evt)
        {
            RemoveBody(evt.Body);
        }

        public IMonaBody FindBodyWithMonaTag(string tag)
        {
            for(var i = 0;i <_bodies.Count; i++)
            {
                if (_bodies[i].HasMonaTag(tag))
                    return _bodies[i];
            }
            return null;
        }

        private void HandleTileTick(MonaTickEvent evt)
        {
        
        }

        public IMonaBody FindClosestInRangeWithMonaTag(string tag)
        {
            var bodies = _bodies;
            IMonaBody closest = null;
            float closestDistance = Mathf.Infinity;
            for (var i = 0; i < bodies.Count; i++)
            {
                var pos = bodies[i].GetPosition();
                var d = Vector3.Distance(pos, _brain.Body.GetPosition());
                if (bodies[i].Intersects(_collider))
                {
                    if (d < closestDistance)
                    {
                        closest = bodies[i];
                        closestDistance = d;
                    }
                }
            }
            return closest;
        }

        private void IncludeIfInsideTrigger(IMonaBody body)
        {
            if (body.Intersects(_collider))
                AddBody(body);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collider == null || !_collider.enabled || other.isTrigger) return;
            var body = other.GetComponentInParent<IMonaBody>();
            AddBody(body);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_collider == null || !_collider.enabled || other.isTrigger) return;
            var body = other.GetComponentInParent<IMonaBody>();
            RemoveBody(body);
        }

        public bool Intersects(IMonaBody body)
        {
            return body.Intersects(_collider);
        }

        private bool AddBody(IMonaBody body)
        {
            if (body != null && body.HasMonaTag(_monaTag))
            {
                if(_brain.Player.NetworkSettings.GetNetworkType() == MonaNetworkType.Shared)
                {
                    //in shared mode only pay attention to your own player body
                    if (body.HasMonaTag(MonaBrainConstants.TAG_REMOTE_PLAYER))
                        return false;
                }

                if (!_bodiesIndex.ContainsKey(body) && body.Intersects(_collider))
                {
                    if(_brain.LoggingEnabled)
                        Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(AddBody)} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _bodiesIndex.Add(body, true);
                    _bodies.Add(body);
                    EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnTriggerEnter));
                    return true;
                }
            }
            return false;
        }

        private bool RemoveBody(IMonaBody body)
        {
            if (body != null && body.HasMonaTag(_monaTag))
            {
                if (_brain.Player.NetworkSettings.GetNetworkType() == MonaNetworkType.Shared)
                {
                    //in shared mode only pay attention to your own player body
                    if (body.HasMonaTag(MonaBrainConstants.TAG_REMOTE_PLAYER))
                        return false;
                }

                if (_bodiesIndex.ContainsKey(body) && !body.Intersects(_collider))
                {
                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(RemoveBody)} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _bodiesIndex.Remove(body);
                    _bodies.Remove(body);
                    if(!_bodiesThatLeft.Contains(body))
                        _bodiesThatLeft.Add(body);
                    EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnTriggerExit));
                    return true;
                }
            }
            return false;
        }

        public List<IMonaBody> BodiesThatLeft => _bodiesThatLeft;
        public bool ClearBodyThatLeft(IMonaBody body)
        {
            if(_bodiesThatLeft.Contains(body))
            {
                _bodiesThatLeft.Remove(body);
                return true;
            }
            return false;
        }

    }
}