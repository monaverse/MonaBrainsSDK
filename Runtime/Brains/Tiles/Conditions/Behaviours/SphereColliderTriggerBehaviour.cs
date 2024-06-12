using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Conditions.Structs;
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
    public class SphereColliderTriggerBehaviour : MonoBehaviour, IDisposable
    {
        private SphereCollider _collider;
        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        private string _monaTag;
        private Dictionary<IMonaBody, bool> _bodiesIndex = new Dictionary<IMonaBody, bool>();
        private List<IMonaBody> _bodies = new List<IMonaBody>();
        private List<ForwardBodyStruct> _foundBodiesInFieldOfView = new List<ForwardBodyStruct>();

        private Action<MonaTickEvent> OnTileTick;
        private Action<MonaBodySpawnedEvent> OnBodySpawned;
        private Action<MonaBodyDespawnedEvent> OnBodyDespawned;

        private bool _monitorInside;
        private float _fieldOfView = 180f;
        private bool _localPlayerOnly;
        private float _radius = 1f;

        private void Awake()
        {
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.isTrigger = true;

            OnBodySpawned = HandleBodySpawned;
            MonaEventBus.Register(new EventHook(MonaCoreConstants.MONA_BODY_SPAWNED), OnBodySpawned);

            OnBodyDespawned = HandleBodyDespawned;
            MonaEventBus.Register(new EventHook(MonaCoreConstants.MONA_BODY_DESPAWNED), OnBodyDespawned);
        }

        public void Dispose()
        {
            _collider = null;

            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_SPAWNED), OnBodySpawned);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_DESPAWNED), OnBodyDespawned);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.TICK_EVENT), OnTileTick);
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

        public void MonitorFieldOfView(bool inside)
        {
            OnTileTick = HandleTileTick;
            MonaEventBus.Register<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), OnTileTick);
            _monitorInside = inside;
        }

        public void SetActive(bool active)
        {
            _collider.enabled = active;
            if(!active)
            {
                _bodies.Clear();
                _bodiesIndex.Clear();
            }
        }

        public void SetRadius(float radius)
        {
            _collider.radius = radius + 2f;
            _radius = radius;
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
            if (_monitorInside)
                FindBodiesWithMonaTagInFieldOfView(_monaTag, _fieldOfView);
            else
                FindBodiesWithMonaTagOutsideFieldOfView(_monaTag, _fieldOfView);
        }

        private Color _color = Color.red;
        private void OnDrawGizmosSelected()
        {
            var pos = _brain.Body.GetPosition();
            var left = Quaternion.AngleAxis(-_fieldOfView, Vector3.up) * _brain.Body.ActiveTransform.forward;
            var right = Quaternion.AngleAxis(_fieldOfView, Vector3.up) * _brain.Body.ActiveTransform.forward;

            if (_bodies.Count > 0)
                _color = Color.red;
            else
                _color = Color.green;

            Gizmos.color = _color;
            Gizmos.DrawLine(pos, pos + left * _radius);
            Gizmos.DrawLine(pos, pos + right * _radius);
            Gizmos.DrawSphere(pos + left * _radius, .1f);
            Gizmos.DrawSphere(pos + right * _radius, .1f);

        }

        private List<ForwardBodyStruct> FindBodiesWithMonaTagInFieldOfView(string tag, float fieldOfView = 45f)
        {
            _fieldOfView = fieldOfView;
            _foundBodiesInFieldOfView.Clear();
            var dotValue = -1f + ((1f-Mathf.Abs(fieldOfView / 180f))*2f);
            for(var i = _bodies.Count-1;i >= 0; i--)
            {
                var dir = (_bodies[i].GetPosition() - transform.position);
                var fwd = transform.forward;
                var dot = Vector3.Dot(dir.normalized, fwd.normalized);
                var body = _bodies[i];
                if (!_bodies[i].GetActive())
                {
                    _bodiesIndex.Remove(_bodies[i]);
                    _bodies.RemoveAt(i);
                }
                else
                {
                    //if(_brain.LoggingEnabled) Debug.Log($"{nameof(FindBodiesWithMonaTagInFieldOfView)} {dot} {dotValue} {_brain.Body.Transform} {_radius} {_brain.Body.WithinRadius(_bodies[i], _radius)}");
                    if (dot >= dotValue && _brain.Body.WithinRadius(_bodies[i], _radius))
                    {
                        _foundBodiesInFieldOfView.Add(new ForwardBodyStruct() { dot = dot, body = body });
                        if (!_bodiesIndex[body])
                        {
                            //Debug.Log($"in view {body.Transform.name}");
                            _bodiesIndex[body] = true;
                            MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnFieldOfViewChanged));
                        }
                    }
                    else
                    {
                        if (_bodiesIndex[body])
                        {
                            //Debug.Log($"out of view {body.Transform.name}");
                            _bodiesIndex[body] = false;
                            MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnFieldOfViewChanged));
                        }
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
            for (var i = _bodies.Count-1; i >= 0; i--)
            {
                var dir = (_bodies[i].GetPosition() - transform.position);
                var fwd = transform.forward;
                var dot = Vector3.Dot(dir.normalized, fwd.normalized);
                var body = _bodies[i];
                if (!_bodies[i].GetActive())
                {
                    _bodiesIndex.Remove(_bodies[i]);
                    _bodies.RemoveAt(i);
                }
                else
                {
                    if (dot < dotValue || !_brain.Body.WithinRadius(_bodies[i], _radius))
                    {
                        _foundBodiesInFieldOfView.Add(new ForwardBodyStruct() { dot = dot, body = body });
                        if (_bodiesIndex[body])
                        {
                            _bodiesIndex[body] = false;
                            MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnFieldOfViewChanged));
                        }
                    }
                    else
                    {
                        if (!_bodiesIndex[body])
                        {
                            _bodiesIndex[body] = true;
                            MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnFieldOfViewChanged));
                        }
                    }
                }
            }
            return _foundBodiesInFieldOfView;
        }

        private int SortDot(ForwardBodyStruct a, ForwardBodyStruct b) => -a.dot.CompareTo(b.dot);

        public List<ForwardBodyStruct> FindForwardMostBodyWithMonaTagInFieldOfView(string tag, float fieldOfView = 45f)
        {
            var bodies = FindBodiesWithMonaTagInFieldOfView(tag, fieldOfView);
            if (bodies.Count == 0) return null;
            bodies.Sort(SortDot);
            return bodies;
        }

        public List<ForwardBodyStruct> FindForwardMostBodyWithMonaTagOutsideFieldOfView(string tag, float fieldOfView = 45f)
        {
            var bodies = FindBodiesWithMonaTagOutsideFieldOfView(tag, fieldOfView);
            if (bodies.Count == 0) return null;
            bodies.Sort(SortDot);
            return bodies;
        }

        public IMonaBody FindClosestInRangeWithMonaTag(string tag)
        {
            IMonaBody closest = null;
            float closestDistance = Mathf.Infinity;
            for (var i = _bodies.Count-1; i >= 0; i--)
            {
                var pos = _bodies[i].GetPosition();
                var d = Vector3.Distance(pos, _brain.Body.GetPosition());
                if (!_bodies[i].GetActive())
                {
                    _bodiesIndex.Remove(_bodies[i]);
                    _bodies.RemoveAt(i);
                }
                else
                {
                    //if (_bodies.Contains(_)
                    {
                        if (d < closestDistance && _brain.Body.WithinRadius(_bodies[i], _radius))
                        {
                            closest = _bodies[i];
                            closestDistance = d;
                        }
                    }
                }
            }
            //Debug.Log($"{nameof(FindClosestInRangeWithMonaTag)} {_bodies.Count} {closest == null}");
            return closest;
        }

        private List<IMonaBody> _rangeBodies = new List<IMonaBody>();
        public List<IMonaBody> FindClosestOutOfRangeWithMonaTag(string tag)
        {
            _rangeBodies.Clear();
            var bodies = MonaBody.FindByTag(tag);
            _rangeBodies.AddRange(bodies);
            //Debug.Log($"{nameof(FindClosestOutOfRangeWithMonaTag)} prefilter: {bodies.Count}");
            IMonaBody closest = null;
            float closestDistance = Mathf.Infinity;
            for(var i = _rangeBodies.Count-1;i >= 0;i--)
            {
                var pos = _rangeBodies[i].GetPosition();
                var d = Vector3.Distance(pos, _brain.Body.GetPosition());
                if (_rangeBodies[i] == _brain.Body)
                {
                    _rangeBodies.RemoveAt(i);
                }
                else if (_rangeBodies[i].GetActive())
                {
                    if(_brain.Body.WithinRadius(_rangeBodies[i], _radius))
                    {
                        _rangeBodies.RemoveAt(i);
                    }
                    else if (d < closestDistance)
                    {
                        closest = bodies[i];
                        closestDistance = d;
                    }
                }
            }

            if (closest != null)
            {
                _rangeBodies.Remove(closest);
                _rangeBodies.Insert(0, closest);
            }
            //Debug.Log($"{nameof(FindClosestOutOfRangeWithMonaTag)} {_bodies.Count}");
            return _rangeBodies;
        }

        private void IncludeIfInsideTrigger(IMonaBody body)
        {
            if (_collider.bounds.Contains(body.ActiveTransform.position))
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
                //Debug.Log($"{nameof(OnTriggerEnter)} {_collider.radius} {_radius} {body.ActiveTransform.name} {_bodiesIndex.ContainsKey(body)} {body.WithinRadius(_brain.Body, _collider.radius)}", body.ActiveTransform.gameObject);

                if (!_bodiesIndex.ContainsKey(body))
                {
                    //if(_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(AddBody)} {_collider.radius} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _bodiesIndex.Add(body, false);
                    _bodies.Add(body);
                    MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnTriggerEnter));
                    return true;
                }
            }
            //Debug.Log($"{nameof(AddBody)} {_bodies.Count}");
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

                if (_bodiesIndex.ContainsKey(body) && !_brain.Body.WithinRadius(body, _radius))
                {
                    //if (_brain.LoggingEnabled)
                    //    Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(RemoveBody)} {_radius} {_collider.radius} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    _bodiesIndex.Remove(body);
                    _bodies.Remove(body);
                    MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnTriggerExit));
                    return true;
                }
            }
            return false;
        }

    }
}