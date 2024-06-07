using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class LoadValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "LoadValue";
        public const string NAME = "Load Value";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(LoadValueInstructionTile);

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

        private const string _x = "X";
        private const string _y = "Y";
        private const string _z = "Z";
        private const string _keyFormatString = "<varType>{0}<varType><var>{1}</var><brain>{2}</brain><axis>{3}</axis>";
        protected const string _saveSlotFormatString = "<SaveSlot>{0}</SaveSlot>";
        protected const string _uniqueKeyFormatString = "<UniqueKey>{0}</UniqueKey>";
        private bool UseBrainName => _keyNameType == StorageStringFormatType.VariableAndBrainName || _keyNameType == StorageStringFormatType.CustomAndBrainName;

        public LoadValueInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_variable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_keyNameString))
                _keyName = _brain.Variables.GetString(_keyNameString);

            if (!string.IsNullOrEmpty(_brainNameString))
                _brainName = _brain.Variables.GetString(_brainNameString);

            if (!string.IsNullOrEmpty(_uniqueKeyName))
                _uniqueKey = _brain.Variables.GetString(_uniqueKeyName);

            if (!string.IsNullOrEmpty(_saveSlotName))
                _saveSlot = _brain.Variables.GetFloat(_saveSlotName);

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
                string storageKey = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Number, keyName, brainName, string.Empty);

                if (PlayerPrefs.HasKey(storageKey))
                    _brain.Variables.Set(_variable, PlayerPrefs.GetFloat(storageKey));
            }
            else if (myValue is IMonaVariablesBoolValue)
            {
                string storageKey = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Bool, keyName, brainName, string.Empty);

                if (PlayerPrefs.HasKey(storageKey))
                {
                    int boolBinary = PlayerPrefs.GetInt(storageKey);
                    _brain.Variables.Set(_variable, boolBinary == 1);
                }
            }
            else if (myValue is IMonaVariablesStringValue)
            {
                string storageKey = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.String, keyName, brainName, string.Empty);

                if (PlayerPrefs.HasKey(storageKey))
                    _brain.Variables.Set(_variable, PlayerPrefs.GetString(storageKey));
            }
            else if (myValue is IMonaVariablesVector2Value)
            {
                string storageX = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _x);
                string storageY = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _y);

                if (PlayerPrefs.HasKey(storageX) && PlayerPrefs.HasKey(storageY))
                {
                    float x = PlayerPrefs.GetFloat(storageX);
                    float y = PlayerPrefs.GetFloat(storageY);

                    _brain.Variables.Set(_variable, new Vector2(x, y));
                }
            }
            else if (myValue is IMonaVariablesVector3Value)
            {
                string storageX = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _x);
                string storageY = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _y);
                string storageZ = saveSlotString + uniqueKeyString + GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _z);

                if (PlayerPrefs.HasKey(storageX) && PlayerPrefs.HasKey(storageY) && PlayerPrefs.HasKey(storageZ))
                {
                    float x = PlayerPrefs.GetFloat(storageX);
                    float y = PlayerPrefs.GetFloat(storageY);
                    float z = PlayerPrefs.GetFloat(storageZ);

                    _brain.Variables.Set(_variable, new Vector3(x, y, z));
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        private string GetStorageKeyString(StorageVariableType varType, string keyName, string brainName, string axis)
        {
            return string.Format(_keyFormatString, varType.ToString(), keyName, brainName, axis);
        }
    }
}