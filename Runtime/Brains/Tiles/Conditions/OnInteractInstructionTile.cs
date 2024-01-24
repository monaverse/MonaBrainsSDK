using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Input;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnInteractInstructionTile : InstructionTile, IOnInteractInstructionTile, IDisposable, IConditionInstructionTile, IStartableInstructionTile, IInputInstructionTile
    {
        public const string ID = "OnInteract";
        public const string NAME = "On Interact";
        public const string CATEGORY = "Condition/Input";
        public override Type TileType => typeof(OnInteractInstructionTile);

        protected IMonaBrain _brain;

        protected virtual MonaInputState _inputState { get => MonaInputState.Pressed; }

        private PlayerInput _inputManager;
        private Inputs _inputs;
        private Action<MonaTileTickEvent> OnTileTick;

        protected MonaInputState _currentInputState;

        public OnInteractInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            ConfigureInput();

            OnTileTick = HandleTileTick;
            EventBus.Register<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTileTick);

        }

        public void Dispose()
        {
            if (_inputManager != null)
            {
                _inputManager.onDeviceLost -= OnDeviceLost;
                _inputManager.onDeviceRegained -= OnDeviceRegained;
            }
            EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTileTick);
        }

        private void ConfigureInput()
        {
            _inputManager = MonaGlobalBrainRunner.Instance.GetPlayerInput();
            _inputManager.onDeviceLost += OnDeviceLost;
            _inputManager.onDeviceRegained += OnDeviceRegained;

            _inputs = new Inputs();
            _inputs.Player.Enable();

            _inputManager.ActivateInput();
        }

        private void ProcessInput()
        {
            ProcessButton(_inputs.Player.Action);

            if (_currentInputState != MonaInputState.None && _currentInputState == _inputState)
            {
                EventBus.Trigger(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, _brain), new MonaHasInputTickEvent(Time.frameCount));
            }
        }

        private void ProcessButton(InputAction action)
        {
            if (action.IsPressed())
                PerformInput();
            else
                ReleaseInput();
        }

        private void PerformInput()
        {
            if (_currentInputState <= MonaInputState.Up)
            {
                _currentInputState = MonaInputState.Pressed;
            }
            else if (_currentInputState == MonaInputState.Pressed)
            {
                _currentInputState = MonaInputState.Held;
            }
        }

        private void ReleaseInput()
        {
            if (_currentInputState > MonaInputState.Up)
            {
                _currentInputState = MonaInputState.Up;
            }
            else if (_currentInputState == MonaInputState.Up)
            {
                _currentInputState = MonaInputState.None;
            }
        }

        private void HandleTileTick(MonaTileTickEvent evt)
        {
            if((_brain.Body == null || _brain.Body.HasControl()))
                ProcessInput();
        }

        void OnDeviceLost(PlayerInput obj)
        {
            Debug.Log("Input Device Lost");
        }

        void OnDeviceRegained(PlayerInput obj)
        {
            Debug.Log("Input Device Regained");
        }

        public override InstructionTileResult Do()
        {
            if (_currentInputState == _inputState)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }
    }
}