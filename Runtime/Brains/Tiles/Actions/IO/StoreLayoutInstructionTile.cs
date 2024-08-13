using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class StoreLayoutInstructionTile : LayoutStorageBaseInstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "StoreLayout";
        public const string NAME = "Store Layout";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(StoreLayoutInstructionTile);

        [SerializeField] private bool _saveNow = false;
        [SerializeField] private string _saveNowName;
        [BrainProperty(false)] public bool SaveNow { get => _saveNow; set => _saveNow = value; }
        [BrainPropertyValueName("SaveNow", typeof(IMonaVariablesBoolValue))] public string SaveNowName { get => _saveNowName; set => _saveNowName = value; }

        private List<IMonaBody> _bodiesToSave = new List<IMonaBody>();

        public StoreLayoutInstructionTile() { }

        protected override void FixedTick()
        {
            if (UseLocalStorage && (_localStorage == null || _localProcesses.Count < 1 || StillProcessing(_localProcesses)))
                return;

            if (UseCloudStorage && (_cloudStorage == null || _localProcesses.Count < 1 || StillProcessing(_cloudProcesses)))
                return;

            _isRunning = false;

            bool anySuccessful = false;

            if (UseLocalStorage)
            {
                for (int i = 0; i < _localProcesses.Count; i++)
                {
                    if (_localProcesses[i].WasSuccessful)
                    {
                        anySuccessful = true;
                        break;
                    }
                }
            }

            if (UseCloudStorage && !anySuccessful)
            {
                for (int i = 0; i < _cloudProcesses.Count; i++)
                {
                    if (_cloudProcesses[i].WasSuccessful)
                    {
                        anySuccessful = true;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, anySuccessful);

            Complete(InstructionTileResult.Success, true);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!_isRunning)
            {
                _localProcesses.Clear();
                _cloudProcesses.Clear();
                _bodiesToSave.Clear();

                if (!string.IsNullOrEmpty(_saveNowName))
                    _saveNow = _brain.Variables.GetBool(_saveNowName);

                if (UseLocalStorage && _localStorage == null)
                {
                    _localStorage = _globalBrainRunner.LocalStorage;

                    if (_localStorage == null)
                        return Complete(InstructionTileResult.Success);
                }

                if (UseCloudStorage && _cloudStorage == null)
                {
                    _cloudStorage = _globalBrainRunner.ClousStorage;

                    if (_cloudStorage == null)
                        return Complete(InstructionTileResult.Success);
                }

                SetNamedValues();

                switch (_target)
                {
                    case MonaBrainTargetLayoutType.Tag:
                        StoreLayoutOfTag();
                        break;
                    case MonaBrainTargetLayoutType.AllBodies:
                        StoreAllBodies();
                        break;
                    case MonaBrainTargetLayoutType.ThisBodyOnly:
                        StoreBody(_brain.Body);
                        break;
                }

                AddFixedTickDelegate();
            }

            if (_localProcesses.Count > 0 || _cloudProcesses.Count > 0)
                return Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
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

        private void StoreAllBodies()
        {
            var globalBodies = GetAllBodies();

            for (int i = 0; i < globalBodies.Length; i++)
                ProcessBodyTransforms(globalBodies[i]);
        }

        private void StoreBody(IMonaBody body)
        {
            if (body == null)
                return;

            ProcessBodyTransforms(body);
        }

        private void StoreBodies()
        {
            for (int i = 0; i < _bodiesToSave.Count; i++)
                ProcessBodyTransforms(_bodiesToSave[i], i);
        }

        private void ProcessBodyTransforms(IMonaBody body, int index = -1)
        {
            if (body == null)
                return;

            string key = GetFullBodyString(body, index);
            Vector3 position = body.GetPosition();
            Vector3 rotation = body.GetRotation().eulerAngles;
            Vector3 scale = body.GetScale();

            LayoutStorageData layout = new LayoutStorageData();
            layout.BaseKey = key;
            layout.ReferenceBody = body;
            layout.SetPosition(position);
            layout.SetRotationEulers(rotation);
            layout.SetScale(scale);

            if (UseLocalStorage)
            {
                BrainProcess process = _localStorage.SetLayout(layout, _saveNow);
                _localProcesses.Add(process);
            }

            if (UseCloudStorage)
            {
                BrainProcess process = _cloudStorage.SetLayout(layout, _saveNow);
                _cloudProcesses.Add(process);
            }
        }
    }
}