using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalAroundMoveInputInstructionTile : InstructionTile, IRotateLocalAroundMoveInputInstructionTile, IActionInstructionTile
    {
        public const string ID = "RotateLocalAroundMoveInput";
        public const string NAME = "Rotate Local Around\n Move Input";
        public const string CATEGORY = "Rotation";
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

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        public float Speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public Vector2 InputMoveDirection
        {
            get => _brain.Variables.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        private MovingStateType _movingState = MovingStateType.Stopped;

        public RotateLocalAroundMoveInputInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        public override void Unload()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public override InstructionTileResult Do()
        {
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
                _brain.Body.RotateAround(_brain.Body.ActiveTransform.up, InputMoveDirection.x * (90f * (deltaTime * (_value * Speed))), true, true);
            }
        }
    }
}