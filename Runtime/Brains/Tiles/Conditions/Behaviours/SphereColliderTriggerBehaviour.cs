using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.Body;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Behaviours
{
    public class SphereColliderTriggerBehaviour : MonoBehaviour
    {
        private struct ForwardBodyStruct
        {
            public float dot;
            public IMonaBody body;
        }

        private SphereCollider _collider;
        private IMonaBrain _brain;
        private string _monaTag;
        private List<IMonaBody> _bodies = new List<IMonaBody>();
        private List<ForwardBodyStruct> _foundBodiesInFieldOfView = new List<ForwardBodyStruct>();

        private void Awake()
        {
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.isTrigger = true;
        }

        private void OnDestroy()
        {
            if (_collider != null)
                Destroy(_collider);
            _collider = null;
        }

        public void SetBrain(IMonaBrain brain)
        {
            _brain = brain;
        }

        public void SetMonaTag(string monaTag)
        {
            _monaTag = monaTag;
        }

        public void SetRadius(float radius)
        {
            _collider.radius = radius;
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

        private List<ForwardBodyStruct> FindBodiesWithMonaTagInFieldOfView(string tag, float fieldOfView = 45f)
        {
            _foundBodiesInFieldOfView.Clear();
            var dotValue = -1f + ((1f-Mathf.Abs(fieldOfView / 180f))*2f);
            for(var i = 0;i < _bodies.Count; i++)
            {
                var dir = (_bodies[i].GetPosition() - transform.position);
                    dir.y = 0;
                var fwd = transform.forward;
                    fwd.y = 0;
                var dot = Vector3.Dot(dir.normalized, fwd.normalized);
                if (dot > dotValue)
                    _foundBodiesInFieldOfView.Add(new ForwardBodyStruct() { dot = dot, body = _bodies[i] });
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

        public IMonaBody FindClosestInRangeWithMonaTag(string tag)
        {
            return _bodies.Find(x => x.HasMonaTag(tag));
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

        private void OnTriggerEnter(Collider other)
        {
            var body = other.GetComponentInParent<IMonaBody>();
            if(body != null && body.HasMonaTag(_monaTag))
            {
                if(AddBody(body))
                    EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnTriggerEnter));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var body = other.GetComponentInParent<IMonaBody>();
            if(body != null && body.HasMonaTag(_monaTag))
            {
                if(RemoveBody(body))
                    EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnTriggerExit));
            }
        }

        private bool AddBody(IMonaBody body)
        {
            //Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(AddBody)} {body.LocalId}");
            if (!_bodies.Contains(body))
            {
                _bodies.Add(body);
                return true;
            }
            return false;
        }

        private bool RemoveBody(IMonaBody body)
        {
            //Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(RemoveBody)} {body.LocalId}");
            if (_bodies.Contains(body))
            {
                _bodies.Remove(body);
                return true;
            }
            return false;
        }

    }
}