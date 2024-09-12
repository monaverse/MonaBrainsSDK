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
    public class ApplyTorqueLocalInstructionTile : InstructionTile, IActionInstructionTile, IPauseableInstructionTile, INeedAuthorityInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IActivateInstructionTile, IRigidbodyInstructionTile
    {
        public override Type TileType => typeof(ApplyTorqueLocalInstructionTile);

        public virtual PushDirectionType DirectionType => PushDirectionType.Forward;
        public virtual TorqueAlignmentMode AlignmentMode => TorqueAlignmentMode.GroundAngle;

        [SerializeField] private AlignmentAxes _alignmentAxis = AlignmentAxes.XZ;
        [BrainPropertyShow(nameof(DirectionType), (int)PushDirectionType.TorqueAlignment)]
        [BrainPropertyEnum(true)] public AlignmentAxes AlignmentAxis { get => _alignmentAxis; set => _alignmentAxis = value; }

        [SerializeField] private float _torque = 1f;
        [SerializeField] private string _torqueValueName = null;
        [BrainProperty(true)] public float Torque { get => _torque; set => _torque = value; }
        [BrainPropertyValueName(nameof(Torque), typeof(IMonaVariablesFloatValue))] public string TorqueValueName { get => _torqueValueName; set => _torqueValueName = value; }

        [SerializeField] private float _duration = 0f;
        [SerializeField] private string _durationValueName = null;
        [BrainPropertyEnum(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName(nameof(Duration), typeof(IMonaVariablesFloatValue))] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private TargetAlignmentGeometry _alignmentGeometry = TargetAlignmentGeometry.Any;
        [BrainPropertyShow(nameof(DisplayGeometryAlignment), (int)DisplayType.Display)]
        [BrainPropertyEnum(true)] public TargetAlignmentGeometry AlignmentGeometry { get => _alignmentGeometry; set => _alignmentGeometry = value; }

        [SerializeField] private string _geometryTag = "Ground";
        [BrainPropertyShow(nameof(DisplayGeometryTag), (int)DisplayType.Display)]
        [BrainPropertyMonaTag(true)] public string GeometryTag { get => _geometryTag; set => _geometryTag = value; }

        [SerializeField] private float _targetAngle;
        [SerializeField] private string _targetAngleName;
        [BrainPropertyShow(nameof(DisplayTargetAngleOne), (int)DisplayType.Display)]
        [BrainProperty(true)] public float TargetAngle { get => _targetAngle; set => _targetAngle = value; }
        [BrainPropertyValueName("TargetAngle", typeof(IMonaVariablesFloatValue))] public string TargetAngleName { get => _targetAngleName; set => _targetAngleName = value; }

        [SerializeField] private Vector2 _targetAngles2;
        [SerializeField] private string[] _targetAngles2Name;
        [BrainPropertyShow(nameof(DisplayTargetAngleTwo), (int)DisplayType.Display)]
        [BrainProperty(true)] public Vector2 TargetAngles2 { get => _targetAngles2; set => _targetAngles2 = value; }
        [BrainPropertyValueName("TargetAngles2", typeof(IMonaVariablesVector2Value))] public string[] TargetAngles2Name { get => _targetAngles2Name; set => _targetAngles2Name = value; }

        [SerializeField] private Vector3 _targetAngles3;
        [SerializeField] private string[] _targetAngles3Name;
        [BrainPropertyShow(nameof(DisplayTargetAngleThree), (int)DisplayType.Display)]
        [BrainProperty(true)] public Vector3 TargetAngles3 { get => _targetAngles3; set => _targetAngles3 = value; }
        [BrainPropertyValueName("TargetAngles3", typeof(IMonaVariablesVector3Value))] public string[] TargetAngles3Name { get => _targetAngles3Name; set => _targetAngles3Name = value; }

        [SerializeField] private float _damping = 50;
        [SerializeField] private string _dampingName;
        [BrainPropertyShow(nameof(DirectionType), (int)PushDirectionType.TorqueAlignment)]
        [BrainProperty(false)] public float Damping { get => _damping; set => _damping = value; }
        [BrainPropertyValueName("Damping", typeof(IMonaVariablesFloatValue))] public string DampingName { get => _dampingName; set => _dampingName = value; }

        [SerializeField] private BodyAlignmentDirection _alignmentDirection = BodyAlignmentDirection.TagDirection;
        [BrainPropertyShow(nameof(DisplayAlignmentDirection), (int)DisplayType.Display)]
        [BrainPropertyEnum(false)] public BodyAlignmentDirection AlignmentDirection { get => _alignmentDirection; set => _alignmentDirection = value; }

        [SerializeField] private string _directionTag = "Player";
        [BrainPropertyShow(nameof(DisplayDirectionTag), (int)DisplayType.Display)]
        [BrainPropertyMonaTag(false)] public string DirectionTag { get => _directionTag; set => _directionTag = value; }

        [SerializeField] private Vector3 _directionVector = Vector3.down;
        [SerializeField] private string[] _directionVectorName;
        [BrainPropertyShow(nameof(DisplayDirectionVector), (int)DisplayType.Display)]
        [BrainProperty(false)] public Vector3 DirectionVector { get => _directionVector; set => _directionVector = value; }
        [BrainPropertyValueName("DirectionVector", typeof(IMonaVariablesVector3Value))] public string[] DirectionVectorName { get => _directionVectorName; set => _directionVectorName = value; }

        [SerializeField] private ForceScaling _scaleForceWith = ForceScaling.TotalMass;
        [BrainPropertyEnum(false)] public ForceScaling ScaleForceWith { get => _scaleForceWith; set => _scaleForceWith = value; }

        [SerializeField] private float _maxSpeed = .2f;
        [SerializeField] private string _maxSpeedName;
        [BrainPropertyShow(nameof(DisplayMaxSpeed), (int)DisplayType.Display)]
        [BrainProperty(false)] public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }
        [BrainPropertyValueName(nameof(MaxSpeed), typeof(IMonaVariablesFloatValue))] public string MaxSpeedName { get => _maxSpeedName; set => _maxSpeedName = value; }

        [SerializeField] private float _alignVelocity = 0.5f;
        [SerializeField] private string _alignVelocityName;
        [BrainProperty(false)] public float AlignVelocity { get => _alignVelocity; set => _alignVelocity = value; }
        [BrainPropertyValueName(nameof(AlignVelocity), typeof(IMonaVariablesFloatValue))] public string AlignVelocityName { get => _alignVelocityName; set => _alignVelocityName = value; }

        [SerializeField] private DistanceSampling _sampling = DistanceSampling.SingleSample;
        [BrainPropertyShow(nameof(DisplayRaySampling), (int)DisplayType.Display)]
        [BrainPropertyEnum(false)] public DistanceSampling Sampling { get => _sampling; set => _sampling = value; }

        [SerializeField] private float _samplingRadius = 1f;
        [SerializeField] private string _samplingRadiusName;
        [BrainPropertyShow(nameof(DisplaySamplingVars), (int)DisplayType.Display)]
        [BrainProperty(false)] public float SamplingRadius { get => _samplingRadius; set => _samplingRadius = value; }
        [BrainPropertyValueName("SamplingRadius", typeof(IMonaVariablesFloatValue))] public string SamplingRadiusName { get => _samplingRadiusName; set => _samplingRadiusName = value; }

        [SerializeField] private float _sampleCount = 10f;
        [SerializeField] private string _sampleCountName;
        [BrainPropertyShow(nameof(DisplaySamplingVars), (int)DisplayType.Display)]
        [BrainProperty(false)] public float SampleCount { get => _sampleCount; set => _sampleCount = value; }
        [BrainPropertyValueName("SampleCount", typeof(IMonaVariablesFloatValue))] public string SampleCountName { get => _sampleCountName; set => _sampleCountName = value; }

        [SerializeField] private SamplingType _sampleToUse = SamplingType.Closest;
        [BrainPropertyShow(nameof(DisplaySamplingVars), (int)DisplayType.Display)]
        [BrainPropertyEnum(false)] public SamplingType SampleToUse { get => _sampleToUse; set => _sampleToUse = value; }

        [SerializeField] private AlignmentDistanceLimitType _alignmentLimit = AlignmentDistanceLimitType.Infinity;
        [BrainPropertyShow(nameof(DisplayAlignmentDirection), (int)DisplayType.Display)]
        [BrainPropertyEnum(false)] public AlignmentDistanceLimitType AlignmentLimit { get => _alignmentLimit; set => _alignmentLimit = value; }

        [SerializeField] private float _distanceLimit = 100f;
        [SerializeField] private string _distanceLimitName;
        [BrainPropertyShow(nameof(DisplayDistanceLimit), (int)DisplayType.Display)]
        [BrainProperty(false)] public float DistanceLimit { get => _distanceLimit; set => _distanceLimit = value; }
        [BrainPropertyValueName("DistanceLimit", typeof(IMonaVariablesFloatValue))] public string DistanceLimitName { get => _distanceLimitName; set => _distanceLimitName = value; }

        [SerializeField] private ApplyForceType _forceType = ApplyForceType.Impulse;
        [BrainPropertyEnum(false)] public ApplyForceType ForceType { get => _forceType; set => _forceType = value; }

        private Vector3 _direction;

        protected IMonaBrain _brain;
        private List<Collider> _colliders;

        private MovingStateType _movingState;
        private float _time;

        private MonaInput _bodyInput;
        private bool _active;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        protected Dictionary<int, RaycastResult> _raycastResults = new Dictionary<int, RaycastResult>();

        private bool _listenToInput;

        public float TrueDistanceLimit => _alignmentLimit == AlignmentDistanceLimitType.MaxDistance ? _distanceLimit : Mathf.Infinity;
        public ForceMode ForceModeToUse => _forceType == ApplyForceType.Impulse ? ForceMode.Impulse : ForceMode.Acceleration;
        public TargetAlignmentGeometry TrueGeometryAlignment => AlignmentMode != TorqueAlignmentMode.GeometryInDirection ? TargetAlignmentGeometry.Any : _alignmentGeometry;

        public DisplayType DisplayAlignmentDirection => DirectionType == PushDirectionType.TorqueAlignment && AlignmentMode == TorqueAlignmentMode.GeometryInDirection ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayGeometryAlignment => DirectionType == PushDirectionType.TorqueAlignment && AlignmentMode == TorqueAlignmentMode.GeometryInDirection ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayGeometryTag => DirectionType == PushDirectionType.TorqueAlignment && TrueGeometryAlignment == TargetAlignmentGeometry.Tag ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayDirectionTag => DirectionType == PushDirectionType.TorqueAlignment && AlignmentMode == TorqueAlignmentMode.GeometryInDirection && AlignmentDirection == BodyAlignmentDirection.TagDirection ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayDirectionVector => DirectionType == PushDirectionType.TorqueAlignment && AlignmentMode == TorqueAlignmentMode.GeometryInDirection && (AlignmentDirection == BodyAlignmentDirection.LocalDirection || AlignmentDirection == BodyAlignmentDirection.GlobalDirection) ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayTargetAngleThree => DirectionType == PushDirectionType.TorqueAlignment && AlignmentMode == TorqueAlignmentMode.TargetAngles && AlignmentAxis == AlignmentAxes.XYZ ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayRaySampling => DirectionType == PushDirectionType.TorqueAlignment && AlignmentMode != TorqueAlignmentMode.TargetAngles ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplaySamplingVars => DisplayRaySampling == DisplayType.Display && Sampling == DistanceSampling.MultipleSamples ? DisplayType.Display : DisplayType.Hide;
        public DisplayType DisplayDistanceLimit => DisplayAlignmentDirection == DisplayType.Display && AlignmentLimit == AlignmentDistanceLimitType.MaxDistance ? DisplayType.Display : DisplayType.Hide;

        // ** SUPPORT FOR OLD TILES **
        public DisplayType DisplayMaxSpeed => DirectionType != PushDirectionType.TorqueAlignment && (MaxSpeed > 0 || !string.IsNullOrEmpty(MaxSpeedName)) ? DisplayType.Display : DisplayType.Hide;
        // ** END **

        public DisplayType DisplayTargetAngleOne
        {
            get
            {
                if (DirectionType != PushDirectionType.TorqueAlignment || AlignmentMode != TorqueAlignmentMode.TargetAngles)
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

        public DisplayType DisplayTargetAngleTwo
        {
            get
            {
                if (DirectionType != PushDirectionType.TorqueAlignment || AlignmentMode != TorqueAlignmentMode.TargetAngles)
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

        protected Vector3 TrueDefinedTargetAngles
        {
            get
            {
                switch (_alignmentAxis)
                {
                    case AlignmentAxes.X:
                        return new Vector3(_targetAngle, 0f, 0f);
                    case AlignmentAxes.Y:
                        return new Vector3(0f, _targetAngle, 0f);
                    case AlignmentAxes.Z:
                        return new Vector3(0f, 0f, _targetAngle);
                    case AlignmentAxes.XY:
                        return new Vector3(_targetAngles2.x, _targetAngles2.y, 0f);
                    case AlignmentAxes.XZ:
                        return new Vector3(_targetAngles2.x, 0f, _targetAngles2.y);
                    case AlignmentAxes.YZ:
                        return new Vector3(0f, _targetAngles2.x, _targetAngles2.y);
                    case AlignmentAxes.XYZ:
                        return _targetAngles3;
                }

                return Vector3.zero;
            }
        }

        public enum TorqueAlignmentMode
        {
            GroundAngle = 0,
            TargetAngles = 10,
            GeometryInDirection = 20
        }

        public enum DistanceSampling
        {
            SingleSample = 0,
            MultipleSamples = 10
        }

        public enum SamplingType
        {
            Closest = 0,
            Furthest = 10,
            Average = 20
        }

        public enum AlignmentDistanceLimitType
        {
            Infinity = 0,
            MaxDistance = 10
        }

        public struct RaycastResult
        {
            public float distance;
            public Vector3 normal;
        }

        public ApplyTorqueLocalInstructionTile() { }
        
        public virtual void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            if (!_brain.Body.HasCollider())
                _colliders = _brain.Body.AddCollider();

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
        
            if (destroy)
            {
                if (_colliders != null)
                {
                    for (var i = 0; i < _colliders.Count; i++)
                        GameObject.Destroy(_colliders[i]);
                }
                _colliders = null;
            }
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
            _bodiesToControl.Clear();
            _bodiesToControl.Add(target);
            return target;
        }

        protected virtual bool ApplyTorqueToTarget()
        {
            return false;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            UpdateInput();

            var target = GetTarget();

            _direction = GetDirectionVector(DirectionType, target);
            //Debug.Log($"{nameof(ApplyTorqueLocalInstructionTile)}.Do {DirectionType}");

            if (!string.IsNullOrEmpty(_torqueValueName))
                _torque = _brain.Variables.GetFloat(_torqueValueName);

            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.Variables.GetFloat(_durationValueName);

            if (!string.IsNullOrEmpty(_dampingName))
                _damping = _brain.Variables.GetFloat(_dampingName);

            if (!string.IsNullOrEmpty(_targetAngleName))
                _targetAngle = _brain.Variables.GetFloat(_targetAngleName);

            if (HasVector2Values(_targetAngles2Name))
                _targetAngles2 = GetVector2Value(_brain, _targetAngles2Name);

            if (HasVector3Values(_targetAngles3Name))
                _targetAngles3 = GetVector3Value(_brain, _targetAngles3Name);

            if (HasVector3Values(_directionVectorName))
                _directionVector = GetVector3Value(_brain, _directionVectorName);

            if (!string.IsNullOrEmpty(_maxSpeedName))
                _maxSpeed = _brain.Variables.GetFloat(_maxSpeedName);

            if (!string.IsNullOrEmpty(_alignVelocityName))
                _alignVelocity = _brain.Variables.GetFloat(_alignVelocityName);

            if (!string.IsNullOrEmpty(_sampleCountName))
                _sampleCount = _brain.Variables.GetFloat(_sampleCountName);

            if (!string.IsNullOrEmpty(_samplingRadiusName))
                _samplingRadius = _brain.Variables.GetFloat(_samplingRadiusName);

            if (!string.IsNullOrEmpty(_distanceLimitName))
                _distanceLimit = _brain.Variables.GetFloat(_distanceLimitName);

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

                body.SetKinematic(false, true);

                Pushed();
                //if (_brain.LoggingEnabled)
                //Debug.Log($"ApplyTorque to Body {body.ActiveTransform.name} {InputMoveDirection} {_direction} {_direction.normalized * _force}", body.ActiveTransform.gameObject);

                if (DirectionType == PushDirectionType.TorqueAlignment)
                {
                    body.ApplyTorque(GetAlignmentForce(body, evt.DeltaTime), ForceModeToUse, true);
                }
                else
                {
                    float torque = _torque * GetMassScaler(body);
                    body.ApplyTorque(_direction.normalized * torque, ForceModeToUse, true);

                    // ** SUPPORT FOR OLD TILES **
                    if (_maxSpeed > 0f)
                    {
                        body.ActiveRigidbody.maxAngularVelocity = _maxSpeed;
                        if (!body.ActiveRigidbody.isKinematic)
                            body.ActiveRigidbody.angularVelocity = Vector3.ClampMagnitude(body.ActiveRigidbody.angularVelocity, _maxSpeed);
                    }
                    // ** END **
                }

                AlignVelocityWithTorque(body);
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
                if (ApplyTorqueToTarget())
                    body = GetTarget();
                
                body.SetKinematic(false, true);

                if (_brain.LoggingEnabled)
                    Debug.Log($"ApplyTorque to Body over time {_duration} {body.ActiveTransform.name} {_direction.normalized * _torque} {body.ActiveRigidbody.angularVelocity.magnitude}", body.ActiveTransform.gameObject);


                if (DirectionType == PushDirectionType.TorqueAlignment)
                {
                    body.ApplyTorque(GetAlignmentForce(body, deltaTime), ForceModeToUse, true);
                }
                else
                {
                    float torque = _torque * GetMassScaler(body);
                    body.ApplyTorque(_direction.normalized * torque, ForceModeToUse, true);

                    // ** SUPPORT FOR OLD TILES **
                    if (_maxSpeed > 0f)
                    {
                        body.ActiveRigidbody.maxAngularVelocity = _maxSpeed;
                        if (!body.ActiveRigidbody.isKinematic)
                            body.ActiveRigidbody.angularVelocity = Vector3.ClampMagnitude(body.ActiveRigidbody.angularVelocity, _maxSpeed);
                    }
                    // ** END **
                }

                AlignVelocityWithTorque(body);

                if (_time >= 1f)
                {
                    StopPushing();
                }

                _time += deltaTime;
            }
        }

        private void AlignVelocityWithTorque(IMonaBody body)
        {
            if (Mathf.Approximately(_alignVelocity, 0f))
                return;

            Vector3 currentVelocity = body.CurrentVelocity;
            //Vector3 alignedVelocity = Vector3.Lerp(currentVelocity, body.Transform.forward * currentVelocity.magnitude, _alignVelocity);
            //body.ActiveRigidbody.velocity = alignedVelocity;

            Vector3 localVelocity = body.Transform.InverseTransformDirection(currentVelocity);

            // Determine the primary movement direction in local space
            Vector3 primaryDirection = localVelocity.normalized;
            primaryDirection.x = 0; // Ignore sideways movement
            primaryDirection.y = 0; // Ignore vertical movement
            primaryDirection = primaryDirection.normalized;

            // Transform the primary direction back to world space
            Vector3 worldAlignDirection = body.Transform.TransformDirection(primaryDirection);

            // Align velocity with rotation
            Vector3 alignedVelocity = Vector3.Lerp(currentVelocity, worldAlignDirection * currentVelocity.magnitude, _alignVelocity);
            body.ActiveRigidbody.velocity = alignedVelocity;
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

        protected Vector3 GetAlignmentForce(IMonaBody body, float deltaTime)
        {
            Quaternion currentRotation = body.GetRotation();
            Quaternion targetRotation;

            switch (AlignmentMode)
            {
                case TorqueAlignmentMode.TargetAngles:
                    targetRotation = Quaternion.Euler(TrueDefinedTargetAngles);
                    break;
                default:
                    return GetDirectionVectorRotation(body, body.GetPosition(), GetAlignmentDirectionVector(body), deltaTime);
            }

            return TorqueForRotation(body, currentRotation, targetRotation, deltaTime);
        }

        protected Vector3 GetDirectionVectorRotation(IMonaBody body, Vector3 rayOrigin, Vector3 direction, float deltaTime)
        {
            Vector3 targetUp = _sampling == DistanceSampling.SingleSample ?
                GetNormalFromRay(rayOrigin, direction) :
                GetNormalFromPlane(rayOrigin, direction);

            Vector3 currentUp = body.Transform.up;
            Vector3 torque = Vector3.Cross(currentUp, targetUp) * _torque;

            switch (_alignmentAxis)
            {
                case AlignmentAxes.X:
                    torque = new Vector3(torque.x, 0, 0);
                    break;
                case AlignmentAxes.Y:
                    torque = new Vector3(0, torque.y, 0);
                    break;
                case AlignmentAxes.Z:
                    torque = new Vector3(0, 0, torque.z);
                    break;
                case AlignmentAxes.XY:
                    torque = new Vector3(torque.x, torque.y, 0);
                    break;
                case AlignmentAxes.XZ:
                    torque = new Vector3(torque.x, 0, torque.z);
                    break;
                case AlignmentAxes.YZ:
                    torque = new Vector3(0, torque.y, torque.z);
                    break;
            }

            float massScaler = GetMassScaler(body);
            float damping = _damping * massScaler;
            torque *= massScaler;

            Vector3 dampingTorque = damping * deltaTime * -body.ActiveRigidbody.angularVelocity;
            return torque + dampingTorque;
        }

        protected Vector3 GetNormalFromRay(Vector3 position, Vector3 direction)
        {
            Ray ray = new Ray(position, direction);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit, TrueDistanceLimit))
            {
                if (TrueGeometryAlignment == TargetAlignmentGeometry.Tag)
                {
                    IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                    if (hitBody == null || !hitBody.HasMonaTag(_geometryTag))
                    {
                        return Vector3.zero;
                    }
                }

                return hit.normal;
            }

            return Vector3.zero;
        }

        protected Vector3 GetNormalFromPlane(Vector3 position, Vector3 direction)
        {
            _raycastResults.Clear();

            Vector3 perp1 = Vector3.Cross(direction, Vector3.up).normalized;

            if (perp1.magnitude < 0.1f)
                perp1 = Vector3.Cross(direction, Vector3.right).normalized;

            Vector3 perp2 = Vector3.Cross(direction, perp1).normalized;

            for (int i = 0; i < _sampleCount; i++)
            {
                Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * _samplingRadius;
                Vector3 sampleOrigin = position + (perp1 * randomPoint.x) + (perp2 * randomPoint.y);

                Ray ray = new Ray(sampleOrigin, direction);
                RaycastHit hit;

                if (UnityEngine.Physics.Raycast(ray, out hit, TrueDistanceLimit))
                {
                    if (TrueGeometryAlignment == TargetAlignmentGeometry.Tag)
                    {
                        IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                        if (hitBody == null || !hitBody.HasMonaTag(_geometryTag))
                            continue;
                    }
                    _raycastResults.Add(i, new RaycastResult { distance = hit.distance, normal = hit.normal });
                }
            }

            Vector3 result = Vector3.zero;
            float closestDistance = float.MaxValue;
            float furthestDistance = float.MinValue;
            Vector3 sumOfNormals = Vector3.zero;

            int[] keys = new int[_raycastResults.Count];
            _raycastResults.Keys.CopyTo(keys, 0);

            switch (_sampleToUse)
            {
                case SamplingType.Closest:
                    for (int i = 0; i < keys.Length; i++)
                    {
                        RaycastResult currentResult = _raycastResults[keys[i]];

                        if (currentResult.distance < closestDistance)
                        {
                            closestDistance = currentResult.distance;
                            result = currentResult.normal;
                        }
                    }
                    break;
                case SamplingType.Furthest:
                    for (int i = 0; i < keys.Length; i++)
                    {
                        RaycastResult currentResult = _raycastResults[keys[i]];

                        if (currentResult.distance > furthestDistance)
                        {
                            furthestDistance = currentResult.distance;
                            result = currentResult.normal;
                        }
                    }
                    break;
                case SamplingType.Average:

                    if (_raycastResults.Count < 1)
                        break;

                    for (int i = 0; i < keys.Length; i++)
                    {
                        RaycastResult currentResult = _raycastResults[keys[i]];
                        sumOfNormals += currentResult.normal;
                    }
                    
                    result = sumOfNormals / _raycastResults.Count;
                    break;
            }

            return result;
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

            return Vector3.down;
        }

        protected Vector3 TorqueForRotation(IMonaBody body, Quaternion currentRotation, Quaternion targetRotation, float deltaTime)
        {
            Rigidbody rb = body.ActiveRigidbody;

            Vector3 targetEulers = GetAdjustedTargetAngles(currentRotation.eulerAngles, targetRotation.eulerAngles);
            Quaternion deltaRotation = Quaternion.Euler(targetEulers) * Quaternion.Inverse(currentRotation);
            //Quaternion deltaRotation = targetRotation * Quaternion.Inverse(currentRotation);
            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

            switch (_alignmentAxis)
            {
                case AlignmentAxes.X:
                    axis.y = 0;
                    axis.z = 0;
                    break;
                case AlignmentAxes.Y:
                    axis.x = 0;
                    axis.z = 0;
                    break;
                case AlignmentAxes.Z:
                    axis.x = 0;
                    axis.y = 0;
                    break;
                case AlignmentAxes.XY:
                    axis.z = 0;
                    break;
                case AlignmentAxes.XZ:
                    axis.y = 0;
                    break;
                case AlignmentAxes.YZ:
                    axis.x = 0;
                    break;
            }

            float adjustedForce = _torque;
            float adjustedDamping = _damping;

            if (_scaleForceWith != ForceScaling.None)
            {
                float massScaler = GetMassScaler(body);
                adjustedForce *= massScaler;
                adjustedDamping *= massScaler;
            }

            Vector3 torque = axis * angle * adjustedForce * deltaTime;
            Vector3 dampingTorque = adjustedDamping * deltaTime * -rb.angularVelocity;
            return torque + dampingTorque;
        }

        protected Vector3 GetAdjustedTargetAngles(Vector3 currentAngles, Vector3 targetAngles)
        {
            Vector3 modifiedTargetEulers = new Vector3(
            ShortestAngleDifference(currentAngles.x, targetAngles.x),
            ShortestAngleDifference(currentAngles.y, targetAngles.y),
            ShortestAngleDifference(currentAngles.z, targetAngles.z)
            );

            return modifiedTargetEulers;
        }

        protected float ShortestAngleDifference(float current, float target)
        {
            float difference = Mathf.DeltaAngle(current, target);
            return difference;
        }

        protected float GetMassScaler(IMonaBody body)
        {
            switch (_scaleForceWith)
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