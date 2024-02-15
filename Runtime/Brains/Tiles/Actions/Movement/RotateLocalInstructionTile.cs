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
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Body.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalInstructionTile : InstructionTile, IRotateLocalInstructionTile, IActionInstructionTile, IPauseableInstructionTile, 
        IActivateInstructionTile, INeedAuthorityInstructionTile, IProgressInstructionTile

    {
        public override Type TileType => typeof(RotateLocalInstructionTile);

        public virtual RotateDirectionType DirectionType => RotateDirectionType.SpinRight;

        [SerializeField] private float _angle = 90f;
        [SerializeField] private string _angleValueName;

        [BrainProperty(true)] public float Angle { get => _angle; set => _angle = value; }
        [BrainPropertyValueName("Angle", typeof(IMonaVariablesFloatValue))] public string AngleValueName { get => _angleValueName; set => _angleValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyEnum(true)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private MoveModeType _mode = MoveModeType.Time;
        [BrainPropertyEnum(false)] public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName = null;

        [BrainProperty(false)] public float Value { get => _value; set => _value= value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private bool _usePhysics = false;
        [BrainProperty(false)] public bool UsePhysics { get => _usePhysics; set => _usePhysics = value; }

        private Quaternion _direction;  

        private IMonaBrain _brain;
        private IInstruction _instruction;
        private string _progressName;

        private Quaternion _start;
        private Quaternion _end;

        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaInputEvent> OnInput;

        private MonaInput _bodyInput;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState = MovingStateType.Stopped;

        public Vector2 InputMoveDirection
        {
            get => _brain.Variables.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }

        [SerializeField] protected bool _onlyTurnWhenMoving = false;

        public float Progress
        {
            get => _brain.Variables.GetFloat(_progressName);
            set => _brain.Variables.Set(_progressName, value);
        }

        public bool InProgress
        {
            get
            {
                var progress = Progress;
                if (_instruction.CurrentTile != this) return false;
                return progress > 0 && progress <= 1f;
            }
        }

        public RotateLocalInstructionTile() { }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;
            
            var pagePrefix = page.IsCore ? "Core" : ("State" + brainInstance.StatePages.IndexOf(page));
            var instructionIndex = page.Instructions.IndexOf(instruction);

            _progressName = $"__{pagePrefix}_{instructionIndex}_progress";

            _brain.Variables.Set(_progressName, 0f);

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

            if (_movingState == MovingStateType.Moving)
            {
                AddFixedTickDelegate();
            }

            AddInputDelegate();

           // if (_brain.LoggingEnabled)
           //     Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
            //if (_brain.LoggingEnabled)
           //     Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Pause)} {_movingState} ");
        }

        public bool Resume()
        {
            UpdateActive();
            return _movingState == MovingStateType.Moving;
        }


        public override void Unload()
        {
            RemoveFixedTickDelegate();
            //if(_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Unload)}");
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    RemoveFixedTickDelegate();
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        private void AddFixedTickDelegate()
        {
            //Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(AddFixedTickDelegate)}, {_brain.Body.ActiveTransform.name}", _brain.Body.ActiveTransform.gameObject);
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void AddInputDelegate()
        {
            if (DirectionType == RotateDirectionType.InputLeftRight)
            {
                OnInput = HandleBodyInput;
                EventBus.Register<MonaInputEvent>(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
            }
        }

        protected void HandleBodyInput(MonaInputEvent evt)
        {
            //Debug.Log($"{nameof(HandleBodyInput)} {evt.Input.MoveValue}");
            if (_movingState != MovingStateType.Moving)
                _bodyInput = evt.Input;
        }

        private void RemoveFixedTickDelegate()
        {
            //Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(RemoveFixedTickDelegate)}, {_brain.Body.ActiveTransform.name}", _brain.Body.ActiveTransform.gameObject);
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        public InstructionTileResult Continue()
        {
            Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Continue)} take over control and continue executing brain at {Progress}, {_progressName} on {this} {_brain.Body.ActiveTransform}", _brain.Body.ActiveTransform.gameObject);
            _movingState = MovingStateType.Moving;
            AddFixedTickDelegate();
            return Do();
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);


            //Debug.Log($"move input {_bodyInput.MoveValue}");
            if (DirectionType == RotateDirectionType.InputLeftRight)
            {
                _brain.Body.SetDragType(DragType.Quadratic);
                _brain.Body.SetDrag(.2f);
                _brain.Body.SetAngularDrag(.2f);
                _brain.Body.SetOnlyApplyDragWhenGrounded(true);
                
                if(_bodyInput.MoveValue.x == 0f || (_onlyTurnWhenMoving && _bodyInput.MoveValue.y == 0))
                {
                    _movingState = MovingStateType.Stopped;
                    return Complete(InstructionTileResult.Success);
                }
            }

            if (_mode == MoveModeType.Instant)
            {
                if (!string.IsNullOrEmpty(_angleValueName))
                    _angle = _brain.Variables.GetFloat(_angleValueName);

                _direction = GetDirectionRotation(DirectionType, _angle);

                if (DirectionType == RotateDirectionType.InputLeftRight && _bodyInput.MoveValue.x != 0f)
                {
                    _direction = Quaternion.AngleAxis(_angle * Mathf.Sign(_bodyInput.MoveValue.x), Vector3.up);
                }

                _brain.Body.SetRotation(_direction, !_usePhysics, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                Progress = 0;
                AddFixedTickDelegate();
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
            if (!_brain.Body.HasControl())
            {
                LostControl();
                return;
            }

            switch (_mode)
            {
                case MoveModeType.Time: MoveOverTime(deltaTime); break;
                case MoveModeType.Speed: MoveAtSpeed(deltaTime); break;
            }
        }

        private void LostControl()
        {
            Debug.Log($"{nameof(RotateLocalInstructionTile)} {nameof(LostControl)}");
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.LostAuthority, true);
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
                var progressDelta = Mathf.Round((deltaTime / _value) * 1000f) / 1000f;

                Progress = Mathf.Clamp01(Progress);

                float diff = Evaluate(Mathf.Clamp01(Progress + progressDelta)) - Evaluate(Progress);

                if (!string.IsNullOrEmpty(_angleValueName))
                    _angle = _brain.Variables.GetFloat(_angleValueName);

                _direction = GetDirectionRotation(DirectionType, _angle * diff);

                if (DirectionType == RotateDirectionType.InputLeftRight && _bodyInput.MoveValue.x != 0f)
                {
                    _direction = Quaternion.AngleAxis(_angle * diff * Mathf.Sign(_bodyInput.MoveValue.x), Vector3.up);
                }

                //if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                _brain.Body.SetRotation(_direction, !_usePhysics, true);

                if (Progress >= 1f)
                {
                    StopMoving();
                }
                else
                {
                    Progress += progressDelta;
                }
                //Debug.Log($"{nameof(RotateLocalInstructionTile)} {Progress} {_start} {_end}");

            }
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                var progressDelta = Mathf.Round((((_angle / 360f) / ((1f / _value) * _speed)) * deltaTime) * 1000f) / 1000f;

                Progress = Mathf.Clamp01(Progress);

                float diff = Evaluate(Mathf.Clamp01(Progress + progressDelta)) - Evaluate(Progress);

                if (!string.IsNullOrEmpty(_angleValueName))
                    _angle = _brain.Variables.GetFloat(_angleValueName);

                _direction = GetDirectionRotation(DirectionType, _angle * diff);

                if (DirectionType == RotateDirectionType.InputLeftRight && _bodyInput.MoveValue.x != 0f)
                {
                    _direction = Quaternion.AngleAxis(_angle * diff * Mathf.Sign(_bodyInput.MoveValue.x), Vector3.up);
                }

                //if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                _brain.Body.SetRotation(_direction, !_usePhysics, true);

                if (Progress >= 1f)
                {
                    StopMoving();
                }
                else {
                    Progress += progressDelta;
                }
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