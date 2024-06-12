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
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{

    [Serializable]
    public class ApplyTorqueLocalInstructionTile : InstructionTile, IActionInstructionTile, IPauseableInstructionTile, INeedAuthorityInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IActivateInstructionTile, IRigidbodyInstructionTile
    {
        public override Type TileType => typeof(ApplyTorqueLocalInstructionTile);

        public virtual PushDirectionType DirectionType => PushDirectionType.Forward;

        [SerializeField] private float _torque = 1f;
        [SerializeField] private string _torqueValueName = null;

        [BrainProperty(true)] public float Torque { get => _torque; set => _torque = value; }
        [BrainPropertyValueName(nameof(Torque), typeof(IMonaVariablesFloatValue))] public string TorqueValueName { get => _torqueValueName; set => _torqueValueName = value; }

        [SerializeField] private float _duration = 0f;
        [SerializeField] private string _durationValueName = null;
        [BrainPropertyEnum(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName(nameof(Duration), typeof(IMonaVariablesFloatValue))] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private float _maxSpeed = .2f;
        [SerializeField] private string _maxSpeedName;
        [BrainProperty(false)] public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }
        [BrainPropertyValueName(nameof(MaxSpeed), typeof(IMonaVariablesFloatValue))] public string MaxSpeedName { get => _maxSpeedName; set => _maxSpeedName = value; }

        private Vector3 _direction;

        protected IMonaBrain _brain;

        private MovingStateType _movingState;
        private float _time;

        private MonaInput _bodyInput;
        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool _listenToInput;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public Vector2 InputMoveDirection
        {
            get => _bodyInput.MoveValue;
        }
        
        public ApplyTorqueLocalInstructionTile() { }
        
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
                Debug.Log($"{nameof(ApplyTorqueLocalInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
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
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        public override void Unload(bool destroy = false)
        {
            RemoveFixedTickDelegate();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyTorqueLocalInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(ApplyTorqueLocalInstructionTile)}.{nameof(Pause)}");
        }

        public bool Resume()
        {
            UpdateActive();
            return false;
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

        protected virtual bool ApplyTorqueToTarget()
        {
            return false;
        }

        public override InstructionTileResult Do()
        {
            UpdateInput();

            var target = GetTarget();

            _direction = GetDirectionVector(DirectionType, target);
            //Debug.Log($"{nameof(ApplyTorqueLocalInstructionTile)}.Do {DirectionType}");

            if (!string.IsNullOrEmpty(_torqueValueName))
                _torque = _brain.Variables.GetFloat(_torqueValueName);

            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.Variables.GetFloat(_durationValueName);

            if (!string.IsNullOrEmpty(_maxSpeedName))
                _maxSpeed = _brain.Variables.GetFloat(_maxSpeedName);

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
                if (ApplyTorqueToTarget())
                    body = target;
                //body.SetDragType(_dragType);
                //body.SetDrag(_drag);
                //body.SetAngularDrag(_angularDrag);
                //body.SetFriction(_friction);
                //body.SetBounce(_bounce);
                body.SetKinematic(false, true);

                Pushed();
                //if (_brain.LoggingEnabled)
                //Debug.Log($"ApplyTorque to Body {body.ActiveTransform.name} {InputMoveDirection} {_direction} {_direction.normalized * _force}", body.ActiveTransform.gameObject);

                body.ApplyTorque(_direction.normalized * _torque, ForceMode.Impulse, true);
                body.ActiveRigidbody.maxAngularVelocity = _maxSpeed;
                if (!body.ActiveRigidbody.isKinematic)
                    body.ActiveRigidbody.angularVelocity = Vector3.ClampMagnitude(body.ActiveRigidbody.angularVelocity, _maxSpeed);
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
                if (ApplyTorqueToTarget())
                    body = GetTarget();
                //body.SetDragType(_dragType);
                //body.SetDrag(_drag);
                //body.SetAngularDrag(_angularDrag);
                //body.SetFriction(_friction);
                //body.SetBounce(_bounce);
                body.SetKinematic(false, true);

                //if (_brain.LoggingEnabled)
                    Debug.Log($"ApplyTorque to Body over time {_duration} {body.ActiveTransform.name} {_direction.normalized * _torque} {body.ActiveRigidbody.angularVelocity.magnitude}", body.ActiveTransform.gameObject);

                body.ApplyTorque(_direction.normalized * _torque, ForceMode.Impulse, true);
                body.ActiveRigidbody.maxAngularVelocity = _maxSpeed;
                if(!body.ActiveRigidbody.isKinematic)
                    body.ActiveRigidbody.angularVelocity = Vector3.ClampMagnitude(body.ActiveRigidbody.angularVelocity, _maxSpeed);

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

        protected virtual Vector3 GetDirectionVector(PushDirectionType moveType, IMonaBody body)
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
                case PushDirectionType.RollRight: return _brain.Body.ActiveTransform.forward * -1f;
                case PushDirectionType.RollLeft: return _brain.Body.ActiveTransform.forward;
                case PushDirectionType.PitchUp: return _brain.Body.ActiveTransform.right * -1f;
                case PushDirectionType.PitchDown: return _brain.Body.ActiveTransform.right;
                case PushDirectionType.TurnRight: return _brain.Body.ActiveTransform.up;
                case PushDirectionType.TurnLeft: return _brain.Body.ActiveTransform.up * -1f;


                default: return Vector3.zero;
            }
        }

    }
}