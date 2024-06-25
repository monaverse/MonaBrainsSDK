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
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Conditions.Behaviours
{
    public class ColliderTriggerBehaviour : MonoBehaviour
    {
        private Collider _collider;
        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        [SerializeField] private string _monaTag;
        private Dictionary<IMonaBody, bool> _bodiesIndex = new Dictionary<IMonaBody, bool>();
        private List<IMonaBody> _bodies = new List<IMonaBody>();
        private List<IMonaBody> _bodiesThatLeft = new List<IMonaBody>();
        private List<IMonaBody> _bodiesThatEntered = new List<IMonaBody>();
        private List<IMonaBody> _bodiesWithin = new List<IMonaBody>();
        
        private Action<MonaTickEvent> OnTileTick;
        private Action<MonaBodySpawnedEvent> OnBodySpawned;
        private Action<MonaBodyDespawnedEvent> OnBodyDespawned;

        private bool _localPlayerOnly;

        public string MonaTag => _monaTag;
        public IMonaBrain Brain => _brain;

        public List<IMonaBody> BodiesWithin => _bodiesWithin;

        public bool ColliderEnabled => _collider != null && _collider.enabled;

        private bool _colliderWasCreatedByMe = false;

        private void Awake()
        {
            _brain = null;

            var colliders = gameObject.GetComponentsInChildren<Collider>(true);
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
                _colliderWasCreatedByMe = true;
                _collider = gameObject.AddComponent<BoxCollider>();
            }

            OnBodySpawned = HandleBodySpawned;
            MonaEventBus.Register(new EventHook(MonaCoreConstants.MONA_BODY_SPAWNED), OnBodySpawned);

            OnBodyDespawned = HandleBodyDespawned;
            MonaEventBus.Register(new EventHook(MonaCoreConstants.MONA_BODY_DESPAWNED), OnBodyDespawned);
        }

        public void Dispose()
        {
            if (_collider != null && _colliderWasCreatedByMe)
                Destroy(_collider);
            _collider = null;

            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_SPAWNED), OnBodySpawned);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_DESPAWNED), OnBodyDespawned);
            
        }

        public void SetBrain(IMonaBrain brain)
        {
            _brain = brain;
            Debug.Log("AA: Brain set, setting collision bounds...");
            SetCollisionBounds();
        }

        private void SetCollisionBounds()
        {
            if (_collider is BoxCollider && _colliderWasCreatedByMe)
            {
                _collider.isTrigger = true;
                var bounds = _brain.Body.GetBounds();
                var col = (BoxCollider)_collider;
                col.center = gameObject.transform.InverseTransformPoint(bounds.center);
                col.size = bounds.size + Vector3.one * .01f;

                Debug.Log("AA: Collision bounds set.");
            }
            else
            {
                Debug.Log("AA: Collision bounds set by exisiting collider.");
            }
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
            if(_collider != null)
                _collider.enabled = active;
            if(!active)
            {
                _bodiesIndex.Clear();
                _bodies.Clear();
            }
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
            if (body != null && _collider != null && body.Intersects(_collider, includeTriggers:true))
                AddBody(body);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collider == null || !_collider.enabled || (_brain != null && _brain.Body == other.GetComponentInParent<IMonaBody>())) return;
            var body = other.GetComponentInParent<IMonaBody>();
            AddBody(body);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_collider == null || !_collider.enabled || (_brain != null && _brain.Body == other.GetComponentInParent<IMonaBody>())) return;
            var body = other.GetComponentInParent<IMonaBody>();
            //Debug.Log($"{nameof(OnTriggerExit)} {body}", body.Transform.gameObject);
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

                if (!_bodiesIndex.ContainsKey(body))
                {
                    //if(_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(ColliderTriggerBehaviour)}.{nameof(AddBody)} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _bodiesIndex.Add(body, true);
                    _bodies.Add(body);
                    if(!_bodiesThatEntered.Contains(body))
                        _bodiesThatEntered.Add(body);
                    if (!_bodiesWithin.Contains(body))
                        _bodiesWithin.Add(body);
                    MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnTriggerEnter));
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

                if (_bodiesIndex.ContainsKey(body))// && !_brain.Body.WithinRadius(body, _radius, includeTriggers: true))
                {
                    //if (_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(ColliderTriggerBehaviour)}.{nameof(RemoveBody)} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _bodiesIndex.Remove(body);
                    _bodies.Remove(body);
                    if(!_bodiesThatLeft.Contains(body))
                        _bodiesThatLeft.Add(body);
                    if (_bodiesWithin.Contains(body))
                        _bodiesWithin.Remove(body);
                    MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnTriggerExit));
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

        public List<IMonaBody> BodiesThatEntered => _bodiesThatEntered;
        public bool ClearBodyThatEntered(IMonaBody body)
        {
            if (_bodiesThatEntered.Contains(body))
            {
                _bodiesThatEntered.Remove(body);
                return true;
            }
            return false;
        }
    }
}