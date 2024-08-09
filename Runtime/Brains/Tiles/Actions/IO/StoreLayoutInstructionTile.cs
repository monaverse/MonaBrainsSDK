using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class StoreLayoutInstructionTile : LayoutStorageBaseInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
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

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

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

            SetNamedValues();

            if (!string.IsNullOrEmpty(_saveNowName))
                _saveNow = _brain.Variables.GetBool(_saveNowName);

            _bodiesToSave.Clear();

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

        private void StoreAllBodies()
        {
            var globalBodies = GetAllBodies();

            for (int i = 0; i < globalBodies.Length; i++)
                StoreBodyTransforms(globalBodies[i]);
        }

        private void StoreBody(IMonaBody body)
        {
            if (body == null)
                return;

            StoreBodyTransforms(body);
        }

        private void StoreBodies()
        {
            for (int i = 0; i < _bodiesToSave.Count; i++)
                StoreBodyTransforms(_bodiesToSave[i], i);
        }

        private void StoreBodyTransforms(IMonaBody body, int index = -1)
        {
            if (body == null)
                return;

            string bodyString = GetFullBodyString(body, index);

            Vector3 position = body.GetPosition();
            Vector3 rotation = body.GetRotation().eulerAngles;
            Vector3 scale = body.GetScale();

            if (_storageTarget != StorageTargetType.Cloud)
            {
                _localStorage.SetString(bodyString, _existsString, out bool _, _saveNow);
                StoreBodyVectorThree(_localStorage, bodyString, _positionString, position);
                StoreBodyVectorThree(_localStorage, bodyString, _rotationString, rotation);
                StoreBodyVectorThree(_localStorage, bodyString, _scaleString, scale);
            }

            if (_storageTarget != StorageTargetType.Local)
            {
                _cloudStorage.SetString(bodyString, _existsString, out bool _, _saveNow);
                StoreBodyVectorThree(_cloudStorage, bodyString, _positionString, position);
                StoreBodyVectorThree(_cloudStorage, bodyString, _rotationString, rotation);
                StoreBodyVectorThree(_cloudStorage, bodyString, _scaleString, scale);
            }
        }

        private void StoreBodyVectorThree(IBrainStorage storage, string bodyString, string transformString, Vector3 value)
        {
            string key = bodyString + transformString;
            storage.SetVector3(key, value, out bool _, _saveNow);
        }
    }
}