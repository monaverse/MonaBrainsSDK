using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class StoreValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "StoreValue";
        public const string NAME = "Store Value";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(StoreValueInstructionTile);

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

        [SerializeField] private bool _saveNow = false;
        [SerializeField] private string _saveNowName;
        [BrainProperty(false)] public bool SaveNow { get => _saveNow; set => _saveNow = value; }
        [BrainPropertyValueName("SaveNow", typeof(IMonaVariablesBoolValue))] public string SaveNowName { get => _saveNowName; set => _saveNowName = value; }

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

        private const string _keyFormatString = "<varType>{0}<varType><var>{1}</var><brain>{2}</brain><axis>{3}</axis>";
        protected const string _saveSlotFormatString = "<SaveSlot>{0}</SaveSlot>";
        protected const string _uniqueKeyFormatString = "<UniqueKey>{0}</UniqueKey>";
        private bool UseBrainName => _keyNameType == StorageStringFormatType.VariableAndBrainName || _keyNameType == StorageStringFormatType.CustomAndBrainName;

        public StoreValueInstructionTile() { }

        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainStorage _localStorage;
        private IBrainStorage _cloudStorage;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null || string.IsNullOrEmpty(_variable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_keyNameString))
                _keyName = _brain.Variables.GetString(_keyNameString);

            if (!string.IsNullOrEmpty(_brainNameString))
                _brainName = _brain.Variables.GetString(_brainNameString);

            if (!string.IsNullOrEmpty(_saveNowName))
                _saveNow = _brain.Variables.GetBool(_saveNowName);

            if (!string.IsNullOrEmpty(_uniqueKeyName))
                _uniqueKey = _brain.Variables.GetString(_uniqueKeyName);

            if (!string.IsNullOrEmpty(_saveSlotName))
                _saveSlot = _brain.Variables.GetFloat(_saveSlotName);

            if (_storageTarget != StorageTargetType.Cloud && _localStorage == null)
            {
                _localStorage = _globalBrainRunner.LocalStorage;

                if (_localStorage == null)
                    return Complete(InstructionTileResult.Success);
            }

            if (_storageTarget != StorageTargetType.Local && _cloudStorage == null)
            {
                _cloudStorage = _globalBrainRunner.ClousStorage;

                if (_cloudStorage == null)
                    return Complete(InstructionTileResult.Success);
            }

            IMonaVariablesValue myValue = _brain.Variables.GetVariable(_variable);

            string keyName = _keyNameType == StorageStringFormatType.VariableName || _keyNameType == StorageStringFormatType.VariableAndBrainName ?
                myValue.Name : _keyName;
            string brainName = UseBrainName ? _brainName : string.Empty;

            string saveSlotString = _saveSlotUsage == UsageType.Defined ?
                string.Format(_saveSlotFormatString, _saveSlot) : string.Empty;

            string uniqueKeyString = _addUniqueKey == UsageType.Defined ?
                string.Format(_uniqueKeyFormatString, _uniqueKey) : string.Empty;

            if (myValue is IMonaVariablesFloatValue)
            {
                float value = ((IMonaVariablesFloatValue)myValue).ValueToReturnFromTile;
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Number, keyName, brainName, string.Empty);
                if (_storageTarget != StorageTargetType.Cloud) _localStorage.SetFloat(key, value, out bool _, _saveNow);
                if (_storageTarget != StorageTargetType.Local) _cloudStorage.SetFloat(key, value, out bool _, _saveNow);
            }
            else if (myValue is IMonaVariablesBoolValue)
            {
                bool value = ((IMonaVariablesBoolValue)myValue).Value;
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Bool, keyName, brainName, string.Empty);
                if (_storageTarget != StorageTargetType.Cloud) _localStorage.SetBool(key, value, out bool _, _saveNow);
                if (_storageTarget != StorageTargetType.Local) _cloudStorage.SetBool(key, value, out bool _, _saveNow);
            }
            else if (myValue is IMonaVariablesStringValue)
            {
                string value = ((IMonaVariablesStringValue)myValue).Value;
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.String, keyName, brainName, string.Empty);
                if (_storageTarget != StorageTargetType.Cloud) _localStorage.SetString(key, value, out bool _, _saveNow);
                if (_storageTarget != StorageTargetType.Local) _cloudStorage.SetString(key, value, out bool _, _saveNow);
            }
            else if (myValue is IMonaVariablesVector2Value)
            {
                Vector2 value = ((IMonaVariablesVector2Value)myValue).Value;
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, string.Empty);
                if (_storageTarget != StorageTargetType.Cloud) _localStorage.SetVector2(key, value, out bool _, _saveNow);
                if (_storageTarget != StorageTargetType.Local) _cloudStorage.SetVector2(key, value, out bool _, _saveNow);
            }
            else if (myValue is IMonaVariablesVector3Value)
            {
                Vector3 value = ((IMonaVariablesVector3Value)myValue).Value;
                string key = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector3, keyName, brainName, string.Empty);
                if (_storageTarget != StorageTargetType.Cloud) _localStorage.SetVector3(key, value, out bool _, _saveNow);
                if (_storageTarget != StorageTargetType.Local) _cloudStorage.SetVector3(key, value, out bool _, _saveNow);
            }

            return Complete(InstructionTileResult.Success);
        }

        private string GetStorageKeyString(StorageVariableType varType, string keyName, string brainName, string axis)
        {
            return string.Format(_keyFormatString, varType.ToString(), keyName, brainName, axis);
        }
    }
}