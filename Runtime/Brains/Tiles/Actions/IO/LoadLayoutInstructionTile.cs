using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Structs;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class LoadLayoutInstructionTile : LayoutStorageBaseInstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "LoadLayout";
        public const string NAME = "Load Layout";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(LoadLayoutInstructionTile);

        private bool _loadProcessed;
        private List<IMonaBody> _bodiesToProcess = new List<IMonaBody>();
        public LoadLayoutInstructionTile() { }

        private enum TransformType
        {
            Position,
            Rotation,
            Scale
        }

        protected override void FixedTick()
        {
            if (UseLocalStorage && (_localStorage == null || _localProcesses.Count < 1 || StillProcessing(_localProcesses)))
                return;

            if (UseCloudStorage && (_cloudStorage == null || _localProcesses.Count < 1 || StillProcessing(_cloudProcesses)))
                return;

            bool anySuccessful = false;

            if (UseLocalStorage)
            {
                for (int i = 0; i < _localProcesses.Count; i++)
                {
                    if (_localProcesses[i].WasSuccessful)
                    {
                        anySuccessful = true;
                        LayoutStorageData layout = _localProcesses[i].GetLayout();
                        ApplyBodyVectors(layout);
                        _bodiesToProcess.Remove(layout.ReferenceBody);
                    }
                }
            }

            if (UseCloudStorage)
            {
                for (int i = 0; i < _cloudProcesses.Count; i++)
                {
                    if (_cloudProcesses[i].WasSuccessful)
                    {
                        anySuccessful = true;
                        LayoutStorageData layout = _localProcesses[i].GetLayout();

                        if (!_bodiesToProcess.Contains(layout.ReferenceBody))
                            continue;
                        
                        ApplyBodyVectors(layout);
                        _bodiesToProcess.Remove(layout.ReferenceBody);
                    }
                }
            }

            _isRunning = false;

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, anySuccessful);

            Complete(InstructionTileResult.Success, true);
        }

        private void ApplyBodyVectors(LayoutStorageData layout)
        {
            if (layout.Position.LoadSuccess)
                layout.ReferenceBody.TeleportPosition(layout.Position.Vector);
            if (layout.RotationEulers.LoadSuccess)
                layout.ReferenceBody.TeleportRotation(Quaternion.Euler(layout.RotationEulers.Vector));
            if (layout.Scale.LoadSuccess)
                layout.ReferenceBody.TeleportScale(layout.Scale.Vector);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _loadProcessed = false;

            if (!_isRunning)
            {
                _localProcesses.Clear();
                _cloudProcesses.Clear();

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

                SetNamedValues();

                ProcessLoad();

                if(!_loadProcessed)
                    AddFixedTickDelegate();
            }

            if (_localProcesses.Count > 0 || _cloudProcesses.Count > 0)
                return _loadProcessed ? Complete(InstructionTileResult.Success) : Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private async Task ProcessLoad()
        {
            switch (_target)
            {
                case MonaBrainTargetLayoutType.Tag:
                    await LoadLayoutOfTag();
                    break;
                case MonaBrainTargetLayoutType.AllBodies:
                    await LoadAllBodies();
                    break;
                case MonaBrainTargetLayoutType.ThisBodyOnly:
                    await ProcessBodyTransforms(_brain.Body);
                    break;
            }
            _loadProcessed = true;
        }

        private async Task LoadLayoutOfTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                await ProcessBodyTransforms(tagBodies[i], i);
            }
        }

        private async Task LoadAllBodies()
        {
            var globalBodies = GetAllBodies();

            for (int i = 0; i < globalBodies.Length; i++)
                await ProcessBodyTransforms(globalBodies[i]);
        }

        private async Task ProcessBodyTransforms(IMonaBody body, int index = -1)
        {
            if (body == null)
                return;

            string key = GetFullBodyString(body, index);
            _bodiesToProcess.Add(body);

            if (UseLocalStorage)
            {
                BrainProcess process = await _localStorage.LoadLayout(key, body);
                _localProcesses.Add(process);
            }

            if (UseCloudStorage)
            {
                BrainProcess process = await _cloudStorage.LoadLayout(key, body);
                _cloudProcesses.Add(process);
            }
        }
    }
}