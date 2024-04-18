using System;

namespace Mona.SDK.Brains.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BrainPropertyShow : System.Attribute
    {
        public string Name;
        public int Value;
        public bool BoolValue;
        public bool UseBoolValue;

        public BrainPropertyShow(string name, int value) 
        {
            Name = name;
            Value = value;
            UseBoolValue = false;
        }

        public BrainPropertyShow(string name, bool boolValue)
        {
            Name = name;
            BoolValue = boolValue;
            UseBoolValue = true;
        }
    }
}