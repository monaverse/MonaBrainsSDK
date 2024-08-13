using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class LayoutStorageBaseInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public override Type TileType => typeof(LayoutStorageBaseInstructionTile);

        [SerializeField] protected StorageTargetType _storageTarget = StorageTargetType.LocalAndCloud;
        [BrainPropertyEnum(true)] public StorageTargetType StorageTarget { get => _storageTarget; set => _storageTarget = value; }

        [SerializeField] protected MonaBrainTargetLayoutType _target = MonaBrainTargetLayoutType.AllBodies;
        [BrainPropertyEnum(true)] public MonaBrainTargetLayoutType Target { get => _target; set => _target = value; }

        [SerializeField] protected string _targetTag = "Default";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

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

        [SerializeField] protected DefinedBodyType _forceSameBody = DefinedBodyType.None;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ThisBodyOnly)]
        [BrainPropertyEnum(false)] public DefinedBodyType ForceSameBody { get => _forceSameBody; set => _forceSameBody = value; }

        [SerializeField] protected UsageType _saveSlotUsage = UsageType.None;
        [BrainPropertyEnum(false)] public UsageType SaveSlotUsage { get => _saveSlotUsage; set => _saveSlotUsage = value; }

        [SerializeField] protected string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        protected virtual bool UseBodyID
        {
            get
            {
                switch (_target)
                {
                    case MonaBrainTargetLayoutType.Tag:
                        return _forceSameBody != DefinedBodyType.None;
                    case MonaBrainTargetLayoutType.ThisBodyOnly:
                        return _forceSameBody != DefinedBodyType.None;
                }

                return true;
            }
        }

        protected const string _saveSlotFormatString = "<SaveSlot>{0}</SaveSlot>";
        protected const string _uniqueKeyFormatString = "<UniqueKey>{0}</UniqueKey>";
        protected const string _bodyIDString = "<BodyID>{0}</BodyID>";
        protected const string _tagFormatString = "<Tag>{0}</Tag>";
        protected const string _indexFormatString = "<Index>{0}</Index>";
        protected const string _brainFormatString = "<Brain>{0}</Brain>";
        protected const string _positionString = "<Position>";
        protected const string _rotationString = "<Rotation>";
        protected const string _scaleString = "<Scale>";
        protected const string _existsString = "[Exists]";
        protected const string _x = "X";
        protected const string _y = "Y";
        protected const string _z = "Z";

        public LayoutStorageBaseInstructionTile() { }

        protected bool _active;
        protected bool _isRunning;
        protected IMonaBrain _brain;
        protected MonaGlobalBrainRunner _globalBrainRunner;
        protected IBrainStorage _localStorage;
        protected IBrainStorage _cloudStorage;
        protected List<BrainProcess> _localProcesses = new List<BrainProcess>();
        protected List<BrainProcess> _cloudProcesses = new List<BrainProcess>();
        protected Action<MonaBodyFixedTickEvent> OnFixedTick;

        protected bool UseLocalStorage => _storageTarget != StorageTargetType.Cloud;
        protected bool UseCloudStorage => _storageTarget != StorageTargetType.Local;

        public virtual void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            SetActive(true);
        }

        public virtual void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        protected void UpdateActive()
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

        public virtual void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public virtual bool Resume()
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

        protected InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        protected InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        protected void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        protected void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        protected void LostControl()
        {
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        protected void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick();
        }

        protected virtual void FixedTick()
        {
            if (UseLocalStorage && (_localStorage == null || _localProcesses.Count < 1 || StillProcessing(_localProcesses)))
                return;

            if (UseCloudStorage && (_cloudStorage == null || _localProcesses.Count < 1 || StillProcessing(_cloudProcesses)))
                return;

            _isRunning = false;

            Complete(InstructionTileResult.Success, true);
        }

        protected bool StillProcessing(List<BrainProcess> processes)
        {
            bool processing = false;

            for (int i = 0; i < processes.Count; i++)
            {
                if (processes[i].IsProcessing)
                {
                    processing = true;
                    break;
                }
            }

            return processing;
        }

        public override InstructionTileResult Do()
        {
            return Complete(InstructionTileResult.Success);
        }

        protected virtual void SetNamedValues()
        {
            if (!string.IsNullOrEmpty(_uniqueKeyName))
                _uniqueKey = _brain.Variables.GetString(_uniqueKeyName);

            if (!string.IsNullOrEmpty(_saveSlotName))
                _saveSlot = _brain.Variables.GetFloat(_saveSlotName);
        }

        protected virtual string GetFullBodyString(IMonaBody body, int index = -1)
        {
            if (body == null)
                return string.Empty;

            string saveSlotString = _saveSlotUsage == UsageType.Defined ?
                string.Format(_saveSlotFormatString, _saveSlot) : string.Empty;

            string uniqueKeyString = _addUniqueKey == UsageType.Defined ?
                string.Format(_uniqueKeyFormatString, _uniqueKey) : string.Empty;

            string bodyID = UseBodyID ?
                GetBodyIDName(body) : string.Empty;

            string tagString = _target == MonaBrainTargetLayoutType.Tag ?
                string.Format(_tagFormatString, _targetTag.ToString()) : string.Empty;

            string indexString = index < 0 ?
                string.Empty : string.Format(_indexFormatString, index);

            return saveSlotString + uniqueKeyString + bodyID + tagString + indexString;
        }

        protected virtual string GetBodyIDName(IMonaBody body)
        {
            if (body == null || body.Transform == null)
                return string.Empty;

            string bodyID = _forceSameBody == DefinedBodyType.UseLocalID && _target != MonaBrainTargetLayoutType.AllBodies ?
                 body.LocalId : body.DurableId.ToString();

            return string.Format(_bodyIDString, bodyID);
        }

        protected virtual string GetBrainsString(IMonaBody body)
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

        protected virtual MonaBody[] GetAllBodies()
        {
            var globalBodies = GameObject.FindObjectsOfType<MonaBody>();
            return globalBodies;
        }
    }
}