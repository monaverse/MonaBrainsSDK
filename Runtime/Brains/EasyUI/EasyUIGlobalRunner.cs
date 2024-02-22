using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using Mona.SDK.Core.State;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Core.EasyUI;

namespace Mona.SDK.Brains.EasyUI
{
    public class EasyUIGlobalRunner : MonoBehaviour
    {
        [SerializeField] private GameObject _primaryScreenRoot;

        private MonaGlobalBrainRunner _globalBrainRunner;
        private EasyUIScreenDefinitions _rootScreenDefinitions;
        private List<IMonaVariablesValue> _displayableVariables;

        private EasyUIScreenDefinitions PrimaryScreenDefinitions => _rootScreenDefinitions ? _rootScreenDefinitions.GetComponent<EasyUIScreenDefinitions>() : null;

        private static EasyUIGlobalRunner _instance;

        public static EasyUIGlobalRunner Instance
        {
            get
            {
                Init();
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public static void Init()
        {
            if (_instance == null)
            {
                var existing = GameObject.FindObjectOfType<EasyUIGlobalRunner>();
                if (existing != null)
                {
                    existing.Awake();
                }
                else
                {
                    var go = new GameObject();
                    var runner = go.AddComponent<EasyUIGlobalRunner>();
                    go.name = nameof(EasyUIGlobalRunner);
                    go.transform.SetParent(GameObject.FindWithTag(MonaCoreConstants.TAG_SPACE)?.transform);
                    runner.Awake();
                }
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                Instance = this;
            }

            _displayableVariables = new List<IMonaVariablesValue>();
        }

        private void Start()
        {
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            _globalBrainRunner.OnStarted += HandleGlobalBrainRunnerStarted;
            SetupScreenUI();
        }

        private void SetupScreenUI()
        {
            if (!_primaryScreenRoot)
            {
                Debug.LogWarning("WARNING: The prefab for the 'primaryScreenRoot' is null or missing!");
                return;
            }

            GameObject rootObject = Instantiate(_primaryScreenRoot, gameObject.transform);
            _rootScreenDefinitions = rootObject.GetComponent<EasyUIScreenDefinitions>();

            if (!_rootScreenDefinitions)
            {
                Debug.LogWarning("WARNING: The prefab for 'primaryScreenRoot' does not contain the 'EasyUIScreenDefinitions' component!");
                return;
            }
        }

        private void OnDestroy()
        {
            if (_globalBrainRunner != null)
            {
                _globalBrainRunner.OnStarted -= HandleGlobalBrainRunnerStarted;
                _globalBrainRunner.OnBrainsChanged -= HandleBrainsChanged;
            }
        }

        private void HandleGlobalBrainRunnerStarted()
        {
            _globalBrainRunner.OnBrainsChanged += HandleBrainsChanged;
            HandleBrainsChanged();
        }

        private void HandleBrainsChanged()
        {
            GetVariablesFromBrains();
            SetupUIElements();
        }

        private void GetVariablesFromBrains()
        {
            if (!_globalBrainRunner)
            {
                _globalBrainRunner = MonaGlobalBrainRunner.Instance;
                return;
            }

            foreach (MonaBrainGraph brainGraph in _globalBrainRunner.Brains)
                GetDisplayReadyVariables(brainGraph);
        }

        private void GetDisplayReadyVariables(MonaBrainGraph brainGraph)
        {
            foreach (IMonaVariablesValue variable in brainGraph.DefaultVariables.VariableList)
            {
                if (variable.GetType() != typeof(MonaVariablesFloat) && !_displayableVariables.Contains(variable))
                    continue;

                if (((IEasyUINumericalDisplay)variable).AllowUIDisplay)
                    _displayableVariables.Add(variable);
            }
        }

        private void SetupUIElements()
        {
            if (_displayableVariables.Count == 0 || !PrimaryScreenDefinitions)
                return;

            foreach (IEasyUINumericalDisplay variable in _displayableVariables)
            {
                switch (variable.DisplaySpace)
                {
                    case EasyUIDisplaySpace.HeadsUpDisplay:
                        PrimaryScreenDefinitions.PlaceElementInHUD(variable);
                        break;
                }
            }
        }
    }
}

