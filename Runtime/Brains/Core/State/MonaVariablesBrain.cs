using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Core.State.Structs
{
    [Serializable]
    public class MonaVariablesBrain : IMonaVariablesValue, IMonaVariablesBrainValue
    {
        public event Action OnChange = delegate { };

        public void Change() => OnChange();

        [SerializeField]
        private string _name;

        public string Name { get => _name; set => _name = value; }

        [SerializeField]
        public IMonaBrain _value;
        private IMonaBrain _resetValue;

        public IMonaBrain Value { get => _value; set => _value = value; }

        public MonaVariablesBrain() { }

        public void Reset()
        {
            _value = _resetValue;
        }

        public void SaveReset()
        {
            _resetValue = _value;
        }
    }
}