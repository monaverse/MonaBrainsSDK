using UnityEngine;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Core.Utils
{
    [System.Serializable]
    public enum ProcessStartType
    {
        Default = 0,
        AutoSucceed = 10,
        AutoFail = 20
    }

    public class BrainProcess
    {
        private string _name;

        private bool _processStarted = false;
        private bool _processing = false;
        private bool _processSuccess = false;

        private bool _bool;
        private int _int;
        private long _long;
        private float _float;
        private double _double;
        private string _string;
        private Vector3 _vector2;
        private Vector3 _vector3;
        private LayoutStorageData _layout;
        private LeaderboardScore _userScore = new LeaderboardScore();
        private List<LeaderboardScore> _scores = new List<LeaderboardScore>();

        public string Name { get => _name; set => _name = value; }
        public bool IsProcessing => _processing;
        public bool WasSuccessful => HasCompleted() && _processSuccess;

        public BrainProcess(ProcessStartType startType = ProcessStartType.Default)
        {
            switch (startType)
            {
                case ProcessStartType.Default:
                    StartProcess();
                    break;
                case ProcessStartType.AutoSucceed:
                    _processSuccess = true;
                    break;
            }

            _processStarted = true;
        }

        public void StartProcess()
        {
            _processing = true;
            _processSuccess = false;
            _processStarted = true;
            _userScore = new LeaderboardScore();
            _scores.Clear();
        }

        public void RestartProcess()
        {
            StartProcess();
        }

        public void EndProcess(bool success)
        {
            _processSuccess = success;
            _processing = false;
        }

        public bool HasCompleted()
        {
            return _processStarted && !_processing;
        }

        public void AddScore(LeaderboardScore score)
        {
            _scores.Add(score);
        }

        public void SetLayoutReferences(IMonaBody body, string key)
        {
            _layout.ReferenceBody = body;
            _layout.BaseKey = key;
        }

        public void SetValue(bool value) { _bool = value; }
        public void SetValue(int value) { _int = value; }
        public void SetValue(long value) { _long = value; }
        public void SetValue(float value) { _float = value; }
        public void SetValue(double value) { _double = value; }
        public void SetValue(string value) { _string = value; }
        public void SetValue(Vector2 value) { _vector2 = value; }
        public void SetValue(Vector3 value) { _vector3 = value; }
        public void SetValue(LayoutStorageData value) { _layout = value; }
        public void SetUserScore(LeaderboardScore score) { _userScore = score; }

        public bool GetBool() { return _bool; }
        public int GetInt() { return _int; }
        public long GetLong() { return _long; }
        public float GetFloat() { return _float; }
        public double GetDouble() { return _double; }
        public string GetString() { return _string; }
        public Vector2 GetVector2() { return _vector2; }
        public Vector3 GetVector3() { return _vector3; }
        public LayoutStorageData GetLayout() { return _layout; }
        public LeaderboardScore GetUserScore() { return _userScore; }
        public List<LeaderboardScore> GetScores() { return _scores; }
    }
}