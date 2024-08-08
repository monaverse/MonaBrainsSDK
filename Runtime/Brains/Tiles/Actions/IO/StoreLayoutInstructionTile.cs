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
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

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
            var tagBodies = MonaBodyFactory.FindByTag(_targetTag);

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

            TrySave();
        }

        private void StoreBody(IMonaBody body)
        {
            if (body == null)
                return;

            StoreBodyTransforms(body);
            TrySave();
        }

        private void StoreBodies()
        {
            for (int i = 0; i < _bodiesToSave.Count; i++)
                StoreBodyTransforms(_bodiesToSave[i], i);

            TrySave();
        }

        private void StoreBodyTransforms(IMonaBody body, int index = -1)
        {
            if (body == null)
                return;

            string bodyString = GetFullBodyString(body, index);

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

        private void TrySave()
        {
            if (_saveNow)
                PlayerPrefs.Save();
        }
    }
}