using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.EasyUI.Leaderboards
{
    public class LeaderboardDisplayController : MonoBehaviour
    {
        [SerializeField] private GameObject _leaderboardObject;
        [SerializeField] private string _fallbackPrefabName = "Leaderboard_Root";

        private LeaderboardWindow _window;
        public LeaderboardWindow Window => _window;

        private static LeaderboardDisplayController _instance;
        public static LeaderboardDisplayController Instance => _instance;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else Destroy(this);

            InitializeLeaderboard();
        }

        private void InitializeLeaderboard()
        {
            if (_leaderboardObject == null)
            {
                var window = GameObject.FindAnyObjectByType<LeaderboardWindow>(FindObjectsInactive.Include);

                if (window == null)
                {
                    GameObject leaderboardPrefab = Resources.Load<GameObject>(_fallbackPrefabName);
                    _leaderboardObject = Instantiate(leaderboardPrefab);

                    if (_leaderboardObject == null)
                        return;
                }
                else
                {
                    _window = window;
                    _leaderboardObject = window.gameObject;
                }
            }

            if (_leaderboardObject == null)
                return;

            if (_window == null)
            {
                _window = _leaderboardObject.GetComponent<LeaderboardWindow>();

                if (_window == null)
                    return;
            }

            _window.InitializeWindow();
        }
    }
}
