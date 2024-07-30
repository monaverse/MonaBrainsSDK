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
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Core.Utils;
using Unity.Profiling;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class RotateLocalInstructionTile : InstructionTile, IActionInstructionTile, IPauseableInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IActivateInstructionTile, INeedAuthorityInstructionTile, IProgressInstructionTile, IRotateLocalInstructionTile, ITickAfterInstructionTile

    {
        public override Type TileType => typeof(RotateLocalInstructionTile);

        public virtual RotateDirectionType DirectionType => RotateDirectionType.SpinRight;

        [SerializeField] private Vector3 _rotationAngles;
        [SerializeField] private string[] _rotationAnglesName;
        [BrainPropertyShow(nameof(DirectionType), (int)RotateDirectionType.EulerAngles)]
        [BrainProperty(true)] public Vector3 RotationAngles { get => _rotationAngles; set => _rotationAngles = value; }
        [BrainPropertyValueName("RotationAngles", typeof(IMonaVariablesVector3Value))] public string[] RotationAnglesName { get => _rotationAnglesName; set => _rotationAnglesName = value; }

        [SerializeField] protected float _angle = 90f;
        [SerializeField] protected string _angleValueName;

        [SerializeField] private MoveModeType _mode = MoveModeType.Time;
        [BrainPropertyEnum(false)] public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName = null;

        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Time)]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Time, "Seconds")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Angles/Sec")]
        [BrainProperty(false)] public float Value { get => _value; set => _value= value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Time)]
        [BrainPropertyEnum(false)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private SpaceType _space = SpaceType.Local;
        [BrainPropertyShow(nameof(DirectionType), (int)RotateDirectionType.EulerAngles)]
        [BrainPropertyEnum(false)] public SpaceType Space { get => _space; set => _space = value; }

        private Quaternion _direction;

        protected IMonaBrain _brain;
        private string _progressName;

        private Quaternion _start;
        private Quaternion _end;

        private bool _active;
        private const float _maximumVectorStep = 1.0f;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool _listenToInput;

        protected MonaInput _bodyInput;

        private bool InstantRotation => _mode == MoveModeType.SpeedOnly || _mode == MoveModeType.Instant;

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
        [SerializeField] protected bool _lookStraightAhead = false;

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

        private bool _preloaded;
        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _profilerPreload.Begin();

            _brain = brainInstance;
            _instruction = instruction;

            if (!_preloaded)
            {
                var pagePrefix = page.IsCore ? "Core" : ("State" + brainInstance.StatePages.IndexOf(page));
                var instructionIndex = page.Instructions.IndexOf(instruction);

                _progressName = $"__{pagePrefix}_{instructionIndex}_progress";
                _preloaded = true;
            }

            _brain.Variables.Set(_progressName, 0f);

            UpdateActive();

            _profilerPreload.End();
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
                if (_movingState == MovingStateType.Moving)
                    LostControl();
                return;
            }

            if (InstantRotation ||  _movingState == MovingStateType.Moving)
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


        public override void Unload(bool destroy = false)
        {
            RemoveFixedTickDelegate();
            //if(_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Unload)}");
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
            if(!InstantRotation) RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddFixedTickDelegate()
        {
            //Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(AddFixedTickDelegate)}, {_brain.Body.ActiveTransform.name}", _brain.Body.ActiveTransform.gameObject);
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void AddInputDelegate()
        {
            if (DirectionType == RotateDirectionType.InputLeftRight)
            {
                _listenToInput = true;
            }
        }

        private void RemoveFixedTickDelegate()
        {
            //Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(RemoveFixedTickDelegate)}, {_brain.Body.ActiveTransform.name}", _brain.Body.ActiveTransform.gameObject);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        public InstructionTileResult Continue()
        {
            Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(Continue)} take over control and continue executing brain at {Progress}, {_progressName} on {this} {_brain.Body.ActiveTransform}", _brain.Body.ActiveTransform.gameObject);
            _movingState = MovingStateType.Moving;
            AddFixedTickDelegate();
            return Do();
        }

        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(RotateLocalInstructionTile)}.{nameof(Do)}");
        static readonly ProfilerMarker _profilerPreload = new ProfilerMarker($"MonaBrains.{nameof(RotateLocalInstructionTile)}.{nameof(Preload)}");
        static readonly ProfilerMarker _profilerFixedTick = new ProfilerMarker($"MonaBrains.{nameof(RotateLocalInstructionTile)}.{nameof(HandleFixedTick)}");

        public override InstructionTileResult Do()
        {

            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            _profilerDo.Begin();

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);

            if (HasVector3Values(_rotationAnglesName))
                _rotationAngles = GetVector3Value(_brain, _rotationAnglesName);

            UpdateInput();

            //Debug.Log($"move input {_bodyInput.MoveValue}");
            if (DirectionType == RotateDirectionType.InputLeftRight)
            {
                //_brain.Body.SetDragType(DragType.Quadratic);
                //_brain.Body.SetDrag(.2f);
                //_brain.Body.SetAngularDrag(.2f);
                //_brain.Body.SetOnlyApplyDragWhenGrounded(true);
                
                if(Mathf.Approximately(_bodyInput.MoveValue.x, 0f) || (_lookStraightAhead && Mathf.Approximately(_bodyInput.MoveValue.y, 0)))
                {
                    _movingState = MovingStateType.Stopped;
                    _profilerDo.End();
                    return Complete(InstructionTileResult.Success);
                }
            }

            if (_movingState == MovingStateType.Stopped)
            {
                Progress = 0;
                StartRotation();
                if(!InstantRotation)
                    AddFixedTickDelegate();
            }
            _movingState = MovingStateType.Moving;

            _profilerDo.End();
            return Complete(InstructionTileResult.Running);
        }

        protected virtual void StartRotation()
        {

        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            _profilerFixedTick.Begin();
            FixedTick(evt.DeltaTime);
            _profilerFixedTick.End();
        }

        private void FixedTick(float deltaTime)
        {
            if (InstantRotation && _movingState == MovingStateType.Stopped) return;

            UpdateInput();

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

            if (InstantRotation)
            {
                if (!string.IsNullOrEmpty(_angleValueName))
                    _angle = _brain.Variables.GetFloat(_angleValueName);

                StartRotation();

                float step = _mode == MoveModeType.SpeedOnly ? _angle * deltaTime : _angle;

                _direction = GetDirectionRotation(DirectionType, step, deltaTime, 1f, true);

                if (DirectionType == RotateDirectionType.InputLeftRight && _bodyInput.MoveValue.x != 0f)
                {
                    _direction = Quaternion.AngleAxis(_angle * Mathf.Sign(_bodyInput.MoveValue.x), Vector3.up);
                }

                if (_onlyTurnWhenMoving && Mathf.Approximately(_bodyInput.MoveValue.y, 0f))
                {
                    _direction = Quaternion.identity;
                }

                _brain.Body.SetRotation(_direction, true);
                StopMoving();
            }
        }

        private void UpdateInput()
        {
            if (!_listenToInput) return;
            if (_movingState != MovingStateType.Moving)
                _bodyInput = _instruction.InstructionInput;
            //Debug.Log($"{nameof(UpdateInput)} {_bodyInput.MoveValue}");
        }

        private void LostControl()
        {
            //Debug.Log($"{nameof(RotateLocalInstructionTile)} {nameof(LostControl)}");
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

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public virtual List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
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

                _direction = GetDirectionRotation(DirectionType, _angle * diff, diff, Progress, false);

                if (DirectionType == RotateDirectionType.InputLeftRight && _bodyInput.MoveValue.x != 0f)
                {
                    _direction = Quaternion.AngleAxis(_angle * diff * Mathf.Sign(_bodyInput.MoveValue.x), Vector3.up);
                }

                if (_onlyTurnWhenMoving && Mathf.Approximately(_bodyInput.MoveValue.y, 0f))
                {
                    _direction = Quaternion.identity;
                }

                //if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                _brain.Body.SetRotation(_direction, true);

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
                if (!string.IsNullOrEmpty(_angleValueName))
                    _angle = _brain.Variables.GetFloat(_angleValueName);

                var speed = _speed * _value;
                var distance = _angle;
                var progressDelta = Mathf.Round(((1f/(distance/speed)) * deltaTime) * 1000f) / 1000f;

                Progress = Mathf.Clamp01(Progress);

                float diff = Evaluate(Mathf.Clamp01(Progress + progressDelta)) - Evaluate(Progress);

                _direction = GetDirectionRotation(DirectionType, _angle * diff, diff, Progress, false);

                if (DirectionType == RotateDirectionType.InputLeftRight && _bodyInput.MoveValue.x != 0f)
                {
                    _direction = Quaternion.AngleAxis(_angle * diff * Mathf.Sign(_bodyInput.MoveValue.x), Vector3.up);
                }

                //if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                _brain.Body.SetRotation(_direction, true);

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

        protected virtual Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress, bool immediate)
        {
            switch (moveType)
            {
                case RotateDirectionType.SpinDown: return Quaternion.AngleAxis(angle, Vector3.right);
                case RotateDirectionType.SpinUp: return Quaternion.AngleAxis(-angle, Vector3.right);
                case RotateDirectionType.RollLeft: return Quaternion.AngleAxis(angle, Vector3.forward);
                case RotateDirectionType.RollRight: return Quaternion.AngleAxis(-angle, Vector3.forward);
                case RotateDirectionType.SpinRight: return Quaternion.AngleAxis(angle, Vector3.up);
                case RotateDirectionType.SpinLeft: return Quaternion.AngleAxis(-angle, Vector3.up);
                case RotateDirectionType.EulerAngles: return GetQuaternionRotationFromVector(_brain.Body, _rotationAngles, diff);
                default: return Quaternion.identity;
            }
        }

        protected Quaternion GetQuaternionRotationFromVector(IMonaBody body, Vector3 targetAngles, float step)
        {
            Quaternion currentRotation = body.GetRotation();

            if (step <= _maximumVectorStep)
                return ConvertVectorRotationToQuaternion(currentRotation, targetAngles, step);

            int steps = Mathf.CeilToInt(step / _maximumVectorStep);
            float remainder = step % 1f;
            Quaternion totalRotation = Quaternion.identity;
            
            for (int i = 0; i < steps; i++)
                totalRotation *= ConvertVectorRotationToQuaternion(currentRotation, targetAngles, _maximumVectorStep);

            if (!Mathf.Approximately(0, remainder))
                totalRotation *= ConvertVectorRotationToQuaternion(currentRotation, targetAngles, remainder);

            return totalRotation;
        }

        protected Quaternion ConvertVectorRotationToQuaternion(Quaternion currentRotation, Vector3 targetAngles, float step = 1.0f)
        {
            return Quaternion.Inverse(currentRotation) * Quaternion.Slerp(currentRotation, Quaternion.Euler(targetAngles), step);
        }
    }
}