using Mona.SDK.Brains.Core.Tiles;
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

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveLocalTowardsMoveInputInstructionTile : InstructionTile, IMoveLocalTowardsMoveInputInstructionTile, IActionInstructionTile
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

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        public float Speed
        {
            get => _brain.State.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public MovingStateType MovingState
        {
            get => (MovingStateType)_brain.State.GetInt(MonaBrainConstants.MOVING_STATE);
            set => _brain.State.Set(MonaBrainConstants.MOVING_STATE, (int)value);
        }

        public Vector2 InputMoveDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        public MoveLocalTowardsMoveInputInstructionTile() { }
        
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
            _direction = _brain.Body.ActiveTransform.forward * InputMoveDirection.y;
            
            MovingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Success);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
            MoveAtSpeed(deltaTime);
            MovingState = MovingStateType.Stopped;
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (MovingState == MovingStateType.Moving)
            {
                _brain.Body.MoveDirection(_direction * (deltaTime * (_value * Speed)), true, true);
            }
        }
    }
}