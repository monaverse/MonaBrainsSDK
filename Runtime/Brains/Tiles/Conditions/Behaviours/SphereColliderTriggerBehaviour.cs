using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Behaviours
{
    public class SphereColliderTriggerBehaviour : MonoBehaviour, IDisposable
    {
        private struct ForwardBodyStruct
        {
            public float dot;
            public IMonaBody body;
        }

        private SphereCollider _collider;
        private IMonaBrain _brain;
        private string _monaTag;
        private Dictionary<IMonaBody, bool> _bodiesIndex = new Dictionary<IMonaBody, bool>();
        private List<IMonaBody> _bodies = new List<IMonaBody>();
        private List<ForwardBodyStruct> _foundBodiesInFieldOfView = new List<ForwardBodyStruct>();

        private Action<MonaTileTickEvent> OnTileTick;
        private Action<MonaBodySpawnedEvent> OnBodySpawned;
        private Action<MonaBodyDespawnedEvent> OnBodyDespawned;

        private bool _monitorInside;
        private float _fieldOfView = 180f;

        private void Awake()
        {
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.isTrigger = true;

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
            EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTileTick);
        }

        public void SetBrain(IMonaBrain brain)
        {
            _brain = brain;
        }

        public void SetMonaTag(string monaTag)
        {
            _monaTag = monaTag;
        }

        public void MonitorFieldOfView(bool inside)
        {
            OnTileTick = HandleTileTick;
            EventBus.Register<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTileTick);
            _monitorInside = inside;
        }

        public void SetRadius(float radius)
        {
            _collider.radius = radius;
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

        private void HandleTileTick(MonaTileTickEvent evt)
        {
            if (_monitorInside)
                FindBodiesWithMonaTagInFieldOfView(_monaTag, _fieldOfView);
            else
                FindBodiesWithMonaTagOutsideFieldOfView(_monaTag, _fieldOfView);
        }

        private List<ForwardBodyStruct> FindBodiesWithMonaTagInFieldOfView(string tag, float fieldOfView = 45f)
        {
            _fieldOfView = fieldOfView;
            _foundBodiesInFieldOfView.Clear();
            var dotValue = -1f + ((1f-Mathf.Abs(fieldOfView / 180f))*2f);
            for(var i = 0;i < _bodies.Count; i++)
            {
                var dir = (_bodies[i].GetPosition() - transform.position);
                var fwd = transform.forward;
                var dot = Vector3.Dot(dir.normalized, fwd.normalized);
                var body = _bodies[i];
                if (dot >= dotValue)
                {
                    _foundBodiesInFieldOfView.Add(new ForwardBodyStruct() { dot = dot, body = body });
                    if (!_bodiesIndex[body])
                    {
                        //Debug.Log($"in view {body.Transform.name}");
                        _bodiesIndex[body] = true;
                        EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnFieldOfViewChanged));
                    }
                }
                else
                {
                    if (_bodiesIndex[body])
                    {
                        //Debug.Log($"out of view {body.Transform.name}");
                        _bodiesIndex[body] = false;
                        EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnFieldOfViewChanged));
                    }
                }
            }
            return _foundBodiesInFieldOfView;
        }

        private List<ForwardBodyStruct> FindBodiesWithMonaTagOutsideFieldOfView(string tag, float fieldOfView = 45f)
        {
            _fieldOfView = fieldOfView;
            _foundBodiesInFieldOfView.Clear();
            var dotValue = -1f + ((1f - Mathf.Abs(fieldOfView / 180f)) * 2f);
            for (var i = 0; i < _bodies.Count; i++)
            {
                var dir = (_bodies[i].GetPosition() - transform.position);
                var fwd = transform.forward;
                var dot = Vector3.Dot(dir.normalized, fwd.normalized);
                var body = _bodies[i];
                if (dot < dotValue)
                {
                    _foundBodiesInFieldOfView.Add(new ForwardBodyStruct() { dot = dot, body = body });
                    if (_bodiesIndex[body])
                    {
                        _bodiesIndex[body] = false;
                        EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnFieldOfViewChanged));
                    }
                }
                else
                {
                    if (!_bodiesIndex[body])
                    {
                        _bodiesIndex[body] = true;
                        EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnFieldOfViewChanged));
                    }
                }
            }
            return _foundBodiesInFieldOfView;
        }

        private int SortDot(ForwardBodyStruct a, ForwardBodyStruct b) => -a.dot.CompareTo(b.dot);

        public IMonaBody FindForwardMostBodyWithMonaTagInFieldOfView(string tag, float fieldOfView = 45f)
        {
            var bodies = FindBodiesWithMonaTagInFieldOfView(tag, fieldOfView);
            if (bodies.Count == 0) return null;
            bodies.Sort(SortDot);
            return bodies[0].body;
        }

        public IMonaBody FindForwardMostBodyWithMonaTagOutsideFieldOfView(string tag, float fieldOfView = 45f)
        {
            var bodies = FindBodiesWithMonaTagOutsideFieldOfView(tag, fieldOfView);
            if (bodies.Count == 0) return null;
            bodies.Sort(SortDot);
            return bodies[0].body;
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
                if (d < _collider.radius)
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

        public IMonaBody FindClosestOutOfRangeWithMonaTag(string tag)
        {
            var bodies = MonaBody.FindByTag(tag);
            IMonaBody closest = null;
            float closestDistance = Mathf.Infinity;
            for(var i = 0;i < bodies.Count;i++)
            {
                var pos = bodies[i].GetPosition();
                var d = Vector3.Distance(pos, _brain.Body.GetPosition());
                if (d > _collider.radius)
                {
                    if(d < closestDistance)
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
            if (_collider.bounds.Contains(body.ActiveTransform.position))
                AddBody(body);
        }

        private void OnTriggerEnter(Collider other)
        {
            var body = other.GetComponentInParent<IMonaBody>();
            AddBody(body);
        }

        private void OnTriggerExit(Collider other)
        {
            var body = other.GetComponentInParent<IMonaBody>();
            RemoveBody(body);
        }

        private bool AddBody(IMonaBody body)
        {
            if (body != null && body.HasMonaTag(_monaTag))
            {
                //Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(AddBody)} {body.LocalId}");
                if (!_bodiesIndex.ContainsKey(body))
                {
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
                //Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(RemoveBody)} {body.LocalId}");
                if (_bodiesIndex.ContainsKey(body))
                {
                    _bodiesIndex.Remove(body);
                    _bodies.Remove(body);
                    EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnTriggerExit));
                    return true;
                }
            }
            return false;
        }

    }
}