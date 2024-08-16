using UnityEngine;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainStorage
    {
        /// <summary>
        /// The StorageTargetType supported by the implementation of
        /// IBrainStorage.
        /// </summary>
        StorageTargetType SupportedStorageTarget { get; }

        BrainProcess SetBool(string key, bool value, bool saveChanges = true);
        BrainProcess SetInt(string key, int value, bool saveChanges = true);
        BrainProcess SetLong(string key, long value, bool saveChanges = true);
        BrainProcess SetFloat(string key, float value, bool saveChanges = true);
        BrainProcess SetDouble(string key, double value, bool saveChanges = true);
        BrainProcess SetString(string key, string value, bool saveChanges = true);
        BrainProcess SetVector2(string key, Vector2 value, bool saveChanges = true);
        BrainProcess SetVector3(string key, Vector3 value, bool saveChanges = true);
        BrainProcess SetLayout(LayoutStorageData layout, bool saveChanges = true);

        BrainProcess LoadBool(string key);
        BrainProcess LoadInt(string key);
        BrainProcess LoadLong(string key);
        BrainProcess LoadFloat(string key);
        BrainProcess LoadDouble(string key);
        BrainProcess LoadString(string key);
        BrainProcess LoadVector2(string key);
        BrainProcess LoadVector3(string key);
        BrainProcess LoadLayout(string key, IMonaBody referenceBody);

        BrainProcess DeleteBool(string key, bool saveChanges = true);
        BrainProcess DeleteInt(string key, bool saveChanges = true);
        BrainProcess DeleteLong(string key, bool saveChanges = true);
        BrainProcess DeleteFloat(string key, bool saveChanges = true);
        BrainProcess DeleteDouble(string key, bool saveChanges = true);
        BrainProcess DeleteString(string key, bool saveChanges = true);
        BrainProcess DeleteVector2(string key, bool saveChanges = true);
        BrainProcess DeleteVector3(string key, bool saveChanges = true);

        BrainProcess DeleteAllData(bool saveChanges = true);
        BrainProcess SaveChanges();
    }
}