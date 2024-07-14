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
using System.Collections.Generic;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Core.State
{
    [Serializable]
    public class MonaBrainVariables : MonaVariables, IMonaBrainVariables
    {
        public event Action OnStateAuthorityChanged = delegate { };

        private IMonaBrain _brain;

        private Dictionary<string, Vector3> _internalVariables = new Dictionary<string, Vector3>();

        private EventHook _eventHook;

        public MonaBrainVariables(GameObject gameObject = null, IMonaBrain brain = null) : base(gameObject)
        {
            _brain = brain;
        }

        public void SaveResetDefaults()
        {
            for (var i = 0; i < VariableList.Count; i++)
                VariableList[i].SaveReset();
        }

        public void SetGameObject(GameObject gameObject, IMonaBrain brain)
        {
            SetGameObject(gameObject);
            _brain = brain;
            _eventHook = new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, _brain);
        }

        public void Set(string name, IMonaBrain value)
        {
            var prop = GetVariable(name, typeof(MonaVariablesBrain));
            var propValue = ((IMonaVariablesBrainValue)prop);
            if (propValue.Value != value)
            {
                propValue.Value = value;
                FireValueEvent(name, prop);
            }
        }

        public void SetInternal(string name, Vector3 value)
        {
            _internalVariables[name] = value;
        }

        public Vector3 GetInternalVector3(string name)
        {
            if (_internalVariables.ContainsKey(name))
                return _internalVariables[name];
            return default;
        }

        public IMonaBrain GetBrain(string name)
        {
            var prop = GetVariable(name, typeof(MonaVariablesBrain));
            return ((IMonaVariablesBrainValue)prop).Value;
        }

        public override void FireValueEvent(string variableName, IMonaVariablesValue value, bool isNetworked = false)
        {
            base.FireValueEvent(variableName, value, isNetworked);
            if (variableName == MonaBrainConstants.RESULT_STATE) return;
            if (variableName.StartsWith("__")) return;
            MonaEventBus.Trigger<MonaValueChangedEvent>(_eventHook, new MonaValueChangedEvent(variableName, value));
        }

        public bool HasControl()
        {
            return NetworkVariables == null || NetworkVariables.HasControl();
        }
    }
}