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
using Mona.SDK.Core.Utils;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class ScaleLocalInstructionTile : InstructionTile, IActionInstructionTile, IPauseableInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IActivateInstructionTile, INeedAuthorityInstructionTile, IProgressInstructionTile, IRotateLocalInstructionTile, ITickAfterInstructionTile

    {
        public const string ID = "Scale To";
        public const string NAME = "Scale To";
        public const string CATEGORY = "Scale";
        public override Type TileType => typeof(ScaleLocalInstructionTile);

        [SerializeField] protected Vector3 _scale = Vector3.zero;
        [SerializeField] protected string[] _scaleValueName;

        [BrainProperty(true)] public Vector3 Scale { get => _scale; set => _scale = value; }
        [BrainPropertyValueName("Scale", typeof(IMonaVariablesVector3Value))] public string[] ScaleValueName { get => _scaleValueName; set => _scaleValueName = value; }

        [SerializeField] private MoveModeType _mode = MoveModeType.Time;
        [BrainPropertyEnum(false)] public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName = null;

        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Time)]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Time, "Seconds")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Amount/Sec")]
        [BrainProperty(false)] public float Value { get => _value; set => _value= value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))] public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Time)]
        [BrainPropertyEnum(false)] public EasingType Easing { get => _easing; set => _easing = value; }

        private Vector3 _startScale;

        protected IMonaBrain _brain;
        private string _progressName;

        private Quaternion _start;
        private Quaternion _end;

        private bool _active;

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

        public ScaleLocalInstructionTile() { }

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
            if (!_active)
            {
                if (_movingState == MovingStateType.Moving)
                    LostControl();
                return;
            }

            if (_movingState == MovingStateType.Moving)
            {
                AddFixedTickDelegate();
            }

        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return _movingState == MovingStateType.Moving;
        }


        public override void Unload(bool destroy = false)
        {
            RemoveFixedTickDelegate();
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
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddFixedTickDelegate()
        {
            //Debug.Log($"{nameof(RotateLocalInstructionTile)}.{nameof(AddFixedTickDelegate)}, {_brain.Body.ActiveTransform.name}", _brain.Body.ActiveTransform.gameObject);
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
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

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);

            if (_movingState == MovingStateType.Stopped)
            {
                Progress = 0;
                StartScale();
                AddFixedTickDelegate();
            }
            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
        }

        protected virtual void StartScale()
        {
            _startScale = _brain.Body.GetScale();
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
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
                if (HasVector3Values(_scaleValueName))
                    _scale = GetVector3Value(_brain, _scaleValueName);

                StartScale();

                Vector3 step = _mode == MoveModeType.SpeedOnly ? _scale * deltaTime : _scale;

                _brain.Body.SetScale(step, true);
                StopMoving();
            }
        }

        private void UpdateInput()
        {
            if (!_listenToInput) return;
            if (_movingState != MovingStateType.Moving)
                _bodyInput = _instruction.InstructionInput;
            Debug.Log($"{nameof(UpdateInput)} {_bodyInput.MoveValue}");
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

                if (HasVector3Values(_scaleValueName))
                    _scale = GetVector3Value(_brain, _scaleValueName);

                //if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                _brain.Body.SetScale(Vector3.Lerp(_startScale, _scale, Progress), true);

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
                if (!string.IsNullOrEmpty(_valueValueName))
                    _value = _brain.Variables.GetFloat(_valueValueName);

                var progressDelta = Mathf.Round(((_value / ((1f / _value) * _speed)) * deltaTime) * 1000f) / 1000f;

                Progress = Mathf.Clamp01(Progress);

                float diff = Evaluate(Mathf.Clamp01(Progress + progressDelta)) - Evaluate(Progress);

                //if (!(NextExecutionTile is IChangeDefaultRotationInstructionTile))
                _brain.Body.SetScale(Vector3.Lerp(_startScale, _scale, Progress), true);

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

    }
}