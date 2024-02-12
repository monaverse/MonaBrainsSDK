using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyMonaAsset : BrainProperty
    {
        public Type Type;

        public BrainPropertyMonaAsset(Type type)
        {
            Type = type;
        }
    }
}