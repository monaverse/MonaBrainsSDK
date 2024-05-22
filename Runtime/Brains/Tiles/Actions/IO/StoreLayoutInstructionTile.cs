using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class StoreLayoutInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "StoreLayout";
        public const string NAME = "Store Layout";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(StoreLayoutInstructionTile);

        [SerializeField] private MonaBrainTargetLayoutType _target = MonaBrainTargetLayoutType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainTargetLayoutType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag = "Default";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _uniqueKey;
        [SerializeField] private string _uniqueKeyName;
        [BrainPropertyShow(nameof(AddUniqueKey), (int)UsageType.Defined)]
        [BrainProperty(true)] public string UniqueKey { get => _uniqueKey; set => _uniqueKey = value; }
        [BrainPropertyValueName("UniqueKey", typeof(IMonaVariablesStringValue))] public string UniqueKeyName { get => _uniqueKeyName; set => _uniqueKeyName = value; }

        [SerializeField] private float _saveSlot;
        [SerializeField] private string _saveSlotName;
        [BrainPropertyShow(nameof(SaveSlotUsage), (int)UsageType.Defined)]
        [BrainProperty(true)] public float SaveSlot { get => _saveSlot; set => _saveSlot = value; }
        [BrainPropertyValueName("SaveSlot", typeof(IMonaVariablesFloatValue))] public string SaveSlotName { get => _saveSlotName; set => _saveSlotName = value; }

        [SerializeField] private UsageType _addUniqueKey = UsageType.None;
        [BrainPropertyEnum(false)] public UsageType AddUniqueKey { get => _addUniqueKey; set => _addUniqueKey = value; }

        [SerializeField] private UsageType _saveSlotUsage = UsageType.None;
        [BrainPropertyEnum(false)] public UsageType SaveSlotUsage { get => _saveSlotUsage; set => _saveSlotUsage = value; }

        [SerializeField] private bool _saveNow = false;
        [SerializeField] private string _saveNowName;
        [BrainProperty(false)] public bool SaveNow { get => _saveNow; set => _saveNow = value; }
        [BrainPropertyValueName("SaveNow", typeof(IMonaVariablesBoolValue))] public string SaveNowName { get => _saveNowName; set => _saveNowName = value; }

        private List<IMonaBody> _bodiesToSave = new List<IMonaBody>();
        
        private const string _saveSlotFormatString = "<SaveSlot>{0}</SaveSlot>";
        private const string _uniqueKeyFormatString = "<UniqueKey>{0}</UniqueKey>";
        private const string _objectFormatString = "<GameObject>{0}</GameObject>";
        private const string _tagFormatString = "<Tag>{0}</Tag>";
        private const string _indexFormatString = "<Index>{0}</Index>";
        private const string _brainFormatString = "<Brain>{0}</Brain>";
        private const string _positionString = "<Position>";
        private const string _rotationString = "<Rotation>";
        private const string _scaleString = "<Scale>";

        private const string _x = "X";
        private const string _y = "Y";
        private const string _z = "Z";

        public StoreLayoutInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public enum UsageType
        {
            None,
            Defined
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_uniqueKeyName))
                _uniqueKey = _brain.Variables.GetString(_uniqueKeyName);

            if (!string.IsNullOrEmpty(_saveSlotName))
                _saveSlot = _brain.Variables.GetFloat(_saveSlotName);

            if (!string.IsNullOrEmpty(_saveNowName))
                _saveNow = _brain.Variables.GetBool(_saveNowName);

            _bodiesToSave.Clear();

            switch (_target)
            {
                case MonaBrainTargetLayoutType.Tag:
                    StoreLayoutOfTag();
                    break;
                case MonaBrainTargetLayoutType.ThisBodyOnly:
                    StoreBody(_brain.Body);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private void StoreLayoutOfTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                _bodiesToSave.Add(tagBodies[i]);
            }

            StoreBodies();
        }

        private void StoreBody(IMonaBody body)
        {
            if (body == null)
                return;

            StoreBodyTransforms(body);

            if (_saveNow)
                PlayerPrefs.Save();
        }

        private void StoreBodies()
        {
            for (int i = 0; i < _bodiesToSave.Count; i++)
                StoreBodyTransforms(_bodiesToSave[i], i);

            if (_saveNow)
                PlayerPrefs.Save();
        }

        private void StoreBodyTransforms(IMonaBody body, int index = -1)
        {
            if (body == null)
                return;

            string bodyString = GetFullBodyString(body, index);
            //string brainString = GetBrainsString(body);

            Vector3 position = body.GetPosition();
            Vector3 rotation = body.GetRotation().eulerAngles;
            Vector3 scale = body.GetScale();

            PlayerPrefs.SetString(bodyString, "[Exists]");
            StoreBodyVectorThree(bodyString, _positionString, position);
            StoreBodyVectorThree(bodyString, _rotationString, rotation);
            StoreBodyVectorThree(bodyString, _scaleString, scale);
        }

        private void StoreBodyVectorThree(string bodyString, string transformString, Vector3 value)
        {
            string key = bodyString + transformString;
            PlayerPrefs.SetFloat(key + _x, value.x);
            PlayerPrefs.SetFloat(key + _y, value.y);
            PlayerPrefs.SetFloat(key + _z, value.z);
        }

        private string GetFullBodyString(IMonaBody body, int index = -1)
        {
            if (body == null)
                return string.Empty;

            string saveSlotString = _saveSlotUsage == UsageType.Defined ?
                string.Format(_saveSlotFormatString, _saveSlot) : string.Empty;

            string uniqueKeyString = _addUniqueKey == UsageType.Defined ?
                string.Format(_uniqueKeyFormatString, _uniqueKey) : string.Empty;

            string indexString = index < 0 ?
                string.Empty : string.Format(_indexFormatString, index);

            string tagString = _target == MonaBrainTargetLayoutType.Tag ?
                string.Format(_tagFormatString, _targetTag.ToString()) : string.Empty;

            return saveSlotString + uniqueKeyString + tagString + indexString;
        }

        private string GetBodyGameObjectName(IMonaBody body)
        {
            if (body == null || body.Transform == null)
                return string.Empty;

            return string.Format(_objectFormatString, body.Transform.gameObject.name);
        }

        private string GetBrainsString(IMonaBody body)
        {
            string brainsString = string.Empty;

            if (body == null)
                return brainsString;

            MonaBrainRunner runner = body.Transform.GetComponent<MonaBrainRunner>();

            if (runner == null)
                return brainsString;

            for (int i = 0; i < runner.BrainGraphs.Count; i++)
            {
                if (runner.BrainGraphs[i] == null)
                    continue;

                brainsString += string.Format(_brainFormatString, runner.BrainGraphs[i].Name);
            }

            return brainsString;
        }
    }
}