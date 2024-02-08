using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyValueName : System.Attribute
    {
        public string PropertyName;
        public Type Type;

        public BrainPropertyValueName(string propertyName, Type type)
        {
            PropertyName = propertyName;
            Type = type;
        }
    }
}