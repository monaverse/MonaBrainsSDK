
using Mona.SDK.Core.Body;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Brain.Structs
{
    [Serializable]
    public struct ResetTransform
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Transform Parent;
        public IMonaBody Body;

        public ResetTransform(IMonaBody body)
        {
            Body = body;
            Parent = body.ActiveTransform.parent;
            Position = body.ActiveTransform.position;
            Rotation = body.ActiveTransform.rotation;
        }
    }
}