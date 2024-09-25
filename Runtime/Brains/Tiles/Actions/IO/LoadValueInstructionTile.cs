using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core.Utils;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class LoadValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "LoadValue";
        public const string NAME = "Load Value";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(LoadValueInstructionTile);

        [SerializeField] private StorageTargetType _storageTarget = StorageTargetType.LocalAndCloud;
        [BrainPropertyEnum(true)] public StorageTargetType StorageTarget { get => _storageTarget; set => _storageTarget = value; }

        [SerializeField] private string _keyName;
        [SerializeField] private string _keyNameString;
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.CustomName)]
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.CustomAndBrainName)]
        [BrainProperty(true)] public string KeyName { get => _keyName; set => _keyName = value; }
        [BrainPropertyValueName("KeyName", typeof(IMonaVariablesStringValue))] public string KeyNameString { get => _keyNameString; set => _keyNameString = value; }

        [SerializeField] private string _variable;
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string Variable { get => _variable; set => _variable = value; }

        [SerializeField] private StorageStringFormatType _keyNameType = StorageStringFormatType.VariableName;
        [BrainPropertyEnum(false)] public StorageStringFormatType KeyNameType { get => _keyNameType; set => _keyNameType = value; }

        [SerializeField] private StorageBrainNameType _brainNameType = StorageBrainNameType.ThisBrain;
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.CustomAndBrainName)]
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.VariableAndBrainName)]
        [BrainPropertyEnum(false)] public StorageBrainNameType BrainNameType { get => _brainNameType; set => _brainNameType = value; }

        [SerializeField] private string _brainName;
        [SerializeField] private string _brainNameString;
        [BrainPropertyShow(nameof(BrainNameType), (int)StorageBrainNameType.CustomBrainName)]
        [BrainProperty(true)] public string BrainName { get => _brainName; set => _brainName = value; }
        [BrainPropertyValueName("BrainName", typeof(IMonaVariablesStringValue))] public string BrainNameString { get => _brainNameString; set => _brainNameString = value; }

        [SerializeField] protected string _uniqueKey;
        [SerializeField] protected string _uniqueKeyName;
        [BrainPropertyShow(nameof(AddUniqueKey), (int)UsageType.Defined)]
        [BrainProperty(true)] public string UniqueKey { get => _uniqueKey; set => _uniqueKey = value; }
        [BrainPropertyValueName("UniqueKey", typeof(IMonaVariablesStringValue))] public string UniqueKeyName { get => _uniqueKeyName; set => _uniqueKeyName = value; }

        [SerializeField] protected float _saveSlot;
        [SerializeField] protected string _saveSlotName;
        [BrainPropertyShow(nameof(SaveSlotUsage), (int)UsageType.Defined)]
        [BrainProperty(true)] public float SaveSlot { get => _saveSlot; set => _saveSlot = value; }
        [BrainPropertyValueName("SaveSlot", typeof(IMonaVariablesFloatValue))] public string SaveSlotName { get => _saveSlotName; set => _saveSlotName = value; }

        [SerializeField] protected UsageType _addUniqueKey = UsageType.None;
        [BrainPropertyEnum(false)] public UsageType AddUniqueKey { get => _addUniqueKey; set => _addUniqueKey = value; }

        [SerializeField] protected UsageType _saveSlotUsage = UsageType.None;
        [BrainPropertyEnum(false)] public UsageType SaveSlotUsage { get => _saveSlotUsage; set => _saveSlotUsage = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        private const string _keyFormatString = "{0}{1}{2}{3}";
        protected const string _saveSlotFormatString = "{0}";
        protected const string _uniqueKeyFormatString = "{0}";
        private bool UseBrainName => _keyNameType == StorageStringFormatType.VariableAndBrainName || _keyNameType == StorageStringFormatType.CustomAndBrainName;

        public LoadValueInstructionTile() { }

        private bool _storageProcessed;
        private bool _active;
        private bool _isRunning;
        private IMonaVariablesValue _storageVariable;
        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainStorageAsync _localStorage;
        private IBrainStorageAsync _cloudStorage;
        private BrainProcess _localProcess;
        private BrainProcess _cloudProcess;
        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool UseLocalStorage => _storageTarget != StorageTargetType.Cloud;
        private bool UseCloudStorage => _storageTarget != StorageTargetType.Local;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                if (_isRunning)
                    LostControl();

                return;
            }

            if (_isRunning)
            {
                AddFixedTickDelegate();
            }
        }

        public override void Unload(bool destroy = false)
        {
            SetActive(false);
            _isRunning = false;
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return _isRunning;
        }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void LostControl()
        {
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick();
        }

        private void FixedTick()
        {
            if (UseLocalStorage && (_localStorage == null || _localProcess == null || _localProcess.IsProcessing))
                return;

            if (UseCloudStorage && (_cloudStorage == null || _cloudProcess == null || _cloudProcess.IsProcessing))
                return;

            _isRunning = false;
            bool localSuccess = UseLocalStorage ? _localProcess.WasSuccessful : false;

            if (_storageVariable is IMonaVariablesFloatValue)
            {
                if (localSuccess) _brain.Variables.Set(_variable, _localProcess.GetFloat());
                else if (UseCloudStorage && _cloudProcess.WasSuccessful) _brain.Variables.Set(_variable, _cloudProcess.GetFloat());
            }
            else if (_storageVariable is IMonaVariablesBoolValue)
            {
                if (localSuccess) _brain.Variables.Set(_variable, _localProcess.GetBool());
                else if (UseCloudStorage && _cloudProcess.WasSuccessful) _brain.Variables.Set(_variable, _cloudProcess.GetBool());
            }
            else if (_storageVariable is IMonaVariablesStringValue)
            {
                if (localSuccess) _brain.Variables.Set(_variable, _localProcess.GetString());
                else if (UseCloudStorage && _cloudProcess.WasSuccessful) _brain.Variables.Set(_variable, _cloudProcess.GetString());
            }
            else if (_storageVariable is IMonaVariablesVector2Value)
            {
                if (localSuccess) _brain.Variables.Set(_variable, _localProcess.GetVector2());
                else if (UseCloudStorage && _cloudProcess.WasSuccessful) _brain.Variables.Set(_variable, _cloudProcess.GetVector2());
            }
            else if (_storageVariable is IMonaVariablesVector3Value)
            {
                if (localSuccess) _brain.Variables.Set(_variable, _localProcess.GetVector3());
                else if (UseCloudStorage && _cloudProcess.WasSuccessful) _brain.Variables.Set(_variable, _cloudProcess.GetVector3());
            }

            if (!string.IsNullOrEmpty(_storeSuccessOn))
            {
                bool success = (UseLocalStorage && _localProcess.WasSuccessful) || (UseCloudStorage && _cloudProcess.WasSuccessful);
                _brain.Variables.Set(_storeSuccessOn, success);
            }

            Complete(InstructionTileResult.Success, true);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null || string.IsNullOrEmpty(_variable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _storageProcessed = false;

            if (!_isRunning)
            {
                if (!string.IsNullOrEmpty(_keyNameString))
                    _keyName = _brain.Variables.GetString(_keyNameString);

                if (!string.IsNullOrEmpty(_brainNameString))
                    _brainName = _brain.Variables.GetString(_brainNameString);

                if (!string.IsNullOrEmpty(_uniqueKeyName))
                    _uniqueKey = _brain.Variables.GetString(_uniqueKeyName);

                if (!string.IsNullOrEmpty(_saveSlotName))
                    _saveSlot = _brain.Variables.GetFloat(_saveSlotName);

                if (UseLocalStorage && _localStorage == null)
                {
                    _localStorage = _globalBrainRunner.LocalStorage;

                    if (_localStorage == null)
                        return Complete(InstructionTileResult.Success);
                }

                if (UseCloudStorage && _cloudStorage == null)
                {
                    _cloudStorage = _globalBrainRunner.CloudStorage;

                    if (_cloudStorage == null)
                        return Complete(InstructionTileResult.Success);
                }

                _storageVariable = _brain.Variables.GetVariable(_variable);

                ProcessLoad();

                if(!_storageProcessed)
                    AddFixedTickDelegate();
            }

            if (UseLocalStorage || UseCloudStorage)
                return Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private async Task ProcessLoad()
        {

            string keyName = _keyNameType == StorageStringFormatType.VariableName || _keyNameType == StorageStringFormatType.VariableAndBrainName ?
                _storageVariable.Name : _keyName;

            string brainName = UseBrainName ? _brainName : string.Empty;

            string saveSlotString = _saveSlotUsage == UsageType.Defined ?
                string.Format(_saveSlotFormatString, _saveSlot) : string.Empty;

            string uniqueKeyString = _addUniqueKey == UsageType.Defined ?
                string.Format(_uniqueKeyFormatString, _uniqueKey) : string.Empty;

            if (_storageVariable is IMonaVariablesFloatValue)
            {
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Number, keyName, brainName, string.Empty);
                if (UseLocalStorage) _localProcess = await _localStorage.LoadFloat(key);
                if (UseCloudStorage) _cloudProcess = await _cloudStorage.LoadFloat(key);
            }
            else if (_storageVariable is IMonaVariablesBoolValue)
            {
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Bool, keyName, brainName, string.Empty);
                if (UseLocalStorage) _localProcess = await _localStorage.LoadBool(key);
                if (UseCloudStorage) _cloudProcess = await _cloudStorage.LoadBool(key);
            }
            else if (_storageVariable is IMonaVariablesStringValue)
            {
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.String, keyName, brainName, string.Empty);
                if (UseLocalStorage) _localProcess = await _localStorage.LoadString(key);
                if (UseCloudStorage) _cloudProcess = await _cloudStorage.LoadString(key);
            }
            else if (_storageVariable is IMonaVariablesVector2Value)
            {
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, string.Empty);
                if (UseLocalStorage) _localProcess = await _localStorage.LoadVector2(key);
                if (UseCloudStorage) _cloudProcess = await _cloudStorage.LoadVector2(key);
            }
            else if (_storageVariable is IMonaVariablesVector3Value)
            {
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector3, keyName, brainName, string.Empty);
                if (UseLocalStorage) _localProcess = await _localStorage.LoadVector3(key);
                if (UseCloudStorage) _cloudProcess = await _cloudStorage.LoadVector3(key);
            }

            _storageProcessed = true;
        }

        private string GetStorageKeyString(StorageVariableType varType, string keyName, string brainName, string axis)
        {
            return string.Format(_keyFormatString, varType.ToString(), keyName, brainName, axis);
        }
    }
}