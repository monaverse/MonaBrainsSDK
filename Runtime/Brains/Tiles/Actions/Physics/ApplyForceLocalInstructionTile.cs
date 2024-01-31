using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{

    [Serializable]
    public class ApplyForceLocalInstructionTile : InstructionTile, IApplyForceLocalInstructionTile, IActionInstructionTile, IPauseableInstructionTile
    {
        public override Type TileType => typeof(ApplyForceLocalInstructionTile);

        public virtual PushDirectionType DirectionType => PushDirectionType.Forward;

        [SerializeField] private float _force = 1f;
        [SerializeField] private string _forceValueName = null;

        [BrainProperty(true)] public float Force { get => _force; set => _force = value; }
        [BrainPropertyValueName("Force")] public string ForceValueName { get => _forceValueName; set => _forceValueName = value; }

        [SerializeField] private float _duration = 0f;
        [SerializeField] private string _durationValueName = null;
        [BrainPropertyEnum(true)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName("Duration")] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private DragType _dragType = DragType.Quadratic;
        [BrainPropertyEnum(false)] public DragType DragType { get => _dragType; set => _dragType = value; }

        [SerializeField] private float _drag = .2f;
        [BrainProperty(false)] public float Drag { get => _drag; set => _drag = value; }

        [SerializeField] private float _angularDrag = .2f;
        [BrainProperty(false)] public float AngularDrag { get => _angularDrag; set => _angularDrag = value; }

        [SerializeField] [Range(0, 1)] private float _friction = .5f;
        [BrainProperty(false)] public float Friction { get => _friction; set => _friction = value; }

        [SerializeField] [Range(0, 1)] private float _bounce = .2f;
        [BrainProperty(false)] public float Bounce { get => _bounce; set => _bounce = value; }

        private Vector3 _direction;

        private IMonaBrain _brain;

        private MovingStateType _movingState;
        private float _time;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private float _speed
        {
            get => _brain.State.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public Vector2 InputMoveDirection
        {
            get => _brain.State.GetVector2(MonaBrainConstants.RESULT_MOVE_DIRECTION);
        }
        
        public ApplyForceLocalInstructionTile() { }
        
        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override void Unload()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.{nameof(Pause)}");
        }

        public void Resume()
        {
            if(_movingState == MovingStateType.Moving)
            {
                EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
                if (_brain.LoggingEnabled) 
                    Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.{nameof(Resume)}");
            }
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    EventBus.Unregister(new EventHook(MonaBrainConstants.TILE_TICK_EVENT), OnFixedTick);
                    if(thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public override InstructionTileResult Do()
        {
            IMonaBody target = _brain.Body;
            switch(DirectionType)
            {
                case PushDirectionType.Pull:
                case PushDirectionType.Push:
                case PushDirectionType.Toward:
                case PushDirectionType.Away:
                    var value = _brain.State.GetValue(MonaBrainConstants.RESULT_TARGET);
                    if (value is IMonaStateBrainValue)
                        target = ((IMonaStateBrainValue)value).Value.Body;
                    else
                        target = ((IMonaStateBodyValue)value).Value;
                    break;
            }

            _direction = GetDirectionVector(DirectionType, target);
            //Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.Do {DirectionType}");

            if (!string.IsNullOrEmpty(_forceValueName))
                _force = _brain.State.GetFloat(_forceValueName);

            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.State.GetFloat(_durationValueName);

            if (_duration == 0f)
            {
                _brain.Body.SetDragType(_dragType);
                _brain.Body.SetDrag(_drag);
                _brain.Body.SetAngularDrag(_angularDrag);
                _brain.Body.SetFriction(_friction);
                _brain.Body.SetBounce(_bounce);

                _brain.Body.ApplyForce(_direction.normalized * _force, ForceMode.Impulse, true);
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                _time = 0;
                
                OnFixedTick = HandleFixedTick;
                EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.FIXED_TICK_EVENT), OnFixedTick);
            }

            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            Tick(evt.DeltaTime);
        }

        private void Tick(float deltaTime)
        {
            PushOverTime(deltaTime);
        }

        private void PushOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                _brain.Body.SetDragType(_dragType);
                _brain.Body.SetDrag(_drag);
                _brain.Body.SetAngularDrag(_angularDrag);
                _brain.Body.SetFriction(_friction);
                _brain.Body.SetBounce(_bounce);

                _brain.Body.ApplyForce(_direction * deltaTime, ForceMode.Acceleration, true);

                if(_time >= 1f)
                {
                    StopPushing();
                }
            }
        }

        private void StopPushing()
        {
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }

        private Vector3 GetDirectionVector(PushDirectionType moveType, IMonaBody body)
        {
            switch (moveType)
            {
                case PushDirectionType.Forward: return _brain.Body.ActiveTransform.forward;
                case PushDirectionType.Backward: return _brain.Body.ActiveTransform.forward * -1f;
                case PushDirectionType.Up: return _brain.Body.ActiveTransform.up;
                case PushDirectionType.Down: return _brain.Body.ActiveTransform.up * -1f;
                case PushDirectionType.Right: return _brain.Body.ActiveTransform.right;
                case PushDirectionType.Left: return _brain.Body.ActiveTransform.right * -1f;
                case PushDirectionType.Push: return body.GetPosition() - _brain.Body.GetPosition();
                case PushDirectionType.Pull: return _brain.Body.GetPosition() - body.GetPosition();
                case PushDirectionType.Away: return _brain.Body.GetPosition() - body.GetPosition();
                case PushDirectionType.Toward: return body.GetPosition() - _brain.Body.GetPosition();
                case PushDirectionType.UseInput: return InputMoveDirection;
                default: return Vector3.zero;
            }
        }

    }
}