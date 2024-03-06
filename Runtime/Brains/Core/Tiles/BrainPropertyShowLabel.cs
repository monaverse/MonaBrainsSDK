using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyShowLabel : BrainPropertyShow
    {
        public string Label;

        public BrainPropertyShowLabel(string name, int value, string label) : base(name, value)
        {
            Label = label;
        }
    }
}