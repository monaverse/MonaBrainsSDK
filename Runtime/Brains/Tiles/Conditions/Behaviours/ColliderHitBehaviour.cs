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
    public struct TagCollision
    {
        public Collision Collision;
        public IMonaBody Body;
        public int Frame;
        public Vector3 Position;
        public TagCollision(IMonaBody body, Collision collision, Vector3 pos, int frame)
        {
            Body = body;
            Collision = collision;
            Frame = frame;
            Position = pos;
        }

        public bool ShouldClear() => Time.frameCount - Frame > 0;
    }

    public class ColliderHitBehaviour : MonoBehaviour, IDisposable
    {
        private Collider _collider;
        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        private string _monaTag;
        private List<TagCollision> _bodiesThatHit = new List<TagCollision>();
        private Action<MonaLateTickEvent> OnLateTick;

        public string MonaTag => _monaTag;

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
                _collider = gameObject.AddComponent<BoxCollider>();
            }

            OnLateTick = HandleLateTick;
            EventBus.Register<MonaLateTickEvent>(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), OnLateTick);
        }

        public void Dispose()
        {
            if (_collider != null)
                Destroy(_collider);
            _collider = null;

            EventBus.Unregister(new EventHook(MonaCoreConstants.LATE_TICK_EVENT), OnLateTick);
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
                if (!found)
                {
                    if (body.ActiveRigidbody != null)
                        _bodiesThatHit.Add(new TagCollision(body, collision, _brain.Body.ActiveRigidbody.position, Time.frameCount));
                    else
                        _bodiesThatHit.Add(new TagCollision(body, collision, _brain.Body.ActiveTransform.position, Time.frameCount));
                    //Debug.Log($"Added collision {body.ActiveTransform.name} {_monaTag} {Time.frameCount}");
                    EventBus.Trigger<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, _brain), new MonaTriggerEvent(MonaTriggerType.OnCollisionEnter));
                    //_bodiesThatHit.Clear();
                }
            }
        }

        private void HandleLateTick(MonaLateTickEvent evt)
        {
            for (var i = _bodiesThatHit.Count - 1; i >= 0; i--)
            {
                if (_bodiesThatHit[i].ShouldClear())
                {
                    //Debug.Log($"remove hit collision {_bodiesThatHit[i].Frame} {_monaTag} {Time.frameCount} {Time.frameCount - _bodiesThatHit[i].Frame} {_bodiesThatHit[i].ShouldClear()} {_bodiesThatHit[i].Body.ActiveTransform.name}");
                    _bodiesThatHit.RemoveAt(i);
                }
            }
        }

        private void OnCollisionExit(Collision other)
        {
        }

        public List<TagCollision> BodiesThatHit => _bodiesThatHit;
        
    }
}