using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyValue : BrainProperty
    {
        public Type Type;
        public bool ShowOnTile;

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