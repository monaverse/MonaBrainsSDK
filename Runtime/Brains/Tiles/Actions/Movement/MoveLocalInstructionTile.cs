using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Movement.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Input;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Animation;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{

    [Serializable]
    public class MoveLocalInstructionTile : InstructionTile, IMoveLocalInstructionTile, IActionInstructionTile, 
        IPauseableInstructionTile, IActivateInstructionTile, INeedAuthorityInstructionTile,
        IProgressInstructionTile
    {
        public override Type TileType => typeof(MoveLocalInstructionTile);

        public virtual MoveDirectionType DirectionType => MoveDirectionType.Forward;

        [SerializeField] protected float _distance = 1f;
        [SerializeField] protected string _distanceValueName = null;
        [SerializeField] protected string[] _coordinatesName;
        [SerializeField] protected Vector3 _moveToCoordinates = Vector3.zero;
        [SerializeField] protected MovementPlaneType _movementPlane = MovementPlaneType.NorthSouthEastWest;

        [SerializeField] protected MoveModeType _mode = MoveModeType.Time;
        [BrainProperty(false)] public MoveModeType Mode { get => _mode; set => _mode = value; }

        [SerializeField] private float _value = 1f;
        [SerializeField] private string _valueValueName = null;

        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Time)]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Time, "Seconds")]
        [BrainPropertyShowLabel(nameof(Mode), (int)MoveModeType.Speed, "Meters/Sec")]
        [BrainProperty(false)] public float Value { get => _value; set => _value = value; }

        [BrainPropertyValueName("Value", typeof(IMonaVariablesFloatValue))]
        public string ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Speed)]
        [BrainPropertyShow(nameof(Mode), (int)MoveModeType.Time)]
        [BrainPropertyEnum(false)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private bool _useStartDirection = false;
        [BrainProperty(false)] public bool UseStartDirection { get => _useStartDirection; set => _useStartDirection = value; }
        
        private Vector3 _direction;
        private Vector3 _startPosition;

        protected IMonaBrain _brain;
        private IInstruction _instruction;
        private int _tileIndex;

        private Vector3 _start;
        private Vector3 _end;
        private bool _active;
        private bool _listenToInput;
        private MonaInput _bodyInput;
        private IMonaAnimationController _controller;

        private Action<MonaBodyAnimationControllerChangedEvent> OnAnimationControllerChanged;
        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyEvent> OnBodyEvent;

        private bool InstantMovement => _mode == MoveModeType.SpeedOnly || _mode == MoveModeType.Instant;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        protected MovingStateType _movingState;

        public Vector2 InputMoveDirection
        {
            get {
                //Debug.Log($"{nameof(InputMoveDirection)} {_bodyInput.MoveValue}");
                return _bodyInput.MoveValue;
            }
        }

        private string _progressName;

        public float Progress
        {
            get => _brain.Variables.GetFloat(_progressName);
            set => _brain.Variables.Set(_progressName, value);
        }

        public bool InProgress
        {
            get {
                var progress = Progress;
                if (_instruction.CurrentTile != this) return false;
                return progress > 0 && progress <= 1f;
            }
        }

        public MoveLocalInstructionTile() { }
        
        public virtual void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            var pagePrefix = page.IsCore ? "Core" : ("State" + brainInstance.StatePages.IndexOf(page));
            var instructionIndex = page.Instructions.IndexOf(instruction);

            _progressName = $"__{pagePrefix}_{instructionIndex}_progress";

            _brain.Variables.Set(_progressName, 0f);

            UpdateActive();

            OnAnimationControllerChanged = HandleAnimationControllerChanged;
            EventBus.Register<MonaBodyAnimationControllerChangedEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            SetupAnimation();
        }

        private void HandleAnimationControllerChanged(MonaBodyAnimationControllerChangedEvent evt)
        {
            SetupAnimation();
        }

        private void SetupAnimation()
        {
            if (_brain.Root != null)
                _controller = _brain.Root.GetComponent<IMonaAnimationController>();
            else
            {
                var children = _brain.Body.Children();
                for (var i = 0; i < children.Count; i++)
                {
                    var root = children[i].Transform.Find("Root");
                    if (root != null)
                    {
                        _controller = root.GetComponent<IMonaAnimationController>();
                        if (_controller != null) break;
                    }
                }
            }
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

            if (InstantMovement || _movingState == MovingStateType.Moving)
            {
                AddFixedTickDelegate();
            }

            AddInputDelegate();

            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        public override void Unload(bool destroy = false)
        {
            RemoveFixedTickDelegate();

            EventBus.Unregister(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            //if(_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
            //if(_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(Pause)}");
        }

        public bool Resume()
        {
            UpdateActive();
            return _movingState == MovingStateType.Moving;
        }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    if(!InstantMovement) RemoveFixedTickDelegate();
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        private void AddFixedTickDelegate()
        {
            //Debug.Log($"{nameof(AddFixedTickDelegate)}, fr: {Time.frameCount}", _brain.Body.Transform.gameObject);

            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            OnBodyEvent = HandleBodyEvent;
            EventBus.Register<MonaBodyEvent>(new EventHook(MonaCoreConstants.MONA_BODY_EVENT, _brain.Body), OnBodyEvent);
        }

        private void AddInputDelegate()
        { 
            if(DirectionType == MoveDirectionType.UseInput || DirectionType == MoveDirectionType.InputForwardBack || DirectionType == MoveDirectionType.CameraAll)
            {
                _listenToInput = true;
            }
        }

        private void RemoveFixedTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            EventBus.Unregister(new EventHook(MonaBrainConstants.MONA_BRAINS_EVENT, _brain.Body), OnBodyEvent);
        }

        private void HandleBodyEvent(MonaBodyEvent evt)
        {
            if (evt.Type == MonaBodyEventType.OnStop)
                ManualStop();
        }

        public InstructionTileResult Continue()
        {
            Debug.Log($"{nameof(Continue)} take over control and continue executing brain at {Progress}, {_progressName} on ", _brain.Body.ActiveTransform.gameObject);
            _movingState = MovingStateType.Moving;
            AddFixedTickDelegate();
            return Do();
        }

        public Vector3 GetEndPosition(Vector3 pos) => Vector3.zero;

        private float _cooldown;
        private bool _coolingDown;

        public override InstructionTileResult Do()
        {
            UpdateInput();

            _startPosition = _brain.Body.GetPosition();

            _direction = GetDirectionVector(DirectionType);

            _brain.Variables.SetInternal(MonaBrainConstants.LAST_MOVE_DIRECTION, _direction);
            
            _distance = GetDistance();

            if (!string.IsNullOrEmpty(_valueValueName))
                _value = _brain.Variables.GetFloat(_valueValueName);

            if (DirectionType == MoveDirectionType.InputForwardBack && Mathf.Approximately(_bodyInput.MoveValue.y, 0f))
            {
                _movingState = MovingStateType.Stopped;
                StoppedMoving();
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                Progress = 0;
                //Debug.Log($"{nameof(MoveLocalInstructionTile)} DO IT {Name} {_progressName} {Progress}");
                if(!InstantMovement)
                    AddFixedTickDelegate();
            }

            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            if (InstantMovement && _movingState == MovingStateType.Stopped) return;

            UpdateInput();

            Tick(evt.DeltaTime);

            if (_movingState == MovingStateType.Moving || _mode == MoveModeType.Instant || _mode == MoveModeType.SpeedOnly)
            {
                if (_controller != null)
                {
                    switch (_controller.PropertyType)
                    {
                        case MonaBrainPropertyType.GroundedCreature: TickGroundedCreature(evt.DeltaTime); break;
                        default: TickDefault(evt.DeltaTime); break;
                    }
                }
                else
                    TickDefault(evt.DeltaTime);
            }

            if (InstantMovement)
            {
                float step = _mode == MoveModeType.SpeedOnly ? _distance * evt.DeltaTime : _distance;

                //Debug.Log($"{_brain.Body.Transform.gameObject} {_distance} {_direction} step: {step} {Time.frameCount}", _brain.Body.Transform.gameObject);
                _brain.Body.AddPosition(_direction.normalized * step, true);
                StopMoving();
            }

        }

        private void UpdateInput()
        {
            if (!_listenToInput) return;
            if (_movingState != MovingStateType.Moving || InstantMovement)
                _bodyInput = _instruction.InstructionInput;
            //Debug.Log($"{nameof(UpdateInput)} {_bodyInput.MoveValue}");
        }

        private void TickGroundedCreature(float deltaTime)
        {
            if (_controller == null) return;
            var motion = GetMotionSpeed(DirectionType);
            var speed = GetSpeed();
            _controller.SetWalk(speed);
            _controller.SetMotionSpeed(motion);
        }

        private void StopGroundedCreature()
        {
            if (_controller == null) return;
            _controller.SetWalk(0);
        }

        private void TickDefault(float deltaTime)
        {

        }


        private float _timeMoving;
        private float _currentSpeed;
        private Vector3 _lastPosition;
        protected virtual void Tick(float deltaTime)
        {
            if (!_brain.Body.HasControl())
            {
                LostControl();
                return;
            }

            _currentSpeed = Mathf.Abs(Vector3.Distance(_brain.Body.GetPosition(), _lastPosition) / deltaTime);
            _lastPosition = _brain.Body.GetPosition();
            if (_currentSpeed < 0.01f || !ShouldAnimateWalk(DirectionType))
                _currentSpeed = 0;

            switch (_mode)
            {
                case MoveModeType.Time: MoveOverTime(deltaTime); break;
                case MoveModeType.Speed: MoveAtSpeed(deltaTime); break;
            }

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

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        protected float GetSpeed()
        {
            return _currentSpeed;
        }

        protected float GetDistance()
        {
            float distance = _distance;

            if (DirectionType == MoveDirectionType.GlobalCoordinates)
            {
                Vector3 endPosition = _moveToCoordinates;
                if (HasVector3Values(_coordinatesName))
                    endPosition = GetVector3Value(_brain, _coordinatesName);

                distance = Vector3.Distance(endPosition, _startPosition);
            }
            else if (!string.IsNullOrEmpty(_distanceValueName))
            {
                distance = _brain.Variables.GetFloat(_distanceValueName);
            }
                
            return distance;
        }

        private void MoveOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                var progressDelta = Mathf.Round((deltaTime / _value) * 100000f) / 100000f;

                if(!_useStartDirection)
                    _direction = GetDirectionVector(DirectionType);

                Progress = Mathf.Clamp01(Progress);

                float diff = Evaluate(Progress + progressDelta) - Evaluate(Progress);

                if (Progress >= 1f)
                {
                    //if (!(NextExecutionTile is IChangeDefaultInstructionTile))
                    //_brain.Body.Add(_end, !_usePhysics, true);
                    StopMoving();
                }
                else
                {
                    _brain.Body.AddPosition(_direction.normalized * (_distance * diff), true);
                    Progress += progressDelta;
                }
            }
        }

        private void MoveAtSpeed(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                var progressDelta = Mathf.Round((((_value * _speed) / _distance) * deltaTime) * 100000f) / 100000f;

                if (!_useStartDirection)
                    _direction = GetDirectionVector(DirectionType);

                Progress = Mathf.Clamp01(Progress);

                float diff = Evaluate(Mathf.Clamp01(Progress + progressDelta)) - Evaluate(Progress);

                if (Progress >= 1f)
                {
                    StopMoving();
                }
                else
                {
                    _brain.Body.AddPosition(_direction.normalized * (_distance * diff), true);
                    Progress += progressDelta;
                }
            }
        }

        private void LostControl()
        {
            //Debug.Log($"{nameof(MoveLocalInstructionTile)} {nameof(LostControl)}");
            _movingState = MovingStateType.Stopped;
            StoppedMoving();
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void ManualStop()
        {
            //Debug.Log($"{nameof(MoveLocalInstructionTile)} {nameof(LostControl)}");
            _movingState = MovingStateType.Stopped;
            StoppedMoving();
            Complete(InstructionTileResult.Success, true);
        }

        private void StopMoving()
        {
            //Debug.Log($"INPUT stopmoving: {Name} {_progressName} {Progress}");
            _timeMoving = 0;
            _bodyInput = default;
            _movingState = MovingStateType.Stopped;
            StoppedMoving();
            Complete(InstructionTileResult.Success, true);
        }

        protected virtual void StoppedMoving()
        {
            if (_controller == null) return;
            switch (_controller.PropertyType)
            {
                case MonaBrainPropertyType.GroundedCreature: StopGroundedCreature(); break;
                default: StopDefault(); break;
            }
        }

        private void StopDefault()
        {

        }

        protected bool ShouldAnimateWalk(MoveDirectionType moveType)
        {
            switch (moveType)
            {
                case MoveDirectionType.Up: return false;
                case MoveDirectionType.Down: return false;
                //case MoveDirectionType.UseInput: return Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y);
                //case MoveDirectionType.InputForwardBack: return Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y);
                case MoveDirectionType.Y: return false;
                case MoveDirectionType.YNegative: return false;
                //case MoveDirectionType.CameraAll: return Mathf.Approximately(InputMoveDirection.y, 0) && Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(Mathf.Abs(InputMoveDirection.y) + Mathf.Abs(InputMoveDirection.x));
                
                case MoveDirectionType.CameraUp: return false;
                case MoveDirectionType.CameraDown: return false;
                //case MoveDirectionType.CameraTruePlanar: return Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y);
                
                case MoveDirectionType.CameraTrueUp: return false;
                case MoveDirectionType.CameraTrueDown: return false;
                default: return true;
            }
        }

        protected float GetMotionSpeed(MoveDirectionType moveType)
        {
            switch (moveType)
            {
                case MoveDirectionType.Forward: return 1f;
                case MoveDirectionType.Backward: return -1f;
                case MoveDirectionType.Up: return 1f;
                case MoveDirectionType.Down: return -1f;
                case MoveDirectionType.Right: return 1f;
                case MoveDirectionType.Left: return -1f;
                case MoveDirectionType.UseInput: return Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y);
                case MoveDirectionType.InputForwardBack: return Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y);
                case MoveDirectionType.X: return 1f;
                case MoveDirectionType.Y: return 1f;
                case MoveDirectionType.Z: return 1f;
                case MoveDirectionType.XNegative: return -1f;
                case MoveDirectionType.YNegative: return -1f;
                case MoveDirectionType.ZNegative: return -1f;
                case MoveDirectionType.CameraAll: return Mathf.Approximately(InputMoveDirection.y, 0) && Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(Mathf.Abs(InputMoveDirection.y)+Mathf.Abs(InputMoveDirection.x));
                case MoveDirectionType.CameraForward: return 1f;
                case MoveDirectionType.CameraBackward: return 1f;
                case MoveDirectionType.CameraRight: return 1f;
                case MoveDirectionType.CameraLeft: return 1f;
                case MoveDirectionType.CameraUp: return 1f;
                case MoveDirectionType.CameraDown: return 1f;
                case MoveDirectionType.CameraTruePlanar: return Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y);
                case MoveDirectionType.CameraTrueForward: return 1f;
                case MoveDirectionType.CameraTrueBackward: return 1f;
                case MoveDirectionType.CameraTrueRight: return 1f;
                case MoveDirectionType.CameraTrueLeft: return 1f;
                case MoveDirectionType.CameraTrueUp: return 1f;
                case MoveDirectionType.CameraTrueDown: return 1f;
                default: return 0;
            }
        }

        protected virtual Vector3 GetDirectionVector(MoveDirectionType moveType)
        {
            switch (moveType)
            {
                case MoveDirectionType.Forward: return _brain.Body.ActiveTransform.forward;
                case MoveDirectionType.Backward: return _brain.Body.ActiveTransform.forward * -1f;
                case MoveDirectionType.Up: return _brain.Body.ActiveTransform.up;
                case MoveDirectionType.Down: return _brain.Body.ActiveTransform.up * -1f;
                case MoveDirectionType.Right: return _brain.Body.ActiveTransform.right;
                case MoveDirectionType.Left: return _brain.Body.ActiveTransform.right * -1f;
                case MoveDirectionType.UseInput: return _brain.Body.ActiveTransform.forward * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y)) + _brain.Body.ActiveTransform.right * (Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(InputMoveDirection.x));
                case MoveDirectionType.InputForwardBack: return _brain.Body.ActiveTransform.forward * Mathf.Sign(InputMoveDirection.y);
                case MoveDirectionType.X: return Vector3.right;
                case MoveDirectionType.Y: return Vector3.up;
                case MoveDirectionType.Z: return Vector3.forward;
                case MoveDirectionType.XNegative: return Vector3.right * -1f;
                case MoveDirectionType.YNegative: return Vector3.up * -1f;
                case MoveDirectionType.ZNegative: return Vector3.forward * -1f;
                case MoveDirectionType.CameraAll: return CameraMovementVector();
                case MoveDirectionType.CameraForward: return FlattenVector(MonaGlobalBrainRunner.Instance.SceneCamera.transform.forward);
                case MoveDirectionType.CameraBackward: return FlattenVector(MonaGlobalBrainRunner.Instance.SceneCamera.transform.forward * -1f);
                case MoveDirectionType.CameraRight: return FlattenVector(MonaGlobalBrainRunner.Instance.SceneCamera.transform.right);
                case MoveDirectionType.CameraLeft: return FlattenVector(MonaGlobalBrainRunner.Instance.SceneCamera.transform.right * -1f);
                case MoveDirectionType.CameraUp: return Vector3.up * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y));
                case MoveDirectionType.CameraDown: return Vector3.up * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y) * -1f);
                case MoveDirectionType.CameraTruePlanar: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.forward * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y)) + MonaGlobalBrainRunner.Instance.SceneCamera.transform.right * (Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(InputMoveDirection.x));
                case MoveDirectionType.CameraTrueForward: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.forward;
                case MoveDirectionType.CameraTrueBackward: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.forward * -1f;
                case MoveDirectionType.CameraTrueRight: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.right;
                case MoveDirectionType.CameraTrueLeft: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.right * -1f;
                case MoveDirectionType.CameraTrueUp: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.up;
                case MoveDirectionType.CameraTrueDown: return MonaGlobalBrainRunner.Instance.SceneCamera.transform.up * -1f;
                case MoveDirectionType.GlobalCoordinates: return MoveToDirection();

                default: return Vector3.zero;
            }
        }

        private Vector3 CameraMovementVector()
        {
            Vector3 cameraRight = MonaGlobalBrainRunner.Instance.SceneCamera.transform.right;
            Vector3 cameraUp = MonaGlobalBrainRunner.Instance.SceneCamera.transform.up;
            Vector3 cameraForward = MonaGlobalBrainRunner.Instance.SceneCamera.transform.forward;

            switch ((int)_movementPlane)
            {
                case 0:
                    cameraForward.y = 0;
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();
                    return cameraForward * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y)) + cameraRight * (Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(InputMoveDirection.x));
                case 1:
                    cameraRight.x = 0;
                    cameraUp.x = 0;
                    cameraRight.Normalize();
                    cameraUp.Normalize();
                    return cameraUp * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y)) + cameraRight * (Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(InputMoveDirection.x));
                case 2:
                    cameraRight.z = 0;
                    cameraUp.z = 0;
                    cameraRight.Normalize();
                    cameraUp.Normalize();
                    return cameraUp * (Mathf.Approximately(InputMoveDirection.y, 0) ? 0 : Mathf.Sign(InputMoveDirection.y)) + cameraRight * (Mathf.Approximately(InputMoveDirection.x, 0) ? 0 : Mathf.Sign(InputMoveDirection.x));
            }

            return Vector3.zero;
        }

        private Vector3 FlattenVector(Vector3 vector)
        {
            vector.y = 0;
            vector.Normalize();
            return vector;
        }

        private Vector3 MoveToDirection()
        {
            Vector3 endPosition = _moveToCoordinates;

            if (HasVector3Values(_coordinatesName))
                endPosition = GetVector3Value(_brain, _coordinatesName);

            return endPosition - _startPosition;
        }
    }
}