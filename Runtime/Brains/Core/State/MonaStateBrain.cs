﻿using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Core.State.Structs
{
    [Serializable]
    public class MonaStateBrain : IMonaStateValue, IMonaStateBrainValue
    {
        [SerializeField]
        private string _name;

        public string Name { get => _name; set => _name = value; }

        [SerializeField]
        public IMonaBrain _value;

        public IMonaBrain Value { get => _value; set => _value = value; }

        public MonaStateBrain() { }
    }
}