using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.State;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;

namespace Mona.SDK.Brains.Core.State
{
    [Serializable]
    public class MonaBrainState : MonaState, IMonaBrainState
    {
        private IMonaBrain _brain;

        public MonaBrainState(GameObject gameObject = null, IMonaBrain brain = null) : base(gameObject)
        {
            _brain = brain;
        }

        public void SetGameObject(GameObject gameObject, IMonaBrain brain)
        {
            SetGameObject(gameObject);
            _brain = brain;
        }

        public void Set(string name, IMonaBrain value)
        {
            var prop = GetValue(name, typeof(MonaStateBrain));
            var propValue = ((IMonaStateBrainValue)prop);
            if (propValue.Value != value)
            {
                propValue.Value = value;
                FireValueEvent(name, prop);
            }
        }

        public IMonaBrain GetBrain(string name)
        {
            var prop = GetValue(name, typeof(MonaStateBrain));
            return ((IMonaStateBrainValue)prop).Value;
        }

        protected override void FireValueEvent(string variableName, IMonaStateValue value)
        {
            base.FireValueEvent(variableName, value);
            EventBus.Trigger<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain), new MonaValueChangedEvent(variableName, value));
        }
    }
}