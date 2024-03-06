using System;

namespace Mona.SDK.Brains.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BrainPropertyShow : System.Attribute
    {
        public string Name;
        public int Value;

        public BrainPropertyShow(string name, int value) 
        {
            Name = name;
            Value = value;
        }
    }
}