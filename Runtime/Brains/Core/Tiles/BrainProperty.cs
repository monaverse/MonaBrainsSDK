using System;

namespace Mona.Brains.Core
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