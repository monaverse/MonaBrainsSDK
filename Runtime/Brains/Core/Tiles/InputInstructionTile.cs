using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Input.Enums;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    [Serializable]
    public abstract class InputInstructionTile : InstructionTile, IInputInstructionTile, IInstructionTileWithPreload, IOnInteractInstructionTile, IConditionInstructionTile,
        IStartableInstructionTile, IActivateInstructionTile, IPauseableInstructionTile
    {
        protected abstract MonaInputState GetInputState();

        private Action<MonaTickEvent> OnTick;
        private Action<MonaInputEvent> OnInput;

        private bool _active;

        protected MonaInputState _currentLocalInputState;

        protected IMonaBrain _brain;
        public IMonaBrain Brain => _brain;

        protected IMonaBrainInput _brainInput;

        protected abstract void ProcessLocalInput();
        protected abstract void HandleBodyInput(MonaInputEvent evt);
        public abstract void ReprocessInput(MonaInput input);
        public abstract MonaInput GetInput();

        public bool PlayerTriggered => true;

        public virtual void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            ConfigureInput();

            UpdateActive();
        }

        protected bool BrainOnRemotePlayer()
        {
            return _brain.Body.IsAttachedToRemotePlayer();
        }

        private void ConfigureInput()
        {
            _brainInput = MonaGlobalBrainRunner.Instance.GetBrainInput();
        }

        protected void SetLocalInput(MonaInput input)
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

        protected virtual void AddTickDelegate()
        {
            OnTick = HandleTick;
            EventBus.Register<MonaTickEvent>(new EventHook(MonaCoreConstants.TICK_EVENT), OnTick);

            OnInput = HandleBodyInput;
            EventBus.Register<MonaInputEvent>(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);

            _brainInput.StartListening(this);
        }

        protected virtual void RemoveTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.TICK_EVENT), OnTick);
            EventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);

            _brainInput.StopListening(this);
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                RemoveTickDelegate();
                return;
            }

            AddTickDelegate();

            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(InputInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }

        public void Pause()
        {
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(InputInstructionTile)}.{nameof(Pause)} input paused");
        }

        public bool Resume()
        {
            UpdateActive();
            return false;
        }

        public override void Unload()
        {
            RemoveTickDelegate();
        }

        private void HandleTick(MonaTickEvent evt)
        {
            ProcessLocalInput();
        }

    }
}