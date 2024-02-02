using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalInstructionTile : InstructionTile, IRotateLocalInstructionTile, IActionInstructionTile, IPauseableInstructionTile, IActivateInstructionTile
    {
        public override Type TileType => typeof(RotateLocalInstructionTile);

        public virtual RotateDirectionType DirectionType => RotateDirectionType.SpinRight;

        [SerializeField] private float _angle = 90f;
        [SerializeField] private string _angleValueName;

        [BrainProperty(true)] public float Angle { get => _angle; set => _angle = value; }
        [BrainPropertyValueName("Angle")] public string AngleValueName { get => _angleValueName; set => _angleValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyEnum(true)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private MoveModeType _mode = MoveModeType.Time;
        [BrainPropertyEnum(false)] public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName = null;

        [BrainProperty(false)] public float Value { get => _value; set => _value= value; }
        [BrainPropertyValueName("Value")] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        private Vector3 _direction;  

        private IMonaBrain _brain;

        private float _time;
        private Quaternion _start;
        private Quaternion _end;

        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private float _speed
        {
            get => _brain.State.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState = MovingStateType.Stopped;

        public Vector2 InputMoveDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        public RotateLocalInstructionTile() { }

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
            if (!_active) return;

            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }

        public void Pause()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Pause)} input paused");
        }

        public void Resume()
        {
            UpdateActive();
        }


        public override void Unload()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Unload)}");
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public override InstructionTileResult Do()
        {
            _direction = GetDirectionVector(DirectionType);

            if (!string.IsNullOrEmpty(_angleValueName))
                _angle = _brain.State.GetFloat(_angleValueName);

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.State.GetFloat(_valueValueName);

            if (_mode == MoveModeType.Instant)
            {
                _brain.Body.RotateAround(_direction, _angle, true, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                _time = 0;
                _start = _brain.Body.GetRotation();
                _end = _start * Quaternion.Euler(_direction * _angle);

                OnFixedTick = HandleFixedTick;
                EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            }
            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
            switch(_mode)
            {
                case MoveModeType.Time: MoveOverTime(deltaTime); break;
                case MoveModeType.Speed: MoveAtSpeed(deltaTime); break;
            }
        }

        private float Evaluate(float t)
        {
            switch (_easing)
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
                _brain.Body.SetRotation(Quaternion.Slerp(_start, _end, Evaluate(_time)), true, true);

                if (_time >= 1f)
                {
                    _brain.Body.SetRotation(_end, true, true);
                    StopMoving();
                }
            }
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _time += ((_angle / 360f) / (_value * _speed)) * deltaTime;
                _brain.Body.SetRotation(Quaternion.Slerp(_start, _end, Evaluate(_time)), true, true);

                if (_time >= 1f)
                {
                    _brain.Body.SetRotation(_end, true, true);
                    StopMoving();
                }
            }
        }

        private void StopMoving()
        {
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }

        private Vector3 GetDirectionVector(RotateDirectionType moveType)
        {
            switch (moveType)
            {
                case RotateDirectionType.SpinDown: return Vector3.right;
                case RotateDirectionType.SpinUp: return Vector3.right * -1f;
                case RotateDirectionType.RollLeft: return Vector3.forward;
                case RotateDirectionType.RollRight: return Vector3.forward * -1;
                case RotateDirectionType.SpinRight: return Vector3.up;
                case RotateDirectionType.SpinLeft: return Vector3.up * -1;
                default: return Vector3.zero;
            }
        }

    }
}