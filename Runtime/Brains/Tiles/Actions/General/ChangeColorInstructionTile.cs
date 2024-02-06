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

namespace Mona.SDK.Brains.Tiles.Actions.General
{

    [Serializable]
    public class ChangeColorInstructionTile : InstructionTile, IChangeColorInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile,
        IActivateInstructionTile, IPauseableInstructionTile
    {
        public const string ID = "ChangeColor";
        public const string NAME = "Change Color";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeColorInstructionTile);

        public virtual MoveDirectionType DirectionType => MoveDirectionType.Forward;

        [SerializeField] private Color _color = Color.white;
        [BrainProperty(true)] public Color Color { get => _color; set => _color = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyEnum(true)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private float _duration = 1f;
        [SerializeField] private string _durationValueName = null;

        [BrainProperty(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName("Duration")] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        private Vector3 _direction;

        private IMonaBrain _brain;

        private Color _start;
        private Color _end;
        private float _time;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaInputEvent> OnInput;

        private float _speed
        {
            get => _brain.State.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState;
        private bool _active;
        private MonaInput _brainInput;

        public Vector2 InputMoveDirection
        {
            get => _brainInput.MoveValue;
        }
        
        public ChangeColorInstructionTile() { }
        
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
            if (!_active)
            {
                RemoveDelegates();
                return;
            }

            if (_movingState == MovingStateType.Moving)
            {
                AddDelegates();
            }

            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        public override void Unload()
        {
            RemoveDelegates();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveDelegates();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(Pause)}");
        }

        public void Resume()
        {
            UpdateActive();
        }

        private void HandleBodyInput(MonaInputEvent evt)
        {
            _brainInput = evt.Input;
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    RemoveDelegates();
                    if(thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
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

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.State.GetFloat(_durationValueName);

            if (_duration == 0)
            {
                _brain.Body.SetColor(_color, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                _time = 0;
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
            MoveOverTime(deltaTime);
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
                _time += deltaTime / _duration;
                _brain.Body.SetColor(Color.Lerp(_start, _end, Evaluate(_time)), true);

                if(_time >= 1f)
                {
                    _brain.Body.SetColor(_end, true);
                    StopMoving();
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