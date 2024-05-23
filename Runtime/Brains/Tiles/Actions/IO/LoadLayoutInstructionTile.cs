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
    public class LoadLayoutInstructionTile : LayoutStorageBaseInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "LoadLayout";
        public const string NAME = "Load Layout";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(LoadLayoutInstructionTile);

        private List<IMonaBody> _bodiesToLoad = new List<IMonaBody>();

        public LoadLayoutInstructionTile() { }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

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

            string positionString = bodyKey + _positionString;
            string rotationString = bodyKey + _rotationString;
            string scaleString = bodyKey + _scaleString;

            if (PlayerPrefs.HasKey(positionString + _x))
                body.TeleportPosition(LoadBodyVectorThree(positionString));

            if (PlayerPrefs.HasKey(rotationString + _x))
                body.TeleportRotation(Quaternion.Euler(LoadBodyVectorThree(rotationString)));

            if (PlayerPrefs.HasKey(scaleString + _x))
                body.TeleportScale(LoadBodyVectorThree(scaleString));
        }

        private Vector3 LoadBodyVectorThree(string key)
        {
            float x = PlayerPrefs.GetFloat(key + _x, 1);
            float y = PlayerPrefs.GetFloat(key + _y, 1);
            float z = PlayerPrefs.GetFloat(key + _z, 1);

            return new Vector3(x, y, z);
        }
    }
}