using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Network.Interfaces;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.State
{
    [Serializable]
    public class MonaBrainVariablesBehaviour : MonoBehaviour, IMonaBrainVariables
    {
        [SerializeField]
        private MonaBrainVariables _variables = new MonaBrainVariables();
        public MonaBrainVariables Variables => _variables;

        public void Awake()
        {
            SetGameObject(gameObject);
        }

        public void SetNetworkVariables(INetworkMonaVariables variables) => _variables.SetNetworkVariables(variables);
        public List<IMonaVariablesValue> VariableList { get => _variables.VariableList; set => _variables.VariableList = value; }

        public void SyncValuesOnNetwork() => _variables.SyncValuesOnNetwork();
        public void SetGameObject(GameObject gameObject) => _variables.SetGameObject(gameObject);
        public void SetGameObject(GameObject gameObject, IMonaBrain brain) => _variables.SetGameObject(gameObject, brain);
        public IMonaVariablesValue CreateVariable(string variableName, Type type, int i) => _variables.CreateVariable(variableName, type, i);
        public IMonaBody GetBody(string variableName, bool createIfNotFound = true) => _variables.GetBody(variableName);
        public IMonaBrain GetBrain(string variableName) => _variables.GetBrain(variableName);
        public bool GetBool(string variableName, bool createIfNotFound = true) => _variables.GetBool(variableName);
        public float GetFloat(string variableName, bool createIfNotFound = true) => _variables.GetFloat(variableName);
        public int GetInt(string variableName, bool createIfNotFound = true) => _variables.GetInt(variableName);
        public string GetString(string variableName, bool createIfNotFound = true) => _variables.GetString(variableName);
        public IMonaVariablesValue GetVariable(string variableName) => _variables.GetVariable(variableName);
        public IMonaVariablesValue GetVariable(string variableName, Type type, bool createIfNotFound = true) => _variables.GetVariable(variableName, type);
        public IMonaVariablesValue GetVariableByIndex(int index) => _variables.GetVariableByIndex(index);
        public int GetVariableIndexByName(string name) => _variables.GetVariableIndexByName(name);
        public Vector2 GetVector2(string variableName, bool createIfNotFound = true) => _variables.GetVector2(variableName);
        public Vector3 GetVector3(string variableName, bool createIfNotFound = true) => _variables.GetVector3(variableName);
        public string GetValueAsString(string variableName) => _variables.GetValueAsString(variableName);
        public void Set(string variableName, int value, bool isNetworked, bool createIfNotFound = true) => _variables.Set(variableName, value, isNetworked);
        public void Set(string variableName, bool value, bool isNetworked, bool createIfNotFound = true) => _variables.Set(variableName, value, isNetworked);
        public void Set(string variableName, string value, bool isNetworked, bool createIfNotFound = true) => _variables.Set(variableName, value, isNetworked);
        public void Set(string variableName, float value, bool isNetworked, bool createIfNotFound = true) => _variables.Set(variableName, value, isNetworked);
        public void Set(string variableName, IMonaBody value, bool createIfNotFound = true) => _variables.Set(variableName, value);
        public void Set(string variableName, IMonaBrain value) => _variables.Set(variableName, value);
        public void Set(string variableName, Vector2 value, bool isNetworked, bool createIfNotFound = true) => _variables.Set(variableName, value, isNetworked);
        public void Set(string variableName, Vector3 value, bool isNetworked, bool createIfNotFound = true) => _variables.Set(variableName, value, isNetworked);
        public void SetInternal(string variableName, Vector3 value) => _variables.SetInternal(variableName, value);
        public Vector3 GetInternalVector3(string variableName) => _variables.GetInternalVector3(variableName);
        public void SaveResetDefaults() => _variables.SaveResetDefaults();
    }
}