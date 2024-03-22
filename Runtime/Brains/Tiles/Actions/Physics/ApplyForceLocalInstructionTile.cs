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
using Mona.SDK.Core.Input;
using Mona.SDK.Brains.Core.Control;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{

    [Serializable]
    public class ApplyForceLocalInstructionTile : InstructionTile, IApplyForceLocalInstructionTile, IActionInstructionTile, IPauseableInstructionTile, INeedAuthorityInstructionTile,
        IActivateInstructionTile, IRigidbodyInstructionTile
    {
        public override Type TileType => typeof(ApplyForceLocalInstructionTile);

        public virtual PushDirectionType DirectionType => PushDirectionType.Forward;

        [SerializeField] private float _force = 1f;
        [SerializeField] private string _forceValueName = null;

        [BrainProperty(true)] public float Force { get => _force; set => _force = value; }
        [BrainPropertyValueName("Force", typeof(IMonaVariablesFloatValue))] public string ForceValueName { get => _forceValueName; set => _forceValueName = value; }

        [SerializeField] private float _duration = 0f;
        [SerializeField] private string _durationValueName = null;
        [BrainPropertyEnum(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName("Duration", typeof(IMonaVariablesFloatValue))] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private float _maxSpeed = .2f;
        [BrainProperty(false)] public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }

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

        protected IMonaBrain _brain;

        private MovingStateType _movingState;
        private float _time;

        private MonaInput _bodyInput;
        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool _listenToInput;
        private IInstruction _instruction;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public Vector2 InputMoveDirection
        {
            get => _bodyInput.MoveValue;
        }
        
        public ApplyForceLocalInstructionTile() { }
        
        public virtual void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            if (!_brain.Body.HasCollider())
                _brain.Body.AddCollider();

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
                RemoveFixedTickDelegate();
                return;
            }

            if (_movingState == MovingStateType.Moving)
            {
                AddFixedTickDelegate();
            }

            AddInputDelegate();

            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void AddInputDelegate()
        {
            if (DirectionType == PushDirectionType.UseInput || DirectionType == PushDirectionType.InputForwardBack)
            {
                _listenToInput = true;
            }
        }

        private void RemoveFixedTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        public override void Unload()
        {
            RemoveFixedTickDelegate();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.{nameof(Pause)}");
        }

        public bool Resume()
        {
            UpdateActive();
            return false;
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    RemoveFixedTickDelegate();
                    if(thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        protected virtual IMonaBody GetTarget()
        {
            IMonaBody target = _brain.Body;
            switch (DirectionType)
            {
                case PushDirectionType.Toward:
                case PushDirectionType.Away:
                    var variables = _brain.Variables.GetVariable(MonaBrainConstants.RESULT_TARGET);
                    if (variables is IMonaVariablesBrainValue)
                        target = ((IMonaVariablesBrainValue)variables).Value.Body;
                    else
                        target = ((IMonaVariablesBodyValue)variables).Value;
                    break;
            }
            return target;
        }

        protected virtual bool ApplyForceToTarget()
        {
            return false;
        }

        public override InstructionTileResult Do()
        {
            UpdateInput();

            var target = GetTarget();

            _direction = GetDirectionVector(DirectionType, target);
            //Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.Do {DirectionType}");

            if (!string.IsNullOrEmpty(_forceValueName))
                _force = _brain.Variables.GetFloat(_forceValueName);

            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.Variables.GetFloat(_durationValueName);

            if (_movingState == MovingStateType.Stopped)
            {
                _time = 0;
                Pushed();
                AddFixedTickDelegate();
            }

            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
        }

        protected virtual void Pushed()
        {

        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            UpdateInput();

            Tick(evt.DeltaTime);

            if (_duration == 0f)
            {
                var target = GetTarget();
                var body = _brain.Body;
                if (ApplyForceToTarget())
                    body = target;
                body.SetDragType(_dragType);
                body.SetDrag(_drag);
                body.SetAngularDrag(_angularDrag);
                body.SetFriction(_friction);
                body.SetBounce(_bounce);
                body.SetKinematic(false, true);

                Pushed();
                //if (_brain.LoggingEnabled)
                //Debug.Log($"ApplyForce to Body {body.ActiveTransform.name} {InputMoveDirection} {_direction} {_direction.normalized * _force}", body.ActiveTransform.gameObject);

                body.ApplyForce(_direction.normalized * _force, ForceMode.Impulse, true);
                body.ActiveRigidbody.velocity = Vector3.ClampMagnitude(body.ActiveRigidbody.velocity, _maxSpeed);
                StopPushing();
            }

        }

        private void UpdateInput()
        {
            if (!_listenToInput) return;
            if(_movingState != MovingStateType.Moving || _duration == 0f)
                _bodyInput = _instruction.InstructionInput;
            Debug.Log($"{nameof(UpdateInput)} {_bodyInput.MoveValue}");
        }

        protected virtual void Tick(float deltaTime)
        {
            PushOverTime(deltaTime);
        }
        
        public virtual IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        private void PushOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving && _duration > 0f)
            {
                IMonaBody body = _brain.Body;
                if (ApplyForceToTarget())
                    body = GetTarget();
                body.SetDragType(_dragType);
                body.SetDrag(_drag);
                body.SetAngularDrag(_angularDrag);
                body.SetFriction(_friction);
                body.SetBounce(_bounce);
                body.SetKinematic(false, true);

                //if (_brain.LoggingEnabled)
                //    Debug.Log($"ApplyForce to Body over time {_duration} {body.ActiveTransform.name} {_direction.normalized * _force * deltaTime}", body.ActiveTransform.gameObject);

                body.ApplyForce(_direction.normalized * _force, ForceMode.Impulse, true);
                body.ActiveRigidbody.velocity = Vector3.ClampMagnitude(body.ActiveRigidbody.velocity, _maxSpeed);

                if (_time >= 1f)
                {
                    StopPushing();
                }

                _time += deltaTime;
            }
        }

        private void StopPushing()
        {
            _movingState = MovingStateType.Stopped;
            StoppedPushing();
            Complete(InstructionTileResult.Success, true);
        }

        protected virtual void StoppedPushing()
        {

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
                case PushDirectionType.UseInput: return _brain.Body.ActiveTransform.forward * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y)) + _brain.Body.ActiveTransform.right * (Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(InputMoveDirection.x));
                case PushDirectionType.InputForwardBack: return _brain.Body.ActiveTransform.forward * Mathf.Sign(InputMoveDirection.y);

                default: return Vector3.zero;
            }
        }

    }
}