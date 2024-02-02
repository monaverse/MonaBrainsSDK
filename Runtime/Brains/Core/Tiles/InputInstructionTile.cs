using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Input;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.Input.Interfaces;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Tiles
{
    [Serializable]
    public abstract class InputInstructionTile : InstructionTile, IInputInstructionTile, IInstructionTileWithPreload, IOnInteractInstructionTile, IConditionInstructionTile,
        IStartableInstructionTile, IActivateInstructionTile, IPauseableInstructionTile
    {
        protected abstract MonaInputState GetInputState();

        private PlayerInput _inputManager;
        protected Inputs _localInputs;

        private Action<MonaTickEvent> OnTick;
        private Action<MonaInputEvent> OnInput;

        private bool _active;

        protected MonaInputState _currentLocalInputState;

        protected IMonaBrain _brain;

        public const float DEAD_ZONE = .1f;

        protected abstract void ProcessLocalInput();
        protected abstract void HandleBodyInput(MonaInputEvent evt);

        public bool PlayerTriggered => true;

        public virtual void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            ConfigureInput();

            UpdateActive();
        }

        private void ConfigureInput()
        {
            _inputManager = MonaGlobalBrainRunner.Instance.GetPlayerInput();
            _inputManager.onDeviceLost += OnDeviceLost;
            _inputManager.onDeviceRegained += OnDeviceRegained;

            _localInputs = new Inputs();
            _localInputs.Player.Enable();

            _inputManager.ActivateInput();
        }

        protected void SetLocalInput(IMonaLocalInput input)
        {
            _brain.Body.SetLocalInput(input);
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                if (_brain != null)
                    UpdateActive();
            }
        }

        private void AddTickDelegate()
        {
            OnTick = HandleTick;
            EventBus.Register<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), OnTick);

            OnInput = HandleBodyInput;
            EventBus.Register<MonaInputEvent>(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        private void RemoveTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.TICK_EVENT), OnTick);
            EventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        private void UpdateActive()
        {
            if (!_active) return;
            AddTickDelegate();

            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(InputInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }

        public void Pause()
        {
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(InputInstructionTile)}.{nameof(Pause)} input paused");
        }

        public void Resume()
        {
            UpdateActive();
        }

        public override void Unload()
        {
            if (_inputManager != null)
            {
                _inputManager.onDeviceLost -= OnDeviceLost;
                _inputManager.onDeviceRegained -= OnDeviceRegained;
            }
            RemoveTickDelegate();
        }

        protected void ProcessAxis(InputAction action)
        {
            var value = action.ReadValue<Vector2>();
            if (value.magnitude > DEAD_ZONE)
                PerformInput();
            else
                ReleaseInput();
        }

        protected void ProcessButton(InputAction action)
        {
            if (action.IsPressed())
                PerformInput();
            else
                ReleaseInput();
        }

        private void PerformInput()
        {
            if (_currentLocalInputState <= MonaInputState.Up)
            {
                _currentLocalInputState = MonaInputState.Pressed;
            }
            else if (_currentLocalInputState == MonaInputState.Pressed)
            {
                _currentLocalInputState = MonaInputState.Held;
            }
        }

        private void ReleaseInput()
        {
            if (_currentLocalInputState > MonaInputState.Up)
            {
                _currentLocalInputState = MonaInputState.Up;
            }
            else if (_currentLocalInputState == MonaInputState.Up)
            {
                _currentLocalInputState = MonaInputState.None;
            }
        }

        private void HandleTick(MonaTickEvent evt)
        {
            ProcessLocalInput();
        }

        void OnDeviceLost(PlayerInput obj)
        {
            Debug.Log("Input Device Lost");
        }

        void OnDeviceRegained(PlayerInput obj)
        {
            Debug.Log("Input Device Regained");
        }

    }
}