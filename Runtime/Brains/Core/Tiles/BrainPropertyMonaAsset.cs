using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyMonaAsset : BrainProperty
    {
        public Type Type;
        public bool UseProviders;

        public BrainPropertyMonaAsset(Type type, bool showOnTile = true, bool useProviders = false)
        {
            Type = type;
            ShowOnTile = showOnTile;
            UseProviders = useProviders;
        }
    }
}