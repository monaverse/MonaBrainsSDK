﻿using System;

namespace Mona.SDK.Brains.Core
{
    public class BrainPropertyEnum : BrainProperty
    {
        public string DefaultValue;

        public BrainPropertyEnum(bool showOnTile = true, string defaultValue = "") : base(showOnTile)
        {
            DefaultValue = defaultValue;
        }
    }
}