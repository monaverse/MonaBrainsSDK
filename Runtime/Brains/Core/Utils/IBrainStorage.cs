using UnityEngine;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainStorage
    {
        /// <summary>
        /// The StorageTargetType supported by the implementation of
        /// IBrainStorage.
        /// </summary>
        StorageTargetType SupportedStorageTarget { get; }

        void SetBool(string key, bool value, out bool success, bool saveChanges = true);
        void SetInt(string key, int value, out bool success, bool saveChanges = true);
        void SetLong(string key, long value, out bool success, bool saveChanges = true);
        void SetFloat(string key, float value, out bool success, bool saveChanges = true);
        void SetDouble(string key, double value, out bool success, bool saveChanges = true);
        void SetString(string key, string value, out bool success, bool saveChanges = true);
        void SetVector2(string key, Vector2 value, out bool success, bool saveChanges = true);
        void SetVector3(string key, Vector3 value, out bool success, bool saveChanges = true);

        bool LoadBool(string key, out bool success);
        int LoadInt(string key, out bool success);
        long LoadLong(string key, out bool success);
        float LoadFloat(string key, out bool success);
        double LoadDouble(string key, out bool success);
        string LoadString(string key, out bool success);
        Vector2 LoadVector2(string key, out bool success);
        Vector3 LoadVector3(string key, out bool success);

        void DeleteBool(string key, out bool success, bool saveChanges = true);
        void DeleteInt(string key, out bool success, bool saveChanges = true);
        void DeleteLong(string key, out bool success, bool saveChanges = true);
        void DeleteFloat(string key, out bool success, bool saveChanges = true);
        void DeleteDouble(string key, out bool success, bool saveChanges = true);
        void DeleteString(string key, out bool success, bool saveChanges = true);
        void DeleteVector2(string key, out bool success, bool saveChanges = true);
        void DeleteVector3(string key, out bool success, bool saveChanges = true);

        void DeleteAllData(out bool success);
        void SaveChanges(out bool success);
    }
}