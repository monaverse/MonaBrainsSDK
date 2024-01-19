using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using Mona.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Events;
using Mona.Brains.Tiles.Actions.Movement.Interfaces;

namespace Mona.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalAroundMoveInputInstructionTile : InstructionTile, IRotateLocalAroundMoveInputInstructionTile, IActionInstructionTile
    {
        public const string ID = "RotateLocalAroundMoveInput";
        public const string NAME = "Rotate Local Around\n Move Input";
        public const string CATEGORY = "Movement";
        public override Type TileType => typeof(RotateLocalAroundMoveInputInstructionTile);

        [SerializeField]
        private float _value;
        [BrainProperty]
        public float Value { get => _value; set => _value = value; }

        [SerializeField]
        private MoveModeType _mode;
        [BrainPropertyEnum(false)]
        public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField]
        private bool _listenForTick;
        [BrainProperty(false)]
        public bool ListenForTick { get => _listenForTick; set => _listenForTick = value; }

        private IMonaBrain _brain;

        private float _timeElapsed;
        public float TimeElapsed => _timeElapsed;

        private Action<MonaTileTickEvent> OnTick;

        public float Speed
        {
            get => _brain.State.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public MovingStateType MovingState
        {
            get => (MovingStateType)_brain.State.GetInt(MonaBrainConstants.MOVING_ROTATE_STATE);
            set => _brain.State.Set(MonaBrainConstants.MOVING_ROTATE_STATE, (int)value);
        }

        public Vector2 InputMoveDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        public RotateLocalAroundMoveInputInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    if (_listenForTick)
                        EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public override InstructionTileResult Do()
        {
            if (MovingState == MovingStateType.Stopped && _mode == MoveModeType.Time)
                _timeElapsed = 0;

            if (_listenForTick)
            {
                if (MovingState == MovingStateType.Stopped)
                {
                    OnTick = HandleTick;
                    EventBus.Register<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
                }
                MovingState = MovingStateType.Moving;
                return Complete(InstructionTileResult.Running);
            }
            else
            {
                Tick(Time.deltaTime);
                MovingState = MovingStateType.Moving;
                return Complete(InstructionTileResult.Success);
            }
        }

        private void HandleTick(MonaTileTickEvent evt)
        {
            Tick(evt.DeltaTime);
        }

        private void Tick(float deltaTime)
        {
            switch(_mode)
            {
                case MoveModeType.Time: MoveOverTime(deltaTime); break;
                case MoveModeType.Speed: MoveAtSpeed(deltaTime); break;
            }
        }

        private void MoveOverTime(float deltaTime)
        {
            if (MovingState == MovingStateType.Moving)
            {
                _brain.Body.RotateAround(_brain.Body.ActiveTransform.up, InputMoveDirection.x * (90f * (deltaTime * Speed)), true, true);
                _timeElapsed += deltaTime;
                if (_timeElapsed >= _value) 
                    StopMoving();
            }
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (MovingState == MovingStateType.Moving)
            {
                _brain.Body.RotateAround(_brain.Body.ActiveTransform.up, InputMoveDirection.x * (90f * (deltaTime * (_value * Speed))), true, true);
                StopMoving();
            }
        }

        private void StopMoving()
        {
            MovingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }

    }
}