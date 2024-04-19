
using Mona.SDK.Core.Body;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Brain.Structs
{
    [Serializable]
    public struct ResetTransform
    {
        public Vector3 Position;
        public Vector3 LocalPosition;
        public Quaternion Rotation;
        public Quaternion LocalRotation;
        public Transform Parent;
        public IMonaBody Body;

        public ResetTransform(IMonaBody body)
        {
            Body = body;

            var transform = body.ActiveTransform;
            if (transform == null)
                transform = body.Transform;

            Parent = transform.parent;
            Position = transform.position;
            LocalPosition = transform.localPosition;
            Rotation = transform.rotation;
            LocalRotation = transform.localRotation;
        }
    }
}