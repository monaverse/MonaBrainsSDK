using UnityEngine;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Core.Utils
{
    public class BrainsDefaultStorage : MonoBehaviour, IBrainStorage
    {
        private const string _x = "X";
        private const string _y = "Y";
        private const string _z = "Z";

        public StorageTargetType SupportedStorageTarget => StorageTargetType.Local;

        public void SetBool(string key, bool value, out bool success, bool saveChanges = true)
        {
            int boolBinary = value ? 1 : 0;
            PlayerPrefs.SetInt(key, boolBinary);

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetInt(string key, int value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetInt(key, value);

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetLong(string key, long value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetString(key, value.ToString());

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetFloat(string key, float value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetFloat(key, value);

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetDouble(string key, double value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetString(key, value.ToString());

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetString(string key, string value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetString(key, value);

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetVector2(string key, Vector2 value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetFloat(key + _x, value.x);
            PlayerPrefs.SetFloat(key + _y, value.y);

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public void SetVector3(string key, Vector3 value, out bool success, bool saveChanges = true)
        {
            PlayerPrefs.SetFloat(key + _x, value.x);
            PlayerPrefs.SetFloat(key + _y, value.y);
            PlayerPrefs.SetFloat(key + _z, value.z);

            if (saveChanges)
                SaveChanges();

            success = true;
        }

        public bool LoadBool(string key, out bool success)
        {
            success = PlayerPrefs.HasKey(key);

            if (!success)
                return false;

            int boolBinary = PlayerPrefs.GetInt(key);
            return boolBinary == 1;
        }

        public int LoadInt(string key, out bool success)
        {
            success = PlayerPrefs.HasKey(key);
            return success ? PlayerPrefs.GetInt(key) : 0;
        }

        public long LoadLong(string key, out bool success)
        {
            success = PlayerPrefs.HasKey(key);
            return success ? long.Parse(PlayerPrefs.GetString(key)) : 0;
        }

        public float LoadFloat(string key, out bool success)
        {
            success = PlayerPrefs.HasKey(key);
            return success ? PlayerPrefs.GetFloat(key) : 0f;
        }

        public double LoadDouble(string key, out bool success)
        {
            success = PlayerPrefs.HasKey(key);
            return success ? double.Parse(PlayerPrefs.GetString(key)) : 0d;
        }

        public string LoadString(string key, out bool success)
        {
            success = PlayerPrefs.HasKey(key);
            return success ? PlayerPrefs.GetString(key) : string.Empty;
        }

        public Vector2 LoadVector2(string key, out bool success)
        {
            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);

            success = xSuccess && ySuccess;

            if (!success)
                return Vector2.zero;

            float x = PlayerPrefs.GetFloat(key + _x);
            float y = PlayerPrefs.GetFloat(key + _y);

            return new Vector2(x, y);
        }

        public Vector3 LoadVector3(string key, out bool success)
        {
            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);
            bool zSuccess = PlayerPrefs.HasKey(key + _z);

            success = xSuccess && ySuccess && zSuccess;

            if (!success)
                return Vector3.zero;

            float x = PlayerPrefs.GetFloat(key + _x);
            float y = PlayerPrefs.GetFloat(key + _y);
            float z = PlayerPrefs.GetFloat(key + _z);

            return new Vector3(x, y, z);
        }

        public void DeleteBool(string key, out bool success, bool saveChanges = true) { DeleteSimpleValue(key, out success, saveChanges); }
        public void DeleteInt(string key, out bool success, bool saveChanges = true) { DeleteSimpleValue(key, out success, saveChanges); }
        public void DeleteLong(string key, out bool success, bool saveChanges = true) { DeleteSimpleValue(key, out success, saveChanges); }
        public void DeleteFloat(string key, out bool success, bool saveChanges = true) { DeleteSimpleValue(key, out success, saveChanges); }
        public void DeleteDouble(string key, out bool success, bool saveChanges = true) { DeleteSimpleValue(key, out success, saveChanges); }
        public void DeleteString(string key, out bool success, bool saveChanges = true) { DeleteSimpleValue(key, out success, saveChanges); }

        private void DeleteSimpleValue(string key, out bool success, bool saveChanges = true)
        {
            success = PlayerPrefs.HasKey(key);

            if (!success)
                return;

            PlayerPrefs.DeleteKey(key);

            if (saveChanges)
                SaveChanges();
        }

        public void DeleteVector2(string key, out bool success, bool saveChanges = true)
        {
            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);

            success = xSuccess && ySuccess;

            if (!success)
                return;

            PlayerPrefs.DeleteKey(key + _x);
            PlayerPrefs.DeleteKey(key + _y);

            if (saveChanges)
                SaveChanges();
        }

        public void DeleteVector3(string key, out bool success, bool saveChanges = true)
        {
            bool xSuccess = PlayerPrefs.HasKey(key + _x);
            bool ySuccess = PlayerPrefs.HasKey(key + _y);
            bool zSuccess = PlayerPrefs.HasKey(key + _z);

            success = xSuccess && ySuccess && zSuccess;

            if (!success)
                return;

            PlayerPrefs.DeleteKey(key + _x);
            PlayerPrefs.DeleteKey(key + _y);
            PlayerPrefs.DeleteKey(key + _z);

            if (saveChanges)
                SaveChanges();
        }

        public void DeleteAllData(out bool success)
        {
            PlayerPrefs.DeleteAll();
            success = true;
        }

        private void SaveChanges()
        {
            SaveChanges(out _);
        }

        public void SaveChanges(out bool success)
        {
            PlayerPrefs.Save();
            success = true;
        }
    }
}