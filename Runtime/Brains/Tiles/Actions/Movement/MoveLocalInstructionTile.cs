using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{

    [Serializable]
    public class MoveLocalInstructionTile : InstructionTile, IMoveLocalInstructionTile, IActionInstructionTile
    {
        public override Type TileType => typeof(MoveLocalInstructionTile);

        public virtual MoveDirectionType DirectionType => MoveDirectionType.Forward;

        [SerializeField] private float _distance = 1f;
        [SerializeField] private string _distanceValueName = null;

        [BrainProperty(true)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance")] public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyEnum(true)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private MoveModeType _mode = MoveModeType.Time;
        [BrainProperty(false)] public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName = null;

        [BrainProperty(false)] public float Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value")] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private Vector3 _direction;

        private IMonaBrain _brain;

        private Vector3 _start;
        private Vector3 _end;
        private float _time;

        private Action<MonaTileTickEvent> OnTick;
        private Action<MonaBrainReloadEvent> OnHotReload;

        private float _speed
        {
            get => _brain.State.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState;

        public Vector2 InputMoveDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }
        
        public MoveLocalInstructionTile() { }
        
        public void Preload(IMonaBrain brainInstance)
        {
            if (_brain != brainInstance)
            {
                _brain = brainInstance;
                OnHotReload = HandleHotReload;
                EventBus.Register<MonaBrainReloadEvent>(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brain.Guid), OnHotReload);
            }
        }

        private void HandleHotReload(MonaBrainReloadEvent evt)
        {
            EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
            _time = 0;
            _movingState = MovingStateType.Stopped;
            Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(HandleHotReload)} {this}");
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
                    if(thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public override InstructionTileResult Do()
        {
            _direction = GetDirectionVector(DirectionType);

            if (!string.IsNullOrEmpty(_distanceValueName))
                _distance = _brain.State.GetFloat(_distanceValueName);

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.State.GetFloat(_valueValueName);

            if (_mode == MoveModeType.Instant)
            {
                _brain.Body.MoveDirection(_direction * _distance, true, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                _time = 0;
                _start = _brain.Body.GetPosition();
                _end = _start + _direction * _distance;
             
                OnTick = HandleTick;
                EventBus.Register<MonaTileTickEvent>(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnTick);
            }

            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
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

        private float Evaluate(float t)
        {
            switch(_easing)
            {
                case EasingType.EaseInOut:
                    return -((Mathf.Cos(Mathf.PI * t) - 1f) / 2f);
                case EasingType.EaseIn:
                    return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
                case EasingType.EaseOut:
                    return Mathf.Sin((t * Mathf.PI) / 2f);
                default:
                    return t;
            }
        }

        private void MoveOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _time += deltaTime / _value;
                _brain.Body.SetPosition(Vector3.Lerp(_start, _end, Evaluate(_time)), true, true);

                if(_time >= 1f)
                {
                    _brain.Body.SetPosition(_end, true, true);
                    StopMoving();
                }
            }
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _time += (_distance / (_value * _speed)) * deltaTime;
                _brain.Body.SetPosition(Vector3.Lerp(_start, _end, Evaluate(_time)), true, true);

                if (_time >= 1f)
                {
                    _brain.Body.SetPosition(_end, true, true);
                    StopMoving();
                }
            }
        }

        private void StopMoving()
        {
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }

        private Vector3 GetDirectionVector(MoveDirectionType moveType)
        {
            switch (moveType)
            {
                case MoveDirectionType.Forward: return _brain.Body.ActiveTransform.forward;
                case MoveDirectionType.Backward: return _brain.Body.ActiveTransform.forward * -1f;
                case MoveDirectionType.Up: return _brain.Body.ActiveTransform.up;
                case MoveDirectionType.Down: return _brain.Body.ActiveTransform.up * -1f;
                case MoveDirectionType.Right: return _brain.Body.ActiveTransform.right;
                case MoveDirectionType.Left: return _brain.Body.ActiveTransform.right * -1f;
                default: return Vector3.zero;
            }
        }

    }
}