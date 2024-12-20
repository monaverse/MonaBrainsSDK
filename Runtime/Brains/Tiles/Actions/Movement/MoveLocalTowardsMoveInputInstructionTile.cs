﻿using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Utils;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveLocalTowardsMoveInputInstructionTile : InstructionTile, IMoveLocalTowardsMoveInputInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile,
        IActivateInstructionTile, IPauseableInstructionTile
    {
        public const string ID = "MoveLocalTowardsMoveInput";
        public const string NAME = "Move Local Towards\n Move Input";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(MoveLocalTowardsMoveInputInstructionTile);

        [SerializeField]
        public float _value;
        [BrainProperty]
        public float Value { get => _value; set => _value = value; }

        public Vector3 _direction;

        private IMonaBrain _brain;

        private Vector3 _start;
        private float _timeElapsed;
        public float TimeElapsed => _timeElapsed;
        private bool _active;
        private MonaInput _brainInput;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaInputEvent> OnInput;

        public float Speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState;

        public Vector2 InputMoveDirection
        {
            get => _brain.Variables.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        public MoveLocalTowardsMoveInputInstructionTile() { }
        
        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            UpdateActive();
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

        private void UpdateActive()
        {
            if (!_active)
            {
                RemoveDelegates();
                return;
            }

            if (_movingState == MovingStateType.Moving)
            {
                AddDelegates();
            }

            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(MoveLocalTowardsMoveInputInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        public override void Unload(bool destroy = false)
        {
            RemoveDelegates();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(MoveLocalTowardsMoveInputInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveDelegates();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(MoveLocalTowardsMoveInputInstructionTile)}.{nameof(Pause)}");
        }

        public bool Resume()
        {
            UpdateActive();
            return _movingState == MovingStateType.Moving;
        }

        private void HandleBodyInput(MonaInputEvent evt)
        {
            _brainInput = evt.Input;
        }

        private void AddDelegates()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            OnInput = HandleBodyInput;
            MonaEventBus.Register<MonaInputEvent>(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        private void RemoveDelegates()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }


        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public virtual List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            _direction = _brain.Body.ActiveTransform.forward * InputMoveDirection.y;

            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Success);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
            MoveAtSpeed(deltaTime);
            _movingState = MovingStateType.Stopped;
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _brain.Body.MoveDirection(_direction * (deltaTime * (_value * Speed)), true);
            }
        }
    }
}