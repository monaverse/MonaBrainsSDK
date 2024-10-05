using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Body.Enums;
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
    public struct TagCollision
    {
        public Collision Collision;
        public Collider Collider;
        public IMonaBody Body;
        public int Frame;
        public Vector3 Position;

        public TagCollision(IMonaBody body, Collision collision, Vector3 pos, int frame)
        {
            Body = body;
            Collider = null;
            Collision = collision;
            Frame = frame;
            Position = pos;
        }

        public TagCollision(Collider collider, Collision collision, Vector3 pos, int frame)
        {
            Body = null;
            Collider = collider;
            Collision = collision;
            Frame = frame;
            Position = pos;
        }

        public bool ShouldClear() => Time.frameCount - Frame > 0;
    }

    public class ColliderHitBehaviour : MonoBehaviour
    {
        private Collider _collider;
        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        private string _monaTag;
        private List<TagCollision> _bodiesThatHit = new List<TagCollision>();
        private List<TagCollision> _collidersThatHit = new List<TagCollision>();

        private List<IMonaBody> _bodiesThatStayed = new List<IMonaBody>();
        private List<Collider> _collidersThatStayed = new List<Collider>();

        private List<IMonaBody> _bodiesThatLeft = new List<IMonaBody>();
        private List<Collider> _collidersThatLeft = new List<Collider>();

        private Action<MonaLateTickEvent> OnLateTick;

        public string MonaTag => _monaTag;

        private bool _colliderWasCreatedByMe = false;

        private void Awake()
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            var found = false;
            for(var i = 0;i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if(!collider.isTrigger)
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

            OnLateTick = HandleLateTick;
            MonaEventBus.Register<MonaLateTickEvent>(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), OnLateTick);
        }

        public void Dispose(bool destroy)
        {
            if (_collider != null && !_colliderWasCreatedByMe)
                Destroy(_collider);
            _collider = null;

            if(destroy)
                MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_SCALE_CHANGED_EVENT, _brain.Body), OnBodyScaleChanged);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), OnLateTick);
        }

        private Action<MonaBodyScaleChangedEvent> OnBodyScaleChanged;
        public void SetBrain(IMonaBrain brain)
        {
            _brain = brain;
            if (_colliderWasCreatedByMe)
            {
                OnBodyScaleChanged = HandleBodyScaleChanged;
                MonaEventBus.Register<MonaBodyScaleChangedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_SCALE_CHANGED_EVENT, _brain.Body), OnBodyScaleChanged);
                HandleBodyScaleChanged(new MonaBodyScaleChangedEvent(_brain.Body));
            }
        }

        private void HandleBodyScaleChanged(MonaBodyScaleChangedEvent evt)
        {
            Debug.Log($"{nameof(HandleBodyScaleChanged)}", evt.Body.Transform.gameObject);
            if (_collider is BoxCollider && _colliderWasCreatedByMe)
            {
                var bounds = evt.Body.GetBounds();
                var coll = (BoxCollider)_collider;
                Debug.Log($"{nameof(HandleBodyScaleChanged)} {bounds.center} {bounds.size}");
                coll.center = transform.InverseTransformPoint(bounds.center);
                coll.size = Vector3.Scale(bounds.size, new Vector3(1f/evt.Body.Transform.lossyScale.x, 1f / evt.Body.Transform.lossyScale.y, 1f / evt.Body.Transform.lossyScale.z));
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
            _collider.enabled = active;
        }

        private Vector3 _lastPosition;
        private void OnCollisionEnter(Collision collision)
        {
            if (_collider == null || !_collider.enabled) return;
            var body = collision.collider.GetComponentInParent<IMonaBody>();
            if (body != null && body.HasMonaTag(_monaTag))
            {
                var found = false;
                for (var i = 0; i < _bodiesThatHit.Count; i++)
                {
                    if (_bodiesThatHit[i].Body == body)
                    {
                        found = true;
                        break;
                    }
                }

                if (!_bodiesThatStayed.Contains(body))
                    _bodiesThatStayed.Add(body);

                if (!found)
                {
                    if (_brain.Body.ActiveRigidbody != null)
                        _bodiesThatHit.Add(new TagCollision(body, collision, _brain.Body.ActiveRigidbody.position, Time.frameCount));
                    else
                        _bodiesThatHit.Add(new TagCollision(body, collision, _brain.Body.ActiveTransform.position, Time.frameCount));

                    if (_brain.Body.ActiveRigidbody == null || _brain.Body.ActiveRigidbody.isKinematic)
                    {
                        var dir = _brain.Body.ActiveRigidbody.position - _lastPosition;
                        RaycastHit hitInfo;
                        if (UnityEngine.Physics.Raycast(_lastPosition, dir, out hitInfo, 1f))
                        {
                            if (hitInfo.collider != null && hitInfo.collider != _collider)
                            {
                                //Debug.Log($"passthrough {hitInfo.collider} {_collider.bounds.extents} {dir}");
                                _lastPosition = hitInfo.point - Vector3.Scale(dir.normalized, _collider.bounds.extents);
                            }
                        }
                       // Debug.Log($"Added collision {_lastPosition} {_brain.Body.ActiveRigidbody.position} {_brain.Body.ActiveTransform.position}", _brain.Body.Transform.gameObject);
                        MonaEventBus.Trigger(new EventHook(MonaCoreConstants.MONA_BODY_EVENT, _brain.Body), new MonaBodyEvent(MonaBodyEventType.OnStop));
                        _brain.Body.TeleportPosition(_lastPosition, true);
                    }

                    //Debug.Log($"Added collision {body.ActiveTransform.name} {_monaTag} {Time.frameCount}");
                    MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnCollisionEnter));
                    //_bodiesThatHit.Clear();
                }
            }
            else if (collision.collider.tag == _monaTag)
            {
                Debug.Log($"{nameof(ColliderHitBehaviour)} {_monaTag} {collision.collider}", collision.collider.gameObject);
                var found = false;
                for (var i = 0; i < _collidersThatHit.Count; i++)
                {
                    if (_collidersThatHit[i].Collider == collision.collider)
                    {
                        found = true;
                        break;
                    }
                }

                if (!_collidersThatStayed.Contains(collision.collider))
                    _collidersThatStayed.Add(collision.collider);

                if (!found)
                {
                    if (_brain.Body.ActiveRigidbody != null)
                        _collidersThatHit.Add(new TagCollision(collision.collider, collision, _brain.Body.ActiveRigidbody.position, Time.frameCount));
                    else
                        _collidersThatHit.Add(new TagCollision(collision.collider, collision, _brain.Body.ActiveTransform.position, Time.frameCount));

                    if (_brain.Body.ActiveRigidbody == null || _brain.Body.ActiveRigidbody.isKinematic)
                    {
                        var dir = _brain.Body.ActiveRigidbody.position - _lastPosition;
                        RaycastHit hitInfo;
                        if (UnityEngine.Physics.Raycast(_lastPosition, dir, out hitInfo, 1f))
                        {
                            if (hitInfo.collider != null && hitInfo.collider != _collider)
                            {
                                //Debug.Log($"passthrough {hitInfo.collider} {_collider.bounds.extents} {dir}");
                                _lastPosition = hitInfo.point - Vector3.Scale(dir.normalized, _collider.bounds.extents);
                            }
                        }
                        // Debug.Log($"Added collision {_lastPosition} {_brain.Body.ActiveRigidbody.position} {_brain.Body.ActiveTransform.position}", _brain.Body.Transform.gameObject);
                        MonaEventBus.Trigger(new EventHook(MonaCoreConstants.MONA_BODY_EVENT, _brain.Body), new MonaBodyEvent(MonaBodyEventType.OnStop));
                        _brain.Body.TeleportPosition(_lastPosition, true);
                    }

                    //Debug.Log($"Added collision {body.ActiveTransform.name} {_monaTag} {Time.frameCount}");
                    MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnCollisionEnter));
                    //_bodiesThatHit.Clear();
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (_collider == null || !_collider.enabled) return;
            var body = collision.collider.GetComponentInParent<IMonaBody>();

            if (body != null && body.HasMonaTag(_monaTag))
            {
                if (!_bodiesThatLeft.Contains(body))
                    _bodiesThatLeft.Add(body);

                if (_bodiesThatStayed.Contains(body))
                    _bodiesThatStayed.Remove(body);

                MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnCollisionExit));
            }
            else if(body == null && collision.collider.CompareTag(_monaTag))
            {
                if (!_collidersThatLeft.Contains(collision.collider))
                    _collidersThatLeft.Add(collision.collider);

                if (_collidersThatStayed.Contains(collision.collider))
                    _collidersThatStayed.Remove(collision.collider);

                MonaEventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new InstructionEvent(MonaTriggerType.OnCollisionExit));
            }
        }

        private void HandleLateTick(MonaLateTickEvent evt)
        {
           if (_brain.Body.ActiveRigidbody != null)
                _lastPosition = _brain.Body.ActiveRigidbody.position;
           else
                _lastPosition = _brain.Body.ActiveTransform.position;

            for (var i = _bodiesThatHit.Count - 1; i >= 0; i--)
            {
                if (_bodiesThatHit[i].ShouldClear())
                {
                    //Debug.Log($"remove hit collision {_bodiesThatHit[i].Frame} {_monaTag} {Time.frameCount} {Time.frameCount - _bodiesThatHit[i].Frame} {_bodiesThatHit[i].ShouldClear()} {_bodiesThatHit[i].Body.ActiveTransform.name}");
                    _bodiesThatHit.RemoveAt(i);
                }
            }

            for (var i = _collidersThatHit.Count - 1; i >= 0; i--)
            {
                if (_collidersThatHit[i].ShouldClear())
                {
                    //Debug.Log($"remove hit collision {_bodiesThatHit[i].Frame} {_monaTag} {Time.frameCount} {Time.frameCount - _bodiesThatHit[i].Frame} {_bodiesThatHit[i].ShouldClear()} {_bodiesThatHit[i].Body.ActiveTransform.name}");
                    _collidersThatHit.RemoveAt(i);
                }
            }
        }

        public List<TagCollision> BodiesThatHit => _bodiesThatHit;
        public List<IMonaBody> BodiesThatStayed => _bodiesThatStayed;
        public List<IMonaBody> BodiesThatLeft => _bodiesThatLeft;

        public List<TagCollision> CollidershatHit => _collidersThatHit;
        public List<Collider> CollidersThatStayed => _collidersThatStayed;
        public List<Collider> CollidersThatLeft => _collidersThatLeft;

    }
}