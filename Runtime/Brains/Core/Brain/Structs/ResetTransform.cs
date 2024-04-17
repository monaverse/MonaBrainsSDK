
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
            Parent = body.ActiveTransform.parent;
            Position = body.ActiveTransform.position;
            LocalPosition = body.ActiveTransform.localPosition;
            Rotation = body.ActiveTransform.rotation;
            LocalRotation = body.ActiveTransform.localRotation;
        }
    }
}