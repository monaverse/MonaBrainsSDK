using UnityEngine;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Core.Utils.Structs
{
    public struct LayoutStorageData
    {
        public string BaseKey;
        public IMonaBody ReferenceBody;
        public LayoutVector Position;
        public LayoutVector RotationEulers;
        public LayoutVector Scale;

        public void SetPosition(Vector3 vector)
        {
            Position.Vector = vector;
            Position.LoadSuccess = true;
        }

        public void SetRotationEulers(Vector3 vector)
        {
            RotationEulers.Vector = vector;
            RotationEulers.LoadSuccess = true;
        }

        public void SetScale(Vector3 vector)
        {
            Scale.Vector = vector;
            Scale.LoadSuccess = true;
        }
    }

    public struct LayoutVector
    {
        public bool LoadSuccess;
        public Vector3 Vector;
    }
}