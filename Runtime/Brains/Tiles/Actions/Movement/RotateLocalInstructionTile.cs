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
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Control;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalInstructionTile : InstructionTile, IRotateLocalInstructionTile, IActionInstructionTile, IPauseableInstructionTile, IActivateInstructionTile, INeedAuthorityInstructionTile,
        IChangeDefaultRotationInstructionTile
        
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

        [SerializeField] private bool _usePhysics = false;
        [BrainProperty(false)] public bool UsePhysics { get => _usePhysics; set => _usePhysics = value; }

        private Quaternion _direction;  

        private IMonaBrain _brain;
        private IInstruction _instruction;

        private float _time;
        private Quaternion _start;
        private Quaternion _end;

        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState = MovingStateType.Stopped;

        public Vector2 InputMoveDirection
        {
            get => _brain.Variables.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        public RotateLocalInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;
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
            if(_brain.LoggingEnabled)
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

        public Quaternion GetEndRotation(Quaternion rotation)
        {
            _direction = GetDirectionRotation(DirectionType, _angle);
            //Debug.Log($"{nameof(MoveLocalInstructionTile)}.Do {DirectionType}");

            if (!string.IsNullOrEmpty(_angleValueName))
                _angle = _brain.Variables.GetFloat(_angleValueName);

            return rotation * _direction;
        }

        Quaternion GetStartRotation()
        {
            return _instruction.GetStartRotation(this);
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);

            if (_mode == MoveModeType.Instant)
            {
                _start = GetStartRotation();
                _end = GetEndRotation(_start);
                _brain.Body.SetRotation(_end, true, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                _time = 0;
                _start = GetStartRotation();
                _end = GetEndRotation(_start);

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

        public virtual IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private void MoveOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _start = GetStartRotation();
                _end = GetEndRotation(_start);

                if (_time >= 1f)
                {
                    if(!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                        _brain.Body.SetRotation(_end, true, true);
                    StopMoving();
                }
                else
                {
                    _brain.Body.SetRotation(Quaternion.Slerp(_start, _end, Evaluate(_time)), true, true);
                }
                _time += Mathf.Round((deltaTime / _value) * 1000f) / 1000f;

            }
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _start = GetStartRotation();
                _end = GetEndRotation(_start);
                
                if (_time >= 1f)
                {
                    if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                        _brain.Body.SetRotation(_end, true, true);
                    StopMoving();
                }
                else {
                    _brain.Body.SetRotation(Quaternion.Slerp(_start, _end, Evaluate(_time)), true, true);
                }
                _time += Mathf.Round(( ((_angle / 360f) / (_value * _speed)) * deltaTime ) * 1000f) / 1000f;
            }
        }

        private void StopMoving()
        {
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }

        private Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle)
        {
            switch (moveType)
            {
                case RotateDirectionType.SpinDown: return Quaternion.AngleAxis(angle, Vector3.right);
                case RotateDirectionType.SpinUp: return Quaternion.AngleAxis(-angle, Vector3.right);
                case RotateDirectionType.RollLeft: return Quaternion.AngleAxis(angle, Vector3.forward);
                case RotateDirectionType.RollRight: return Quaternion.AngleAxis(-angle, Vector3.forward);
                case RotateDirectionType.SpinRight: return Quaternion.AngleAxis(angle, Vector3.up);
                case RotateDirectionType.SpinLeft: return Quaternion.AngleAxis(-angle, Vector3.up);
                default: return Quaternion.identity;
            }
        }

    }
}