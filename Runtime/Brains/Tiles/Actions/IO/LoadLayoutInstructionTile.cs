using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class LoadLayoutInstructionTile : LayoutStorageBaseInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "LoadLayout";
        public const string NAME = "Load Layout";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(LoadLayoutInstructionTile);

        public LoadLayoutInstructionTile() { }

        private enum TransformType
        {
            Position,
            Rotation,
            Scale
        }

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

           switch (_target)
            {
                case MonaBrainTargetLayoutType.Tag:
                    LoadLayoutOfTag();
                    break;
                case MonaBrainTargetLayoutType.AllBodies:
                    LoadAllBodies();
                    break;
                case MonaBrainTargetLayoutType.ThisBodyOnly:
                    TryLoadBody(_brain.Body);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private void LoadLayoutOfTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (!TryLoadBody(tagBodies[i], i))
                    break;
            }
        }

        private void LoadAllBodies()
        {
            var globalBodies = GetAllBodies();

            for (int i = 0; i < globalBodies.Length; i++)
                TryLoadBody(globalBodies[i]);
        }

        private bool TryLoadBody(IMonaBody body, int index = -1)
        {
            if (body == null)
                return false;

            string bodyKey = GetFullBodyString(body, index);

            if (!PlayerPrefs.HasKey(bodyKey))
                return false;

            SetBodyTransforms(body, bodyKey);

            return true;
        }

        private void SetBodyTransforms(IMonaBody body, string bodyKey)
        {
            if (body == null)
                return;

            bool localSuccess = false;

            if (_storageTarget != StorageTargetType.Cloud)
            {
                SetTrasformsFromStorage(body, _localStorage, TransformType.Position, bodyKey, out bool positionSuccess);
                SetTrasformsFromStorage(body, _localStorage, TransformType.Position, bodyKey, out bool rotationSuccess);
                SetTrasformsFromStorage(body, _localStorage, TransformType.Position, bodyKey, out bool scaleSuccess);
                localSuccess = positionSuccess || rotationSuccess || scaleSuccess;
            }

            if (_storageTarget != StorageTargetType.Local && !localSuccess)
            {
                SetTrasformsFromStorage(body, _localStorage, TransformType.Position, bodyKey, out _);
                SetTrasformsFromStorage(body, _localStorage, TransformType.Position, bodyKey, out _);
                SetTrasformsFromStorage(body, _localStorage, TransformType.Position, bodyKey, out _);
            }
        }

        private void SetTrasformsFromStorage(IMonaBody body, IBrainStorage storage, TransformType type, string bodyKey, out bool success)
        {
            switch (type)
            {
                case TransformType.Position:
                    Vector3 position = storage.LoadVector3(bodyKey + _positionString, out success);
                    if (success) body.TeleportPosition(position);
                    break;
                case TransformType.Rotation:
                    Vector3 rotation = storage.LoadVector3(bodyKey + _rotationString, out success);
                    if (success) body.TeleportPosition(rotation);
                    break;
                case TransformType.Scale:
                    Vector3 scale = storage.LoadVector3(bodyKey + _scaleString, out success);
                    if (success) body.TeleportPosition(scale);
                    break;
            }

            success = false;
        }
    }
}