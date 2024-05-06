﻿using Mona.SDK.Brains.Core.Enums;
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
    public class DeleteValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "DeleteValue";
        public const string NAME = "Delete Value";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(DeleteValueInstructionTile);

        [SerializeField] private string _keyName;
        [SerializeField] private string _keyNameString;
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.CustomName)]
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.CustomAndBrainName)]
        [BrainProperty(true)] public string KeyName { get => _keyName; set => _keyName = value; }
        [BrainPropertyValueName("KeyName", typeof(IMonaVariablesStringValue))] public string KeyNameString { get => _keyNameString; set => _keyNameString = value; }

        [SerializeField] private string _variable;
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.VariableName)]
        [BrainPropertyShow(nameof(KeyNameType), (int)StorageStringFormatType.VariableAndBrainName)]
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

        private const string _x = "X";
        private const string _y = "Y";
        private const string _z = "Z";
        private const string _keyFormatString = "<varType>{0}<varType><var>{1}</var><brain>{2}</brain><axis>{3}</axis>";
        private bool UseBrainName => _keyNameType == StorageStringFormatType.VariableAndBrainName || _keyNameType == StorageStringFormatType.CustomAndBrainName;

        public DeleteValueInstructionTile() { }

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

            if (!string.IsNullOrEmpty(_saveNowName))
                _saveNow = _brain.Variables.GetBool(_saveNowName);

            IMonaVariablesValue myValue = _brain.Variables.GetVariable(_variable);

            string keyName = _keyNameType == StorageStringFormatType.VariableName || _keyNameType == StorageStringFormatType.VariableAndBrainName ?
                myValue.Name : _keyName;

            string brainName = UseBrainName ? _brainName : string.Empty;

            if (myValue is IMonaVariablesFloatValue)
            {
                string storageKey = GetStorageKeyString(StorageVariableType.Number, keyName, brainName, string.Empty);

                if (PlayerPrefs.HasKey(storageKey))
                    PlayerPrefs.DeleteKey(storageKey);
            }
            else if (myValue is IMonaVariablesBoolValue)
            {
                string storageKey = GetStorageKeyString(StorageVariableType.Bool, keyName, brainName, string.Empty);

                if (PlayerPrefs.HasKey(storageKey))
                    PlayerPrefs.DeleteKey(storageKey);
            }
            else if (myValue is IMonaVariablesStringValue)
            {
                string storageKey = GetStorageKeyString(StorageVariableType.String, keyName, brainName, string.Empty);

                if (PlayerPrefs.HasKey(storageKey))
                    PlayerPrefs.DeleteKey(storageKey);
            }
            else if (myValue is IMonaVariablesVector2Value)
            {
                string storageX = GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _x);
                string storageY = GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _y);

                if (PlayerPrefs.HasKey(storageX))
                    PlayerPrefs.DeleteKey(storageX);

                if (PlayerPrefs.HasKey(storageY))
                    PlayerPrefs.DeleteKey(storageY);
            }
            else if (myValue is IMonaVariablesVector3Value)
            {
                string storageX = GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _x);
                string storageY = GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _y);
                string storageZ = GetStorageKeyString(StorageVariableType.Vector2, keyName, brainName, _z);

                if (PlayerPrefs.HasKey(storageX))
                    PlayerPrefs.DeleteKey(storageX);

                if (PlayerPrefs.HasKey(storageY))
                    PlayerPrefs.DeleteKey(storageY);

                if (PlayerPrefs.HasKey(storageZ))
                    PlayerPrefs.DeleteKey(storageZ);
            }

            if (_saveNow)
                PlayerPrefs.Save();

            return Complete(InstructionTileResult.Success);
        }

        private string GetStorageKeyString(StorageVariableType varType, string keyName, string brainName, string axis)
        {
            return string.Format(_keyFormatString, varType.ToString(), keyName, brainName, axis);
        }
    }
}