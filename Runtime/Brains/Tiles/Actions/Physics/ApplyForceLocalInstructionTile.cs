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
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{

    [Serializable]
    public class ApplyForceLocalInstructionTile : InstructionTile, IApplyForceLocalInstructionTile, IActionInstructionTile, IPauseableInstructionTile, INeedAuthorityInstructionTile,
        IActivateInstructionTile, IRigidbodyInstructionTile
    {
        public override Type TileType => typeof(ApplyForceLocalInstructionTile);

        public virtual PushDirectionType DirectionType => PushDirectionType.Forward;
        public virtual PositionalAlignmentMode AlignmentMode => PositionalAlignmentMode.Hover;

        [SerializeField] private float _force = 1f;
        [SerializeField] private string _forceValueName = null;
        [BrainProperty(true)] public float Force { get => _force; set => _force = value; }
        [BrainPropertyValueName("Force", typeof(IMonaVariablesFloatValue))] public string ForceValueName { get => _forceValueName; set => _forceValueName = value; }

        [SerializeField] private float _distance = 5;
        [SerializeField] private string _distanceName;
        [BrainPropertyShow(nameof(DisplayDistance), (int)DisplayType.Display)]
        [BrainProperty(true)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceName { get => _distanceName; set => _distanceName = value; }

        [SerializeField] private BodyAlignmentDirection _alignmentDirection = BodyAlignmentDirection.Downward;
        [BrainPropertyShow(nameof(DisplayAlignmentDirection), (int)DisplayType.Display)]
        [BrainPropertyEnum(true)] public BodyAlignmentDirection AlignmentDirection { get => _alignmentDirection; set => _alignmentDirection = value; }

        [SerializeField] private string _directionTag = "Player";
        [BrainPropertyShow(nameof(DisplayDirectionTag), (int)DisplayType.Display)]
        [BrainPropertyMonaTag(true)] public string DirectionTag { get => _directionTag; set => _directionTag = value; }

        [SerializeField] private Vector3 _directionVector = Vector3.down;
        [SerializeField] private string[] _directionVectorName;
        [BrainPropertyShow(nameof(DisplayDirectionVector), (int)DisplayType.Display)]
        [BrainProperty(true)] public Vector3 DirectionVector { get => _directionVector; set => _directionVector = value; }
        [BrainPropertyValueName("DirectionVector", typeof(IMonaVariablesVector3Value))] public string[] DirectionVectorName { get => _directionVectorName; set => _directionVectorName = value; }

        [SerializeField] private float _duration = 0f;
        [SerializeField] private string _durationValueName = null;
        [BrainPropertyEnum(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName("Duration", typeof(IMonaVariablesFloatValue))] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private float _damping = 5;
        [SerializeField] private string _dampingName;
        [BrainPropertyShow(nameof(DirectionType), (int)PushDirectionType.PositionalAlignment)]
        [BrainProperty(false)] public float Damping { get => _damping; set => _damping = value; }
        [BrainPropertyValueName("Damping", typeof(IMonaVariablesFloatValue))] public string DampingName { get => _dampingName; set => _dampingName = value; }

        [SerializeField] private TargetAlignmentGeometry _alignmentGeometry = TargetAlignmentGeometry.Any;
        [BrainPropertyShow(nameof(DisplayGeometryAlignment), (int)DisplayType.Display)]
        [BrainPropertyEnum(false)] public TargetAlignmentGeometry AlignmentGeometry { get => _alignmentGeometry; set => _alignmentGeometry = value; }

        [SerializeField] private string _geometryTag = "Player";
        [BrainPropertyShow(nameof(DisplayGeometryTag), (int)DisplayType.Display)]
        [BrainPropertyMonaTag(false)] public string GeometryTag { get => _geometryTag; set => _geometryTag = value; }

        [SerializeField] private AlignmentAxes _alignmentAxis = AlignmentAxes.XZ;
        [BrainPropertyShow(nameof(DisplayAlignmentAxis), (int)DisplayType.Display)]
        [BrainPropertyEnum(true)] public AlignmentAxes AlignmentAxis { get => _alignmentAxis; set => _alignmentAxis = value; }

        [SerializeField] private float _targetAxisValue;
        [SerializeField] private string _targetAxisValueName;
        [BrainPropertyShow(nameof(DisplayTargetAxis), (int)DisplayType.Display)]
        [BrainProperty(true)] public float TargetAxisValue { get => _targetAxisValue; set => _targetAxisValue = value; }
        [BrainPropertyValueName("TargetAxisValue", typeof(IMonaVariablesFloatValue))] public string TargetAxisValueName { get => _targetAxisValueName; set => _targetAxisValueName = value; }

        [SerializeField] private Vector2 _targetPlane;
        [SerializeField] private string[] _targetPlaneName;
        [BrainPropertyShow(nameof(DisplayTargetPlane), (int)DisplayType.Display)]
        [BrainProperty(true)] public Vector2 TargetPlane { get => _targetPlane; set => _targetPlane = value; }
        [BrainPropertyValueName("TargetPlane", typeof(IMonaVariablesVector2Value))] public string[] TargetPlaneName { get => _targetPlaneName; set => _targetPlaneName = value; }

        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private string[] _targetPositionName;
        [BrainPropertyShow(nameof(DisplayTargetPosition), (int)DisplayType.Display)]
        [BrainProperty(true)] public Vector3 TargetPosition { get => _targetPosition; set => _targetPosition = value; }
        [BrainPropertyValueName("TargetPosition", typeof(IMonaVariablesVector3Value))] public string[] TargetPositionName { get => _targetPositionName; set => _targetPositionName = value; }

        [SerializeField] private ForceScaling _scaleWith = ForceScaling.TotalMass;
        [BrainPropertyEnum(false)] public ForceScaling ScaleWith { get => _scaleWith; set => _scaleWith = value; }

        [SerializeField] private CoordinateType _forceSpace = CoordinateType.Global;
        [BrainPropertyShow(nameof(DisplayForceSpace), (int)DisplayType.Display)]
        [BrainPropertyEnum(false)] public CoordinateType ForceSpace { get => _forceSpace; set => _forceSpace = value; }

        [SerializeField] private float _maxSpeed = .2f;
        [SerializeField] private string _maxSpeedName;
        [BrainPropertyShow(nameof(DisplayMaxSpeed), (int)DisplayType.Display)]
        [BrainProperty(false)] public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }
        [BrainPropertyValueName("MaxSpeed", typeof(IMonaVariablesFloatValue))] public string MaxSpeedName { get => _maxSpeedName; set => _maxSpeedName = value; }

        private Vector3 _direction;

        protected IMonaBrain _brain;

        private MovingStateType _movingState;
        private float _time;

        private MonaInput _bodyInput;
        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool _listenToInput;
        private IInstruction _instruction;

        public TargetAlignmentGeometry TrueGeometryAlignment => AlignmentMode != PositionalAlignmentMode.Direction ? TargetAlignmentGeometry.Any : _alignmentGeometry;
        public DisplayType DisplayDistance => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode != PositionalAlignmentMode.TargetPosition ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayMaxSpeed => DirectionType != PushDirectionType.PositionalAlignment ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayForceSpace => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.TargetPosition ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayAlignmentAxis => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.TargetPosition ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayAlignmentDirection => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.Direction ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayGeometryAlignment => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.Direction ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayGeometryTag => DirectionType == PushDirectionType.PositionalAlignment && TrueGeometryAlignment == TargetAlignmentGeometry.Tag ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayDirectionTag => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.Direction && AlignmentDirection == BodyAlignmentDirection.TagDirection ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayDirectionVector => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.Direction && (AlignmentDirection == BodyAlignmentDirection.LocalDirection || AlignmentDirection == BodyAlignmentDirection.GlobalDirection) ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayTargetPosition => DirectionType == PushDirectionType.PositionalAlignment && AlignmentMode == PositionalAlignmentMode.TargetPosition && AlignmentAxis == AlignmentAxes.XYZ ? DisplayType.Display : DisplayType.Hide;

        public DisplayType DisplayTargetAxis
        {
            get
            {
                if (DirectionType != PushDirectionType.PositionalAlignment || AlignmentMode != PositionalAlignmentMode.TargetPosition)
                    return DisplayType.Hide;

                switch (_alignmentAxis)
                {
                    case AlignmentAxes.X:
                    case AlignmentAxes.Y:
                    case AlignmentAxes.Z:
                        return DisplayType.Display;

                }

                return DisplayType.Hide;
            }
        }

        public DisplayType DisplayTargetPlane
        {
            get
            {
                if (DirectionType != PushDirectionType.PositionalAlignment || AlignmentMode != PositionalAlignmentMode.TargetPosition)
                    return DisplayType.Hide;

                switch (_alignmentAxis)
                {
                    case AlignmentAxes.XY:
                    case AlignmentAxes.XZ:
                    case AlignmentAxes.YZ:
                        return DisplayType.Display;

                }

                return DisplayType.Hide;
            }
        }

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        public Vector2 InputMoveDirection
        {
            get => _bodyInput.MoveValue;
        }

        protected Vector3 TrueDefinedPosition
        {
            get
            {
                switch (_alignmentAxis)
                {
                    case AlignmentAxes.X:
                        return new Vector3(_targetAxisValue, 0f, 0f);
                    case AlignmentAxes.Y:
                        return new Vector3(0f, _targetAxisValue, 0f);
                    case AlignmentAxes.Z:
                        return new Vector3(0f, 0f, _targetAxisValue);
                    case AlignmentAxes.XY:
                        return new Vector3(_targetPlane.x, _targetPlane.y, 0f);
                    case AlignmentAxes.XZ:
                        return new Vector3(_targetPlane.x, 0f, _targetPlane.y);
                    case AlignmentAxes.YZ:
                        return new Vector3(0f, _targetPlane.x, _targetPlane.y);
                    case AlignmentAxes.XYZ:
                        return _targetPosition;
                }

                return Vector3.zero;
            }
        }

        public enum PositionalAlignmentMode
        {
            Hover = 0,
            TargetPosition = 10,
            Direction = 20
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
            _bodiesToControl.Clear();
            _bodiesToControl.Add(target);
            return target;
        }

        protected virtual bool ApplyForceToTarget()
        {
            return false;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            UpdateInput();

            var target = GetTarget();

            _direction = GetDirectionVector(DirectionType, target);
            //Debug.Log($"{nameof(ApplyForceLocalInstructionTile)}.Do {DirectionType}");

            if (!string.IsNullOrEmpty(_forceValueName))
                _force = _brain.Variables.GetFloat(_forceValueName);

            if (!string.IsNullOrEmpty(_distanceName))
                _distance = _brain.Variables.GetFloat(_distanceName);

            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.Variables.GetFloat(_durationValueName);

            if (!string.IsNullOrEmpty(_dampingName))
                _damping = _brain.Variables.GetFloat(_dampingName);

            if (!string.IsNullOrEmpty(_targetAxisValueName))
                _targetAxisValue = _brain.Variables.GetFloat(_targetAxisValueName);

            if (HasVector2Values(_targetPlaneName))
                _targetPlane = GetVector2Value(_brain, _targetPlaneName);

            if (HasVector3Values(_targetPositionName))
                _targetPosition = GetVector3Value(_brain, _targetPositionName);

            if (HasVector3Values(_directionVectorName))
                _directionVector = GetVector3Value(_brain, _directionVectorName);

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
                if (ApplyForceToTarget())
                    body = target;

                body.SetKinematic(false, true);

                Pushed();
                //if (_brain.LoggingEnabled)
                //Debug.Log($"ApplyForce to Body {body.ActiveTransform.name} {InputMoveDirection} {_direction} {_direction.normalized * _force}", body.ActiveTransform.gameObject);


                if (DirectionType == PushDirectionType.PositionalAlignment)
                {
                    body.ApplyForce(GetAlignmentForce(body), ForceMode.Acceleration, true);
                }
                else
                {
                    float force = _force * GetMassScaler(body);
                    body.ApplyForce(_direction.normalized * force, ForceMode.Impulse, true);
                    if (!body.ActiveRigidbody.isKinematic)
                        body.ActiveRigidbody.velocity = Vector3.ClampMagnitude(body.ActiveRigidbody.velocity, _maxSpeed);
                }
                
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

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public virtual List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }

        private void PushOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving && _duration > 0f)
            {
                IMonaBody body = _brain.Body;
                if (ApplyForceToTarget())
                    body = GetTarget();

                body.SetKinematic(false, true);

                //if (_brain.LoggingEnabled)
                //    Debug.Log($"ApplyForce to Body over time {_duration} {body.ActiveTransform.name} {_direction.normalized * _force * deltaTime}", body.ActiveTransform.gameObject);

                if (DirectionType == PushDirectionType.PositionalAlignment)
                {
                    body.ApplyForce(GetAlignmentForce(body), ForceMode.Acceleration, true);
                }
                else
                {
                    float force = _force * GetMassScaler(body);
                    body.ApplyForce(_direction.normalized * force, ForceMode.Impulse, true);
                    if (!body.ActiveRigidbody.isKinematic)
                        body.ActiveRigidbody.velocity = Vector3.ClampMagnitude(body.ActiveRigidbody.velocity, _maxSpeed);
                }

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

                default: return Vector3.zero;
            }
        }

        protected Vector3 GetAlignmentForce(IMonaBody body)
        {
            switch (AlignmentMode)
            {
                case PositionalAlignmentMode.Hover:
                    return AlignmentVectorForce(body, Vector3.down);
                case PositionalAlignmentMode.TargetPosition:
                    return AlignmentPositionalForce(body);
                case PositionalAlignmentMode.Direction:
                    return AlignmentVectorForce(body, GetAlignmentDirectionVector(body));
            }

            return Vector3.zero;
        }

        protected Vector3 GetAlignmentDirectionVector(IMonaBody body)
        {
            switch (_alignmentDirection)
            {
                case BodyAlignmentDirection.TagDirection:
                    if (string.IsNullOrEmpty(_directionTag))
                        break;
                    IMonaBody tagBody = body.GetClosestTag(_directionTag);
                    Vector3 direction = tagBody.GetPosition() - body.GetPosition();
                    return direction;
                case BodyAlignmentDirection.LocalDirection:
                    return body.Transform.TransformDirection(_directionVector);
                case BodyAlignmentDirection.GlobalDirection:
                    return _directionVector;
            }

            return -Vector3.up;
        }

        protected Vector3 AlignmentVectorForce(IMonaBody body, Vector3 direction)
        {
            Rigidbody rb = body.ActiveRigidbody;
            Vector3 position = body.GetPosition();

            Ray ray = new Ray(position, direction);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit))
            {
                if (TrueGeometryAlignment == TargetAlignmentGeometry.Tag)
                {
                    IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                    if (hitBody == null || !hitBody.HasMonaTag(_geometryTag))
                        return Vector3.zero;
                }

                float distanceToSurface = hit.distance;
                float distanceError = _distance - distanceToSurface;
                Vector3 oppositeDirection = direction * -1f;
                float speedInDirection = Vector3.Dot(rb.velocity, oppositeDirection.normalized);

                float adjustedForce = _force;
                float adjustedDamping = _damping;

                if (_scaleWith != ForceScaling.None)
                {
                    float massScaler = GetMassScaler(body);
                    adjustedForce *= massScaler;
                    adjustedDamping *= massScaler;
                }

                float forceAmount = (distanceError * adjustedForce) - (speedInDirection * adjustedDamping);

                return oppositeDirection.normalized * forceAmount;
            }

            return Vector3.zero;
        }

        protected Vector3 AlignmentPositionalForce(IMonaBody body)
        {
            Rigidbody rb = body.ActiveRigidbody;
            Vector3 positionError = _forceSpace == CoordinateType.Local ?
                TrueDefinedPosition - body.Transform.localPosition :
                TrueDefinedPosition - body.GetPosition();

            Vector3 forceDirection = Vector3.zero;

            switch (_alignmentAxis)
            {
                case AlignmentAxes.X:
                    forceDirection = new Vector3(positionError.x, 0, 0);
                    break;
                case AlignmentAxes.Y:
                    forceDirection = new Vector3(0, positionError.y, 0);
                    break;
                case AlignmentAxes.Z:
                    forceDirection = new Vector3(0, 0, positionError.z);
                    break;
                case AlignmentAxes.XY:
                    forceDirection = new Vector3(positionError.x, positionError.y, 0);
                    break;
                case AlignmentAxes.XZ:
                    forceDirection = new Vector3(positionError.x, 0, positionError.z);
                    break;
                case AlignmentAxes.YZ:
                    forceDirection = new Vector3(0, positionError.y, positionError.z);
                    break;
                case AlignmentAxes.XYZ:
                    forceDirection = positionError;
                    break;
            }

            float adjustedForce = _force;
            float adjustedDamping = _damping;

            if (_scaleWith != ForceScaling.None)
            {
                float massScaler = GetMassScaler(body);
                adjustedForce *= massScaler;
                adjustedDamping *= massScaler;
            }

            Vector3 dampingForce = -rb.velocity * adjustedDamping;
            Vector3 alignmentForce = forceDirection * adjustedForce;

            return alignmentForce + dampingForce;
        }

        protected float GetMassScaler(IMonaBody body)
        {
            switch (_scaleWith)
            {
                case ForceScaling.BodyMass:
                    if (body.ActiveRigidbody == null)
                        break;
                    return body.ActiveRigidbody.mass;

                case ForceScaling.TotalMass:
                    IMonaBody topBody = body;

                    while (topBody.Parent != null)
                        topBody = topBody.Parent;

                    var childRigidBodies = topBody.Transform.GetComponentsInChildren<Rigidbody>();

                    if (childRigidBodies.Length < 1)
                        break;

                    float mass = 0f;

                    for (int i = 0; i < childRigidBodies.Length; i++)
                        mass += childRigidBodies[i].mass;

                    if (mass <= 0)
                        break;

                    return mass;
            }

            return 1f;
        }

    }
}