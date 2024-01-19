using Mona.Brains.Core.Brain;
using Mona.Brains.Core.State.Structs;
using Mona.Brains.Core.Events;
using Mona.Core.State;
using Unity.VisualScripting;
using UnityEngine;
using System;

namespace Mona.Brains.Core.State
{
    [Serializable]
    public class MonaBrainState : MonaState, IMonaBrainState
    {
        public MonaBrainState() : base() { }

        public void Set(string name, IMonaBrain value)
        {
            var prop = GetValue(name, typeof(MonaStateBrain));
            var propValue = ((IMonaStateBrainValue)prop);
            if (propValue.Value != value)
            {
                propValue.Value = value;
                FireBrainEvent(name, propValue.Value);
            }
        }

        public IMonaBrain GetBrain(string name)
        {
            var prop = GetValue(name, typeof(MonaStateBrain));
            return ((IMonaStateBrainValue)prop).Value;
        }

        private void FireBrainEvent(string variableName, IMonaBrain value)
        {
            EventBus.Trigger<MonaBrainChangedEvent>(new EventHook(MonaBrainConstants.BRAIN_CHANGED_EVENT, _monaBody), new MonaBrainChangedEvent(variableName, value));
        }
    }
}