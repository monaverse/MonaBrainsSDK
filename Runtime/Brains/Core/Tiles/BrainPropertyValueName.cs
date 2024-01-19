using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyValueName : System.Attribute
    {
        public string PropertyName;

        public BrainPropertyValueName(string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }
}