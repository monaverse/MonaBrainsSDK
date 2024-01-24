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
using System;
using UnityEngine.InputSystem.Controls;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnKeyInstructionTile : InstructionTile, IOnInteractInstructionTile, IDisposable, IConditionInstructionTile, IStartableInstructionTile, IInputInstructionTile
    {
        public const string ID = "OnKey";
        public const string NAME = "On Key";
        public const string CATEGORY = "Condition/Input";
        public override Type TileType => typeof(OnKeyInstructionTile);

        [SerializeField] private Key _key = Key.Space;
        [BrainPropertyEnum(true)] public Key Key { get => _key; set => _key = value; }

        [SerializeField] protected MonaInputState _inputState = MonaInputState.Pressed;
        [BrainProperty(true)] public MonaInputState InputState { get => _inputState; set => _inputState = value; }

        private IMonaBrain _brain;

        private PlayerInput _inputManager;
        private Inputs _inputs;
        private Action<MonaTileTickEvent> OnTileTick;

        private MonaInputState _currentInputState;

        public OnKeyInstructionTile() { }

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
            ProcessKey(Keyboard.current[_key]);
            if (_currentInputState != MonaInputState.None && _currentInputState == _inputState)
            {
                EventBus.Trigger(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, _brain), new MonaHasInputTickEvent(Time.frameCount));
            }
        }

        private void ProcessKey(KeyControl keyControl)
        {
            if (keyControl.wasPressedThisFrame)
                _currentInputState = MonaInputState.Pressed;
            else if (keyControl.wasReleasedThisFrame)
                _currentInputState = MonaInputState.Up;
            else if (keyControl.isPressed)
                _currentInputState = MonaInputState.Held;
            else
                _currentInputState = MonaInputState.None;
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