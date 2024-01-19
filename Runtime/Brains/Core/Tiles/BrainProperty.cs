using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainProperty : System.Attribute
    {
        public bool ShowOnTile;

        public BrainProperty(bool showOnTile = true)
        {
            ShowOnTile = showOnTile;
        }
    }
}