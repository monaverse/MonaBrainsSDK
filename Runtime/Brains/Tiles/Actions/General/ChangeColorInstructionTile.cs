using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;

namespace Mona.SDK.Brains.Tiles.Actions.General
{

    [Serializable]
    public class ChangeColorInstructionTile : InstructionTile, IChangeColorInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile,
        IActivateInstructionTile, IPauseableInstructionTile, IProgressInstructionTile
    {
        public const string ID = "ChangeColor";
        public const string NAME = "Change Color";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeColorInstructionTile);

        public virtual MoveDirectionType DirectionType => MoveDirectionType.Forward;

        [SerializeField] private Color _color = Color.white;
        [BrainProperty(true)] public Color Color { get => _color; set => _color = value; }

        [SerializeField] private float _duration = 1f;
        [SerializeField] private string _durationValueName = null;

        [BrainProperty(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName("Duration", typeof(IMonaVariablesFloatValue))] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyEnum(false)] public EasingType Easing { get => _easing; set => _easing = value; }

        private Vector3 _direction;

        private IMonaBrain _brain;
        private IInstruction _instruction;
        private string _progressName;


        private Color _start;
        private Color _end;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaInputEvent> OnInput;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState;
        private bool _active;
        private MonaInput _brainInput;

        public Vector2 InputMoveDirection
        {
            get => _brainInput.MoveValue;
        }

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

        public ChangeColorInstructionTile() { }
        
        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            var pagePrefix = page.IsCore ? "Core" : ("State" + brainInstance.StatePages.IndexOf(page));
            var instructionIndex = page.Instructions.IndexOf(instruction);

            _progressName = $"__{pagePrefix}_{instructionIndex}_progress";

            _brain.Variables.GetFloat(_progressName);

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

            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        public override void Unload(bool destroy = false)
        {
            RemoveDelegates();
            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveDelegates();
            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(Pause)}");
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

        public override void SetThenCallback(InstructionTileCallback thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback = thenCallback;
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback;
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveDelegates();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }


        private void AddDelegates()
        {
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            if (DirectionType == MoveDirectionType.UseInput)
            {
                OnInput = HandleBodyInput;
                EventBus.Register<MonaInputEvent>(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
            }
        }

        private void RemoveDelegates()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            EventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public InstructionTileResult Continue()
        {
            Debug.Log($"{nameof(Continue)} take over control and continue executing brain at {Progress}, {_progressName} on ", _brain.Body.ActiveTransform.gameObject);
            _movingState = MovingStateType.Moving;
            _start = _brain.Body.GetColor();
            _end = _color;
            AddDelegates();
            return Do();
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.Variables.GetFloat(_durationValueName);

            if (_duration == 0)
            {
                _brain.Body.SetColor(_color, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                Progress = 0;
                _start = _brain.Body.GetColor();
                _end = _color;
                AddDelegates();
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

            MoveOverTime(deltaTime);
        }

        private void LostControl()
        {
            Debug.Log($"{nameof(ChangeColorInstructionTile)} {nameof(LostControl)}");
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.LostAuthority, true);
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
                if(Progress >= 1f)
                {
                    _brain.Body.SetColor(_end, true);
                    StopMoving();
                }
                else
                {
                    _brain.Body.SetColor(Color.Lerp(_start, _end, Evaluate(Progress)), true);
                }
                Progress += deltaTime / _duration;
            }
        }

        private void StopMoving()
        {
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }
    }
}