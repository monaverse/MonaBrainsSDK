using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveWithInputInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction
    {
        public const string ID = "MoveWithInput";
        public const string NAME = "Move With Input";
        public const string CATEGORY = "Adv Movement";
        public override Type TileType => typeof(MoveWithInputInstructionTile);

        [SerializeField] private MonaBrainTransformTargetType _target = MonaBrainTransformTargetType.Self;
        [BrainPropertyEnum(true)] public MonaBrainTransformTargetType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag = "Player";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformTargetType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private PlayerInputDeviceType _device = PlayerInputDeviceType.Mouse;
        [BrainPropertyEnum(true)] public PlayerInputDeviceType Device { get => _device; set => _device = value; }

        [SerializeField] private Vector2 _directionVector;
        [SerializeField] private string[] _directionVectorName;
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.Vector2)]
        [BrainProperty(true)] public Vector2 DirectionVector { get => _directionVector; set => _directionVector = value; }
        [BrainPropertyValueName("DirectionVector", typeof(IMonaVariablesVector2Value))] public string[] DirectionVectorName { get => _directionVectorName; set => _directionVectorName = value; }

        [SerializeField] private MovementMode _moveAlong = MovementMode.XZ;
        [BrainPropertyEnum(true)] public MovementMode MoveAlong { get => _moveAlong; set => _moveAlong = value; }

        [SerializeField] private MovementPlaneType _axisType = MovementPlaneType.ViewBasedPlaneLock;
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.GenericMovement)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.GenericLook)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.DigitalPad)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.LeftAnalogStick)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.RightAnalogStick)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.Vector2)]
        [BrainPropertyEnum(false)] public MovementPlaneType AxisType { get => _axisType; set => _axisType = value; }

        [SerializeField] private float _range = 30f;
        [SerializeField] private string _rangeName;
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.Mouse)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.GenericPointer)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.TouchScreen)]
        [BrainProperty(false)] public float MaxDistance { get => _range; set => _range = value; }
        [BrainPropertyValueName("Range", typeof(IMonaVariablesFloatValue))] public string MaxDistanceName { get => _rangeName; set => _rangeName = value; }

        [SerializeField] private float _gridSnapUnits = 1f;
        [SerializeField] private string _gridSnapUnitsName;
        [BrainProperty(false)] public float GridSnapUnits { get => _gridSnapUnits; set => _gridSnapUnits = value; }
        [BrainPropertyValueName("GridSnapUnits", typeof(IMonaVariablesFloatValue))] public string GridSnapUnitsName { get => _gridSnapUnitsName; set => _gridSnapUnitsName = value; }

        [SerializeField] private float _snapInterval = 0.1f;
        [SerializeField] private string _snapIntervalName;
        [BrainPropertyShow(nameof(SnapType), (int)MovementSnapType.SnapToGrid)]
        [BrainProperty(false)] public float SnapInterval { get => _snapInterval; set => _snapInterval = value; }
        [BrainPropertyValueName("SnapInterval", typeof(IMonaVariablesFloatValue))] public string SnapIntervalName { get => _snapIntervalName; set => _snapIntervalName = value; }

        [SerializeField] private float _speed = 10f;
        [SerializeField] private string _speedName;
        [BrainPropertyShow(nameof(DisplaySpeed), (int)DisplayField.Display)]
        [BrainPropertyShowLabel(nameof(DisplaySpeed), (int)DisplayField.Display, "Meters/Sec")]
        [BrainProperty(false)] public float Speed { get => _speed; set => _speed = value; }
        [BrainPropertyValueName("Speed", typeof(IMonaVariablesFloatValue))] public string SpeedName { get => _speedName; set => _speedName = value; }

        [SerializeField] private bool _placeOnSurface = false;
        [SerializeField] private string _placeOnSurfaceName;
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.Mouse)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.GenericPointer)]
        [BrainPropertyShow(nameof(Device), (int)PlayerInputDeviceType.TouchScreen)]
        [BrainProperty(false)] public bool PlaceOnSurface { get => _placeOnSurface; set => _placeOnSurface = value; }
        [BrainPropertyValueName("PlaceOnSurface", typeof(IMonaVariablesBoolValue))] public string PlaceOnSurfaceName { get => _placeOnSurfaceName; set => _placeOnSurfaceName = value; }

        private float _previousTime = 0;
        private float _lastGamepadMoveTime = 0f;
        private float _deltaTime;
        private string _ignoreRaycastLayer = "Ignore Raycast";
        private Vector3 _pointerPosition = Vector3.zero;
        private Vector3 _closestBodyPositionToPointer = Vector3.zero;
        private LayerMask _placementLayerMask;
        private Camera _mainCamera;
        private IMonaBrain _brain;
        private IMonaBody _closestBodyToPointer = null;
        private IMonaBrainInput _brainInput;
        private List<LayerMask> _bodyLayers = new List<LayerMask>();
        private List<IMonaBody> _targetBodies = new List<IMonaBody>();

        public MoveWithInputInstructionTile() { }

        public MovementSnapType SnapType => _gridSnapUnits > 0 ? MovementSnapType.SnapToGrid : MovementSnapType.NoSnap;

        public DisplayField DisplaySpeed => SnapType == MovementSnapType.NoSnap && !UsePointer ? DisplayField.Display : DisplayField.Hide;

        private bool UsePointer
        {
            get
            {
                switch (_device)
                {
                    case PlayerInputDeviceType.GenericPointer:
                    case PlayerInputDeviceType.Mouse:
                    case PlayerInputDeviceType.TouchScreen:
                        return true;
                }
                return false;
            }
        }

        public enum MovementSnapType
        {
            NoSnap = 0,
            SnapToGrid = 1
        }

        public enum DisplayField
        {
            Hide = 0,
            Display = 1
        }

        public enum MovementMode
        {
            XY = 0,
            XZ = 1,
            YZ = 2,
            X = 3,
            Y = 4,
            Z = 5
        }

        public enum PlayerInputDeviceType
        {
            GenericPointer = 0,
            GenericMovement = 10,
            GenericLook = 20,
            Mouse = 30,
            TouchScreen = 40,
            DigitalPad = 50,
            LeftAnalogStick = 60,
            RightAnalogStick = 70,
            Vector2 = 80
        }

        public enum MovementPlaneType
        {
            Local = 0,
            World = 10,
            ViewBasedPlaneLock = 20,
            TrueCameraRelative = 30
        }

        public virtual void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;
            _brainInput = MonaGlobalBrainRunner.Instance.GetBrainInput();

            int ignoreRaycastLayer = LayerMask.NameToLayer(_ignoreRaycastLayer);
            _placementLayerMask = ~(1 << ignoreRaycastLayer);

            if (_device != PlayerInputDeviceType.Vector2)
                _brainInput.StartListening(this);
        }

        public override void Unload(bool destroyed = false)
        {
            if (_device != PlayerInputDeviceType.Vector2)
                _brainInput.StopListening(this);
        }

        private float GetAndUpdateDeltaTime()
        {
            float time = Time.time;
            float deltaTime = Time.time - _previousTime;
            _previousTime = time;

            return deltaTime < 0.1f ? deltaTime : 0;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _mainCamera = MonaGlobalBrainRunner.Instance.SceneCamera;
            _targetBodies.Clear();

            if (HasVector2Values(_directionVectorName))
                _directionVector = GetVector2Value(_brain, _directionVectorName);

            if (!string.IsNullOrEmpty(_rangeName))
                _range = _brain.Variables.GetFloat(_rangeName);

            if (!string.IsNullOrEmpty(_gridSnapUnitsName))
                _gridSnapUnits = _brain.Variables.GetFloat(_gridSnapUnitsName);

            if (!string.IsNullOrEmpty(_snapIntervalName))
                _snapInterval = _brain.Variables.GetFloat(_snapIntervalName);

            if (!string.IsNullOrEmpty(_speedName))
                _speed = _brain.Variables.GetFloat(_speedName);

            if (!string.IsNullOrEmpty(_placeOnSurfaceName))
                _placeOnSurface = _brain.Variables.GetBool(_placeOnSurfaceName);

            _deltaTime = GetAndUpdateDeltaTime();

            if (_range <= 0)
                _range = Mathf.Infinity;

            SetTargetBodies();

            if (_targetBodies.Count < 1)
                return Complete(InstructionTileResult.Success);

            Vector3 moveVector = UsePointer ? Vector3.zero : GetGamepadMovement();

            if (UsePointer)
            {
                switch(_device)
                {
                    case PlayerInputDeviceType.GenericPointer:
                        if (UnityEngine.Input.touchCount > 0)
                        {
                            Touch justTouch = UnityEngine.Input.GetTouch(0);
                            _pointerPosition = justTouch.position;
                        }
                        else
                        {
                            _pointerPosition = UnityEngine.Input.mousePosition;
                        }
                        break;
                    case PlayerInputDeviceType.Mouse:
                        _pointerPosition = UnityEngine.Input.mousePosition;
                        break;
                    case PlayerInputDeviceType.TouchScreen:
                        if (UnityEngine.Input.touchCount > 0)
                        {
                            Touch justTouch = UnityEngine.Input.GetTouch(0);
                            _pointerPosition = justTouch.position;
                        }
                        else
                        {
                            return Complete(InstructionTileResult.Success);
                        }
                        break;
                }

                SetClosestBodyToPointer();

                if (_closestBodyToPointer != null)
                    SetBodyToPointerPosition(_closestBodyToPointer);
            }

            for (int i = 0; i < _targetBodies.Count; i++)
            {
                if (UsePointer) SetPointerBodyOffsets(_targetBodies[i]);
                else ApplyGamepadInput(_targetBodies[i], moveVector);
            }

            return Complete(InstructionTileResult.Success);
        }

        private void SetPointerBodyOffsets(IMonaBody body)
        {
            if (_closestBodyToPointer == null || body == _closestBodyToPointer)
                return;

            Vector3 offset = body.GetPosition() - _closestBodyPositionToPointer;
            Vector3 newPosition = _closestBodyToPointer.GetPosition() + offset;

            body.TeleportPosition(newPosition);
        }

        private void SetBodyToPointerPosition(IMonaBody body)
        {
            if (body == null) return;

            if (_placeOnSurface && TryPlaceBodyAtPointerPosition(body, _pointerPosition))
                return;

            Vector3 pointerWorldPosition = GetPointerWorldPosition(body, _pointerPosition);
            ApplyPointerInput(body, pointerWorldPosition);
        }

        private Vector3 GetPointerWorldPosition(IMonaBody body, Vector3 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Vector3 bodyPosition = body.GetPosition();

            switch (_moveAlong)
            {
                case MovementMode.XY:
                    Plane planeXY = new Plane(Vector3.forward, new Vector3(0, 0, bodyPosition.z));
                    if (planeXY.Raycast(ray, out float distanceXY))
                        return ray.GetPoint(distanceXY);
                    break;
                case MovementMode.XZ:
                    Plane planeXZ = new Plane(Vector3.up, new Vector3(0, bodyPosition.y, 0));
                    if (planeXZ.Raycast(ray, out float distanceXZ))
                        return ray.GetPoint(distanceXZ);
                    break;
                case MovementMode.YZ:
                    Plane planeYZ = new Plane(Vector3.right, new Vector3(bodyPosition.x, 0, 0));
                    if (planeYZ.Raycast(ray, out float distanceYZ))
                        return ray.GetPoint(distanceYZ);
                    break;
                case MovementMode.X:
                    Plane planeX = new Plane(Vector3.up, new Vector3(0, 0, bodyPosition.z));
                    if (planeX.Raycast(ray, out float distanceX))
                        return ray.GetPoint(distanceX);
                    break;
                case MovementMode.Y:
                    Plane planeY = new Plane(Vector3.forward, new Vector3(bodyPosition.x, 0, 0));
                    if (planeY.Raycast(ray, out float distanceY))
                        return ray.GetPoint(distanceY);
                    break;
                case MovementMode.Z:
                    Plane planeZ = new Plane(Vector3.up, new Vector3(0, bodyPosition.y, 0));
                    if (planeZ.Raycast(ray, out float distanceZ))
                        return ray.GetPoint(distanceZ);
                    break;
            }

            return Vector3.zero;
        }

        private void ApplyPointerInput(IMonaBody body, Vector3 pointerWorldPosition)
        {
            
            Vector3 pointerPosition = SnapToGrid(pointerWorldPosition);
            Vector3 newPosition = body.GetPosition();
            switch (_moveAlong)
            {
                case MovementMode.XY:
                    newPosition.x = pointerPosition.x;
                    newPosition.y = pointerPosition.y;
                    break;
                case MovementMode.XZ:
                    newPosition.x = pointerPosition.x;
                    newPosition.z = pointerPosition.z;
                    break;
                case MovementMode.YZ:
                    newPosition.y = pointerPosition.y;
                    newPosition.z = pointerPosition.z;
                    break;
                case MovementMode.X:
                    newPosition.x = pointerPosition.x;
                    break;
                case MovementMode.Y:
                    newPosition.y = pointerPosition.y;
                    break;
                case MovementMode.Z:
                    newPosition.z = pointerPosition.z;
                    break;
            }

            newPosition = SnapToGrid(newPosition);
            newPosition = ClampPositionToMaxDistance(newPosition);
            body.TeleportPosition(newPosition);
        }

        private bool TryPlaceBodyAtPointerPosition(IMonaBody body, Vector3 pointerPosition)
        {
            _bodyLayers.Clear();
            SetOriginalBodyLayers(body);
            SetBodyLayer(body, LayerMask.NameToLayer(_ignoreRaycastLayer));

            Ray ray = _mainCamera.ScreenPointToRay(pointerPosition);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit, _range, _placementLayerMask))
            {
                Vector3 hitPoint = hit.point;
                Vector3 hitNormal = hit.normal;

                Bounds bounds = GetBodyBounds(body);
                float offsetDistance = CalculateOffsetDistance(bounds, hitNormal);
                Vector3 newPosition = SnapToGrid(hitPoint + hitNormal * offsetDistance);

                if (SnapType == MovementSnapType.SnapToGrid && UnityEngine.Physics.CheckBox(newPosition, bounds.extents, body.GetRotation()))
                {
                    Vector3 direction = hitNormal.normalized;
                    newPosition = SnapToGrid(hitPoint + direction * _gridSnapUnits);
                }

                body.TeleportPosition(newPosition);
            }
            else
            {
                ResetOriginalBodyLayers(body);
                return false;
            }

            ResetOriginalBodyLayers(body);
            return true;
        }

        private void SetOriginalBodyLayers(IMonaBody body)
        {
            Transform[] tfs = body.Transform.GetComponentsInChildren<Transform>();

            for (int i = 0; i < tfs.Length; i++)
                _bodyLayers.Add(tfs[i].gameObject.layer);
        }

        private void ResetOriginalBodyLayers(IMonaBody body)
        {
            Transform[] tfs = body.Transform.GetComponentsInChildren<Transform>();

            if (tfs.Length > _bodyLayers.Count)
                return;

            for (int i = 0; i < tfs.Length; i++)
                tfs[i].gameObject.layer = _bodyLayers[i];

        }

        private void SetBodyLayer(IMonaBody body, LayerMask layer)
        {
            Transform[] tfs = body.Transform.GetComponentsInChildren<Transform>();

            for (int i = 0; i < tfs.Length; i++)
                tfs[i].gameObject.layer = layer;
        }

        private Bounds GetBodyBounds(IMonaBody body)
        {
            Bounds bounds = new Bounds(body.GetPosition(), Vector3.zero);

            Renderer[] renderers = body.Transform.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].GetComponent<LineRenderer>() == null)
                    bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        private float CalculateOffsetDistance(Bounds bounds, Vector3 normal)
        {
            // Calculate the distance from the center to the furthest point in the direction of the normal
            float maxDistance = 0f;
            Vector3[] points = new Vector3[8];

            points[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            points[1] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
            points[2] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            points[3] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
            points[4] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            points[5] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
            points[6] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
            points[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

            foreach (Vector3 point in points)
            {
                float distance = Vector3.Dot(point - bounds.center, normal);
                maxDistance = Mathf.Max(maxDistance, distance);
            }

            return maxDistance;
        }

        private Vector3 GetGamepadMovement()
        {
            if (SnapType == MovementSnapType.SnapToGrid && Time.time < _lastGamepadMoveTime + _snapInterval)
                return Vector3.zero;

            Vector2 inputVector;
            Vector3 move = Vector3.zero;

            switch (_device)
            {
                case PlayerInputDeviceType.GenericLook:
                case PlayerInputDeviceType.RightAnalogStick:
                    inputVector = _brainInput.ProcessInput(false, SDK.Core.Input.Enums.MonaInputType.Look, SDK.Core.Input.Enums.MonaInputState.None).LookValue;
                    break;
                case PlayerInputDeviceType.Vector2:
                    inputVector = _directionVector;
                    break;
                default:
                    inputVector = _brainInput.ProcessInput(false, SDK.Core.Input.Enums.MonaInputType.Move, SDK.Core.Input.Enums.MonaInputState.None).MoveValue;
                    break;
            }

            switch (_axisType)
            {
                case MovementPlaneType.Local:
                case MovementPlaneType.World:
                    move = GetMovementVector(inputVector);
                    break;
                case MovementPlaneType.ViewBasedPlaneLock:
                    move = GetPlaneLockedCameraRelativeMovement(inputVector);
                    break;
                case MovementPlaneType.TrueCameraRelative:
                    move = GetTrueCameraRelativeMovement(inputVector);
                    break;
            }

            _lastGamepadMoveTime = Time.time;
            move *= SnapType == MovementSnapType.SnapToGrid ? _gridSnapUnits : _speed * _deltaTime;
            return move;
        }

        private Vector3 GetMovementVector(Vector2 inputVector)
        {
            switch (_moveAlong)
            {
                case MovementMode.XY:
                    return new Vector3(inputVector.x, inputVector.y, 0f);
                case MovementMode.XZ:
                    return new Vector3(inputVector.x, 0f, inputVector.y);
                case MovementMode.YZ:
                    return new Vector3(0f, inputVector.y, inputVector.x);
                case MovementMode.X:
                    return new Vector3(inputVector.x, 0f, 0f);
                case MovementMode.Y:
                    return new Vector3(0f, inputVector.y, 0f);
                case MovementMode.Z:
                    return new Vector3(0f, 0f, inputVector.x);
            }

            return Vector3.zero;
        }

        private Vector3 GetPlaneLockedCameraRelativeMovement(Vector2 inputVector)
        {
            Vector3 cameraRight = _mainCamera.transform.right;
            Vector3 cameraUp = _mainCamera.transform.up;
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 move = Vector3.zero;

            switch (_moveAlong)
            {
                case MovementMode.XY:
                    cameraUp.z = 0;
                    cameraUp.x = 0;
                    cameraRight.y = 0;
                    cameraRight.Normalize();
                    cameraUp.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.XZ:
                    cameraForward.y = 0;
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();
                    move = cameraForward * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.YZ:
                    cameraUp.z = 0;
                    cameraUp.x = 0;
                    cameraForward.y = 0;
                    cameraForward.Normalize();
                    cameraUp.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraForward * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.X:
                    cameraUp = Vector3.zero;
                    cameraRight.y = 0;
                    cameraRight.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.Y:
                    cameraUp.z = 0;
                    cameraUp.x = 0;
                    cameraRight = Vector3.zero;
                    cameraUp.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.Z:
                    cameraForward.y = 0;
                    cameraRight = Vector3.zero;
                    cameraForward.Normalize();
                    move = cameraForward * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
            }

            return move;
        }

        private Vector3 GetTrueCameraRelativeMovement(Vector2 inputVector)
        {
            Vector3 cameraRight = _mainCamera.transform.right;
            Vector3 cameraUp = _mainCamera.transform.up;
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 move = Vector3.zero;

            switch (_moveAlong)
            {
                case MovementMode.XY:
                    cameraRight.y = 0;
                    cameraRight.Normalize();
                    cameraUp.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.XZ:
                    cameraRight.y = 0;
                    cameraForward.Normalize();
                    cameraRight.Normalize();
                    move = cameraForward * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.YZ:
                    cameraForward.Normalize();
                    cameraUp.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraForward * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.X:
                    cameraUp = Vector3.zero;
                    cameraRight.y = 0;
                    cameraRight.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.Y:
                    cameraRight = Vector3.zero;
                    cameraUp.Normalize();
                    move = cameraUp * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
                case MovementMode.Z:
                    cameraRight = Vector3.zero;
                    cameraForward.Normalize();
                    move = cameraForward * (Mathf.Approximately(inputVector.y, 0) ?
                        0 : Mathf.Sign(inputVector.y)) + cameraRight * (Mathf.Approximately(inputVector.x, 0) ?
                            0 : Mathf.Sign(inputVector.x));
                    break;
            }

            return move;
        }

        private void ApplyGamepadInput(IMonaBody body, Vector3 move)
        {
            if (_axisType == MovementPlaneType.Local)
                ApplyLocalMovementVector(body, move);
            else
                ApplyGlobalMovementVector(body, move);
        }

        private void ApplyGlobalMovementVector(IMonaBody body, Vector3 move)
        {
            Vector3 bodyPosition = body.GetPosition();
            Vector3 newPosition = bodyPosition + move;

            newPosition = SnapToGrid(newPosition);

            Vector3 direction = newPosition - bodyPosition;
            body.AddPosition(direction);
        }

        private void ApplyLocalMovementVector(IMonaBody body, Vector3 move)
        {
            Vector3 localPosition = body.Transform.localPosition;
            Vector3 newPosition = localPosition + move;

            Vector3 direction = newPosition - localPosition;
            direction = body.Transform.TransformDirection(direction);

            if (SnapType == MovementSnapType.SnapToGrid)
                direction = direction.normalized * _gridSnapUnits;

            body.AddPosition(direction);
        }

        private Vector3 ClampPositionToMaxDistance(Vector3 position)
        {
            Vector3 cameraPosition = _mainCamera.transform.position;
            Vector3 direction = (position - cameraPosition).normalized;
            float distance = Vector3.Distance(cameraPosition, position);

            if (distance > _range)
            {
                Vector3 newPosition = cameraPosition + direction * _range;

                switch (_moveAlong)
                {
                    case MovementMode.XY:
                        position.x = newPosition.x;
                        position.y = newPosition.y;
                        break;
                    case MovementMode.XZ:
                        position.x = newPosition.x;
                        position.z = newPosition.z;
                        break;
                    case MovementMode.YZ:
                        position.y = newPosition.y;
                        position.z = newPosition.z;
                        break;
                    case MovementMode.X:
                        position.x = newPosition.x;
                        break;
                    case MovementMode.Y:
                        position.y = newPosition.y;
                        break;
                    case MovementMode.Z:
                        position.z = newPosition.z;
                        break;
                }
            }

            return position;
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            if (SnapType == MovementSnapType.NoSnap)
                return position;

            position.x = Mathf.Round(position.x / _gridSnapUnits) * _gridSnapUnits;
            position.y = Mathf.Round(position.y / _gridSnapUnits) * _gridSnapUnits;
            position.z = Mathf.Round(position.z / _gridSnapUnits) * _gridSnapUnits;

            return position;
        }

        private void SetTargetBodies()
        {
            switch (_target)
            {
                case MonaBrainTransformTargetType.Tag:
                    _targetBodies.AddRange(MonaBody.FindByTag(_targetTag));
                    break;
                case MonaBrainTransformTargetType.Self:
                    IMonaBody topBody = _brain.Body;
                    while (topBody.Parent != null)
                        topBody = topBody.Parent;
                    _targetBodies.Add(topBody);
                    break;
                case MonaBrainTransformTargetType.Parent:
                    IMonaBody parent = _brain.Body.Parent;
                    if (parent != null) _targetBodies.Add(parent);
                    break;
                case MonaBrainTransformTargetType.Children:
                    _targetBodies.AddRange(_brain.Body.Children());
                    break;
                case MonaBrainTransformTargetType.ThisBodyOnly:
                    _targetBodies.Add(_brain.Body);
                    break;
                case MonaBrainTransformTargetType.OnConditionTarget:
                    _targetBodies.Add(_brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET));
                    break;
                case MonaBrainTransformTargetType.OnSelectTarget:
                    _targetBodies.Add(_brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET));
                    break;
                case MonaBrainTransformTargetType.MySpawner:
                    _targetBodies.Add(_brain.Body.Spawner);
                    break;
                case MonaBrainTransformTargetType.LastSpawnedByMe:
                    _targetBodies.Add(_brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED));
                    break;
                case MonaBrainTransformTargetType.MyPoolPreviouslySpawned:
                    _targetBodies.Add(_brain.Body.PoolBodyPrevious);
                    break;
                case MonaBrainTransformTargetType.MyPoolNextSpawned:
                    _targetBodies.Add(_brain.Body.PoolBodyNext);
                    break;
                case MonaBrainTransformTargetType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null) _targetBodies.Add(brain.Body);
                    break;
                case MonaBrainTransformTargetType.MyBodyArray:
                    _targetBodies.AddRange(_brain.Variables.GetBodyArray(_bodyArray));
                    break;
            }
        }

        private void SetClosestBodyToPointer()
        {
            _closestBodyToPointer = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < _targetBodies.Count; i++)
            {
                if (_targetBodies[i] == null)
                    continue;

                Vector3 bodyPosition = _targetBodies[i].GetPosition();
                Vector3 screenPosition = _mainCamera.WorldToScreenPoint(bodyPosition);
                float distance = Vector2.Distance(new Vector2(_pointerPosition.x, _pointerPosition.y), new Vector2(screenPosition.x, screenPosition.y));

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    _closestBodyToPointer = _targetBodies[i];
                    _closestBodyPositionToPointer = bodyPosition;
                }
            }
        }
    }
}