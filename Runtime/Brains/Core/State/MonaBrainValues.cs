using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.State
{
    [Serializable]
    public class MonaBrainValues : MonoBehaviour, IMonaBrainState
    {
        [SerializeField]
        private MonaBrainState _state = new MonaBrainState();
        public MonaBrainState State => _state;

        public void Awake()
        {
            SetGameObject(gameObject);
        }

        public List<IMonaStateValue> Values { get => _state.Values; set => _state.Values = value; }

        public void SetGameObject(GameObject gameObject) => _state.SetGameObject(gameObject);
        public IMonaStateValue CreateValue(string variableName, Type type, int i) => _state.CreateValue(variableName, type, i);
        public IMonaBody GetBody(string variableName) => _state.GetBody(variableName);
        public IMonaBrain GetBrain(string variableName) => _state.GetBrain(variableName);
        public bool GetBool(string variableName) => _state.GetBool(variableName);
        public float GetFloat(string variableName) => _state.GetFloat(variableName);
        public int GetInt(string variableName) => _state.GetInt(variableName);
        public string GetString(string variableName) => _state.GetString(variableName);
        public IMonaStateValue GetValue(string variableName, Type type) => _state.GetValue(variableName, type);
        public Vector2 GetVector2(string variableName) => _state.GetVector2(variableName);
        public Vector3 GetVector3(string variableName) => _state.GetVector3(variableName);
        public void Set(string variableName, int value, bool isNetworked) => _state.Set(variableName, value, isNetworked);
        public void Set(string variableName, bool value, bool isNetworked) => _state.Set(variableName, value, isNetworked);
        public void Set(string variableName, string value, bool isNetworked) => _state.Set(variableName, value, isNetworked);
        public void Set(string variableName, float value, bool isNetworked) => _state.Set(variableName, value, isNetworked);
        public void Set(string variableName, IMonaBody value) => _state.Set(variableName, value);
        public void Set(string variableName, IMonaBrain value) => _state.Set(variableName, value);
        public void Set(string variableName, Vector2 value, bool isNetworked) => _state.Set(variableName, value, isNetworked);
        public void Set(string variableName, Vector3 value, bool isNetworked) => _state.Set(variableName, value, isNetworked);
    }
}