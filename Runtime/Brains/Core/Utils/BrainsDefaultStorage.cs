using UnityEngine;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Core.Body;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Core.Utils
{
    public class BrainsDefaultStorage : MonoBehaviour, IBrainStorageAsync
    {
        private const string _x = "X";
        private const string _y = "Y";
        private const string _z = "Z";
        protected const string _positionString = "<Position>";
        protected const string _rotationString = "<Rotation>";
        protected const string _scaleString = "<Scale>";

        public bool Processing => false;
        public StorageTargetType SupportedStorageTarget => StorageTargetType.Local;

        public bool StorageEnabled => true;

        public async Task<BrainProcess> RegisterNewUser(string username, string password) { return new BrainProcess(ProcessStartType.AutoSucceed); }
        public async Task<BrainProcess> AutoLogin() { return new BrainProcess(ProcessStartType.AutoSucceed); }
        public async Task<BrainProcess> LoginUser(string username) { return new BrainProcess(ProcessStartType.AutoSucceed); }
        public async Task<BrainProcess> LoginUser(string username, string password) { return new BrainProcess(ProcessStartType.AutoSucceed); }
        public async Task<BrainProcess> LogoutCurrentUser() { return new BrainProcess(ProcessStartType.AutoSucceed); }
        public async Task<BrainProcess> ChangeCurrentUserPassword(string newPassword) { return new BrainProcess(ProcessStartType.AutoSucceed); }

        public async Task<BrainProcess> SetBool(string key, bool value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            int boolBinary = value ? 1 : 0;
            PlayerPrefs.SetInt(key, boolBinary);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetInt(string key, int value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            PlayerPrefs.SetInt(key, value);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetLong(string key, long value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            PlayerPrefs.SetString(key, value.ToString());

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetFloat(string key, float value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            PlayerPrefs.SetFloat(key, value);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetDouble(string key, double value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            PlayerPrefs.SetString(key, value.ToString());

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetString(string key, string value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetVector2(string key, Vector2 value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            PlayerPrefs.SetFloat(key + _x, value.x);
            PlayerPrefs.SetFloat(key + _y, value.y);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetVector3(string key, Vector3 value, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            StoreAnyVector3(key, value);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SetLayout(LayoutStorageData layout, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();
            StoreAnyVector3(layout.BaseKey + _positionString, layout.Position.Vector);
            StoreAnyVector3(layout.BaseKey + _rotationString, layout.RotationEulers.Vector);
            StoreAnyVector3(layout.BaseKey + _scaleString, layout.Scale.Vector);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        private void StoreAnyVector3(string key, Vector3 value)
        {
            PlayerPrefs.SetFloat(key + _x, value.x);
            PlayerPrefs.SetFloat(key + _y, value.y);
            PlayerPrefs.SetFloat(key + _z, value.z);
        }

        public async Task<BrainProcess> LoadBool(string key)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            int boolBinary = PlayerPrefs.GetInt(key);

            state.SetValue(boolBinary == 1);
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadInt(string key)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            state.SetValue(PlayerPrefs.GetInt(key));
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadLong(string key)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            state.SetValue(long.Parse(PlayerPrefs.GetString(key)));
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadFloat(string key)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            state.SetValue(PlayerPrefs.GetFloat(key));
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadDouble(string key)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            state.SetValue(double.Parse(PlayerPrefs.GetString(key)));
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadString(string key)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            state.SetValue(PlayerPrefs.GetString(key));
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadVector2(string key)
        {
            BrainProcess state = new BrainProcess();

            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);

            if (!xSuccess || !ySuccess)
            {
                state.EndProcess(false);
                return state;
            }

            float x = PlayerPrefs.GetFloat(key + _x);
            float y = PlayerPrefs.GetFloat(key + _y);

            state.SetValue(new Vector2(x, y));
            state.EndProcess(true);
            return state;
        }

        public async Task<BrainProcess> LoadVector3(string key)
        {
            BrainProcess state = new BrainProcess();
            state.SetValue(GetAnyVector3(key, out bool success));
            state.EndProcess(success);
            return state;
        }

        private Vector3 GetAnyVector3(string key, out bool success)
        {
            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);
            bool zSuccess = PlayerPrefs.HasKey(key + _z);

            if (!xSuccess || !ySuccess || !zSuccess)
            {
                success = false;
                return Vector3.zero;
            }

            float x = PlayerPrefs.GetFloat(key + _x);
            float y = PlayerPrefs.GetFloat(key + _y);
            float z = PlayerPrefs.GetFloat(key + _z);

            success = true;
            return new Vector3(x, y, z);
        }

        public async Task<BrainProcess> LoadLayout(string key, IMonaBody referenceBody)
        {
            BrainProcess state = new BrainProcess();

            Vector3 position = GetAnyVector3(key + _positionString, out bool successPosition);
            Vector3 rotation = GetAnyVector3(key + _rotationString, out bool successRotation);
            Vector3 scale = GetAnyVector3(key + _scaleString, out bool successScale);

            if (!successPosition && !successRotation && !successScale)
            {
                state.EndProcess(false);
                return state;
            }

            LayoutStorageData layout = new LayoutStorageData
            {
                BaseKey = key,
                ReferenceBody = referenceBody,
            };

            if (successPosition) layout.SetPosition(position);
            if (successRotation) layout.SetRotationEulers(rotation);
            if (successScale) layout.SetScale(scale);

            state.SetValue(layout);
            state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> DeleteBool(string key, bool saveChanges = true) { return await DeleteSimpleValue(key, saveChanges); }
        public async Task<BrainProcess> DeleteInt(string key, bool saveChanges = true) { return await DeleteSimpleValue(key, saveChanges); }
        public async Task<BrainProcess> DeleteLong(string key, bool saveChanges = true) { return await DeleteSimpleValue(key, saveChanges); }
        public async Task<BrainProcess> DeleteFloat(string key, bool saveChanges = true) { return await DeleteSimpleValue(key, saveChanges); }
        public async Task<BrainProcess> DeleteDouble(string key, bool saveChanges = true) { return await DeleteSimpleValue(key, saveChanges); }
        public async Task<BrainProcess> DeleteString(string key, bool saveChanges = true) { return await DeleteSimpleValue(key, saveChanges); }

        private async Task<BrainProcess> DeleteSimpleValue(string key, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            if (!PlayerPrefs.HasKey(key))
            {
                state.EndProcess(false);
                return state;
            }

            PlayerPrefs.DeleteKey(key);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> DeleteVector2(string key, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);

            if (!xSuccess || !ySuccess)
            {
                state.EndProcess(false);
                return state;
            }

            PlayerPrefs.DeleteKey(key + _x);
            PlayerPrefs.DeleteKey(key + _y);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> DeleteVector3(string key, bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);
            bool zSuccess = PlayerPrefs.HasKey(key + _z);

            if (!xSuccess || !ySuccess || !zSuccess)
            {
                state.EndProcess(false);
                return state;
            }

            PlayerPrefs.DeleteKey(key + _x);
            PlayerPrefs.DeleteKey(key + _y);
            PlayerPrefs.DeleteKey(key + _z);

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> DeleteAllData(bool saveChanges = true)
        {
            BrainProcess state = new BrainProcess();

            PlayerPrefs.DeleteAll();

            if (saveChanges) state = await SaveChanges();
            else state.EndProcess(true);

            return state;
        }

        public async Task<BrainProcess> SaveChanges()
        {
            PlayerPrefs.Save();
            return new BrainProcess(ProcessStartType.AutoSucceed);
        }
    }
}