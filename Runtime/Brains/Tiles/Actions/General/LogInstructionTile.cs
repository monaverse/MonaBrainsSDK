using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class LogInstructionTile : InstructionTile, ILogInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "Log";
        public const string NAME = "Log";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(LogInstructionTile);

        [SerializeField] private string _prefix;
        [SerializeField] private string _prefixName;
        [BrainProperty] public string Prefix { get => _prefix; set => _prefix = value; }
        [BrainPropertyValueName("Prefix", typeof(IMonaVariablesValue))] public string PrefixName { get => _prefixName; set => _prefixName = value; }

        [SerializeField] private string _message;
        [SerializeField] private string _messageValueName;
        [BrainProperty] public string Message { get => _message; set => _message = value; }
        [BrainPropertyValueName("Message", typeof(IMonaVariablesValue))] public string MessageValueName { get => _messageValueName; set => _messageValueName = value; }

        [SerializeField] private bool _logBodyData = true;
        [SerializeField] private string _logBodyDataName;
        [BrainProperty(false)] public bool LogBodyData { get => _logBodyData; set => _logBodyData = value; }
        [BrainPropertyValueName("LogBodyData", typeof(IMonaVariablesBoolValue))] public string LogBodyDataName { get => _logBodyDataName; set => _logBodyDataName = value; }

        [SerializeField] private bool _logBrainData;
        [SerializeField] private string _logBrainDataName;
        [BrainProperty(false)] public bool LogBrainData { get => _logBrainData; set => _logBrainData = value; }
        [BrainPropertyValueName("LogBrainData", typeof(IMonaVariablesBoolValue))] public string LogBrainDataName { get => _logBrainDataName; set => _logBrainDataName = value; }

        [SerializeField] private bool _logFrameData;
        [SerializeField] private string _logFrameDataName;
        [BrainProperty(false)] public bool LogFrameData { get => _logFrameData; set => _logFrameData = value; }
        [BrainPropertyValueName("LogFrameData", typeof(IMonaVariablesBoolValue))] public string FrameDataName { get => _logFrameDataName; set => _logFrameDataName = value; }

        [SerializeField] private bool _logTimeData;
        [SerializeField] private string _logTimeDataName;
        [BrainProperty(false)] public bool LogTimeData { get => _logTimeData; set => _logTimeData = value; }
        [BrainPropertyValueName("LogTimeData", typeof(IMonaVariablesBoolValue))] public string LogTimeDataName { get => _logTimeDataName; set => _logTimeDataName = value; }

        private const string _variableLog = "{0}Variable: '{1}' = '{2}'";
        private const string _nonVariableLog = "{0}{1} {2}";
        private const string _prefixLog = "{0}: ";
        private const string _bodyLog = " | Body = '{0}'";
        private const string _brainLog = " | Brain = '{0};";
        private const string _frameLog = " | Framerate = {0} | FrameCount = {1}";
        private const string _timeLog = " | GameTime = {0} | DeltaTime = {1}";
        

        public LogInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_prefixName))
                _prefix = _brain.Variables.GetString(_prefixName);

            if (!string.IsNullOrEmpty(_logBodyDataName))
                _logBodyData = _brain.Variables.GetBool(_logBodyDataName);

            if (!string.IsNullOrEmpty(_logBrainDataName))
                _logBrainData = _brain.Variables.GetBool(_logBrainDataName);

            if (!string.IsNullOrEmpty(_logFrameDataName))
                _logFrameData = _brain.Variables.GetBool(_logFrameDataName);

            if (!string.IsNullOrEmpty(_logTimeDataName))
                _logTimeData = _brain.Variables.GetBool(_logTimeDataName);

            string fullLog;
            string prefix = string.IsNullOrEmpty(_prefix) ? string.Empty : string.Format(_prefixLog, _prefix);
            string bodyObject = _logBodyData ? string.Format(_bodyLog, _brain.Body.Transform.gameObject.name) : string.Empty;
            string brainData = _logBrainData ? string.Format(_brainLog, _brain.Name) : string.Empty;
            string frames = _logFrameData ? string.Format(_frameLog, (int)(1f / Time.unscaledDeltaTime), Time.frameCount) : string.Empty;
            string time = _logTimeData ? string.Format(_timeLog, Time.time, Time.deltaTime) : string.Empty;

            if (!string.IsNullOrEmpty(_messageValueName))
            {
                var variable = _brain.Variables.GetVariable(_messageValueName);
                if (variable is IMonaVariablesStringValue) _message = string.Format(_variableLog, prefix, _messageValueName, ((IMonaVariablesStringValue)variable).Value);
                if (variable is IMonaVariablesFloatValue) _message = string.Format(_variableLog, prefix, _messageValueName, ((IMonaVariablesFloatValue)variable).ValueToReturnFromTile.ToString());
                if (variable is IMonaVariablesBoolValue) _message = string.Format(_variableLog, prefix, _messageValueName, ((IMonaVariablesBoolValue)variable).Value.ToString());
                if (variable is IMonaVariablesVector2Value) _message = string.Format(_variableLog, prefix, _messageValueName, ((IMonaVariablesVector2Value)variable).Value.ToString());
                if (variable is IMonaVariablesVector3Value) _message = string.Format(_variableLog, prefix, _messageValueName, ((IMonaVariablesVector3Value)variable).Value.ToString());

                fullLog = _message + frames + time + bodyObject + brainData;
            }
            else
            {
                fullLog = string.Format(_nonVariableLog, prefix, _message, _brain.Body.Transform.gameObject) + frames + time + bodyObject + brainData;
            }

            Debug.Log(fullLog);

            return Complete(InstructionTileResult.Success);
        }
    }
}