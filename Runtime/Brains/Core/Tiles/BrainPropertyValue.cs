using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyValue : BrainProperty
    {
        public Type Type;

        public BrainPropertyValue(bool showOnTile = true)
        {
            Type = null;
            ShowOnTile = showOnTile;
        }

        public BrainPropertyValue(Type type, bool showOnTile = true)
        {
            Type = type;
            ShowOnTile = showOnTile;
        }
    }
}