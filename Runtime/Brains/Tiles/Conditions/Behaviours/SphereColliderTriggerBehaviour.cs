using Mona.Core.Body;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions.Behaviours
{
    public class SphereColliderTriggerBehaviour : MonoBehaviour
    {
        private struct ForwardBodyStruct
        {
            public float dot;
            public IMonaBody body;
        }

        private SphereCollider _collider;
        private List<IMonaBody> _bodies = new List<IMonaBody>();
        private List<IMonaBody> _foundBodies = new List<IMonaBody>();
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

        public List<IMonaBody> FindBodiesWithMonaTag(string tag)
        {
            _foundBodies.Clear();
            _foundBodies = new List<IMonaBody>();
            for (var i = 0; i < _bodies.Count; i++)
            {
                if (_bodies[i].HasMonaTag(tag))
                    _foundBodies.Add(_bodies[i]);
            }
            return _foundBodies;
        }

        private List<ForwardBodyStruct> FindBodiesWithMonaTagInFieldOfView(string tag, float fieldOfView = 45f)
        {
            _foundBodiesInFieldOfView.Clear();
            _foundBodies = FindBodiesWithMonaTag(tag);
            var dotValue = -1f + ((1f-Mathf.Abs(fieldOfView / 180f))*2f);
            for(var i = 0;i < _foundBodies.Count; i++)
            {
                var dir = (_foundBodies[i].GetPosition() - transform.position);
                    dir.y = 0;
                var fwd = transform.forward;
                    fwd.y = 0;
                var dot = Vector3.Dot(dir.normalized, fwd.normalized);
                if (dot > dotValue)
                    _foundBodiesInFieldOfView.Add(new ForwardBodyStruct() { dot = dot, body = _foundBodies[i] });
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

        private void OnTriggerEnter(Collider other)
        {
            var body = other.GetComponentInParent<IMonaBody>();
            if(body != null)
            {
                AddBody(body);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var body = other.GetComponentInParent<IMonaBody>();
            if(body != null)
            {
                RemoveBody(body);
            }
        }

        private void AddBody(IMonaBody body)
        {
            Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(AddBody)} {body.LocalId}");
            if (!_bodies.Contains(body))
                _bodies.Add(body);
        }

        private void RemoveBody(IMonaBody body)
        {
            Debug.Log($"{nameof(SphereColliderTriggerBehaviour)}.{nameof(RemoveBody)} {body.LocalId}");
            if (_bodies.Contains(body))
                _bodies.Remove(body);
        }

    }
}