using UnityEngine;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Structs;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IBrainStorageAsync : IBrainSocialPlatformUserAsync
    {
        bool StorageEnabled { get; }

        /// <summary>
        /// The StorageTargetType supported by the implementation of
        /// IBrainStorage.
        /// </summary>
        StorageTargetType SupportedStorageTarget { get; }

        Task<BrainProcess> SetBool(string key, bool value, bool saveChanges = true);
        Task<BrainProcess> SetInt(string key, int value, bool saveChanges = true);
        Task<BrainProcess> SetLong(string key, long value, bool saveChanges = true);
        Task<BrainProcess> SetFloat(string key, float value, bool saveChanges = true);
        Task<BrainProcess> SetDouble(string key, double value, bool saveChanges = true);
        Task<BrainProcess> SetString(string key, string value, bool saveChanges = true);
        Task<BrainProcess> SetVector2(string key, Vector2 value, bool saveChanges = true);
        Task<BrainProcess> SetVector3(string key, Vector3 value, bool saveChanges = true);
        Task<BrainProcess> SetLayout(LayoutStorageData layout, bool saveChanges = true);

        Task<BrainProcess> LoadBool(string key);
        Task<BrainProcess> LoadInt(string key);
        Task<BrainProcess> LoadLong(string key);
        Task<BrainProcess> LoadFloat(string key);
        Task<BrainProcess> LoadDouble(string key);
        Task<BrainProcess> LoadString(string key);
        Task<BrainProcess> LoadVector2(string key);
        Task<BrainProcess> LoadVector3(string key);
        Task<BrainProcess> LoadLayout(string key, IMonaBody referenceBody);

        Task<BrainProcess> DeleteBool(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteInt(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteLong(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteFloat(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteDouble(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteString(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteVector2(string key, bool saveChanges = true);
        Task<BrainProcess> DeleteVector3(string key, bool saveChanges = true);

        Task<BrainProcess> DeleteAllData(bool saveChanges = true);
        Task<BrainProcess> SaveChanges();
    }
}