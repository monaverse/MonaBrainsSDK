using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveWithInputInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
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

        [SerializeField] private PlayerInputDeviceType _device = PlayerInputDeviceType.Mouse;
        [BrainPropertyEnum(true)] public PlayerInputDeviceType Device { get => _device; set => _device = value; }

        [SerializeField] private MovementMode _moveAlong = MovementMode.XZ;
        [BrainPropertyEnum(true)] public MovementMode MoveAlong { get => _moveAlong; set => _moveAlong = value; }

        [SerializeField] private float _range = 30f;
        [SerializeField] private string _rangeName;
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
        [BrainPropertyShow(nameof(SnapType), (int)MovementSnapType.NoSnap)]
        [BrainProperty(false)] public float Speed { get => _speed; set => _speed = value; }
        [BrainPropertyValueName("Speed", typeof(IMonaVariablesFloatValue))] public string SpeedName { get => _speedName; set => _speedName = value; }

        private float _previousTime = 0;
        private float _deltaTime;
        private float _lastGamepadMoveTime = 0f;
        private IMonaBrain _brain;
        private Camera _mainCamera;
        private List<IMonaBody> _targetBodies = new List<IMonaBody>();

        public MoveWithInputInstructionTile() { }

        public MovementSnapType SnapType => _gridSnapUnits > 0 ? MovementSnapType.SnapToGrid : MovementSnapType.NoSnap;

        private bool UseGamepad
        {
            get
            {
                switch (_device)
                {
                    case PlayerInputDeviceType.GenericMovement:
                    case PlayerInputDeviceType.DigitalPad:
                    case PlayerInputDeviceType.LeftAnalogStick:
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
        }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
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

            _mainCamera = Camera.main;
            _targetBodies.Clear();

            if (!string.IsNullOrEmpty(_rangeName))
                _range = _brain.Variables.GetFloat(_rangeName);

            if (!string.IsNullOrEmpty(_gridSnapUnitsName))
                _gridSnapUnits = _brain.Variables.GetFloat(_gridSnapUnitsName);

            if (!string.IsNullOrEmpty(_snapIntervalName))
                _snapInterval = _brain.Variables.GetFloat(_snapIntervalName);

            if (!string.IsNullOrEmpty(_speedName))
                _speed = _brain.Variables.GetFloat(_speedName);

            _deltaTime = GetAndUpdateDeltaTime();

            SetTargetBodies();

            Vector3 gamepadMovement = UseGamepad ? GetGamepadMovement() : Vector3.zero;

            for (int i = 0; i < _targetBodies.Count; i++)
            {
                switch (_device)
                {
                    case PlayerInputDeviceType.GenericPointer:
                        if (UnityEngine.Input.touchCount > 0)
                            SetAllBodiesWithTouch();
                        else
                            SetAllBodiesWithMouse();
                        break;
                    case PlayerInputDeviceType.Mouse:
                        SetAllBodiesWithMouse();
                        break;
                    case PlayerInputDeviceType.TouchScreen:
                        SetAllBodiesWithTouch();
                        break;
                    case PlayerInputDeviceType.GenericMovement:
                    case PlayerInputDeviceType.DigitalPad:
                    case PlayerInputDeviceType.LeftAnalogStick:
                        SetAllBodiesWithGamepad(gamepadMovement);
                        break;
                    case PlayerInputDeviceType.GenericLook:
                    case PlayerInputDeviceType.RightAnalogStick:
                        break;
                }
            }

            return Complete(InstructionTileResult.Success);
        }

        private void SetAllBodiesWithMouse()
        {
            for (int i = 0; i < _targetBodies.Count; i++)
                SetBodyToMousePosition(_targetBodies[i]);
        }

        private void SetBodyToMousePosition(IMonaBody body)
        {
            if (body == null) return;
            Vector3 pointerWorldPosition = GetPointerWorldPosition(body, UnityEngine.Input.mousePosition);
            Vector3 offset = GetPointerBodyOffset(body, pointerWorldPosition);
            ApplyPointerInput(body, offset, pointerWorldPosition);
        }

        private void SetAllBodiesWithTouch()
        {
            if (UnityEngine.Input.touchCount < 1)
                return;

            for (int i = 0; i < _targetBodies.Count; i++)
                SetBodyToTouchPosition(_targetBodies[i]);
        }

        private void SetBodyToTouchPosition(IMonaBody body)
        {
            if (body == null) return;

            Touch touch = UnityEngine.Input.GetTouch(0);
            Vector3 pointerWorldPosition = GetPointerWorldPosition(body, touch.position);
            Vector3 offset = GetPointerBodyOffset(body, pointerWorldPosition);
            ApplyPointerInput(body, offset, pointerWorldPosition);
        }

        private void SetAllBodiesWithGamepad(Vector3 move)
        {
            for (int i = 0; i < _targetBodies.Count; i++)
                ApplyGamepadInput(_targetBodies[i], move);
        }

        private Vector3 GetPointerBodyOffset(IMonaBody body, Vector3 pointerWorldPosition)
        {
            return body.GetPosition() - pointerWorldPosition;
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
                    {
                        return ray.GetPoint(distanceXY);
                    }
                    break;
                case MovementMode.XZ:
                    Plane planeXZ = new Plane(Vector3.up, new Vector3(0, bodyPosition.y, 0));
                    if (planeXZ.Raycast(ray, out float distanceXZ))
                    {
                        return ray.GetPoint(distanceXZ);
                    }
                    break;
                case MovementMode.YZ:
                    Plane planeYZ = new Plane(Vector3.right, new Vector3(bodyPosition.x, 0, 0));
                    if (planeYZ.Raycast(ray, out float distanceYZ))
                    {
                        return ray.GetPoint(distanceYZ);
                    }
                    break;
                case MovementMode.X:
                    Plane planeX = new Plane(Vector3.up, new Vector3(0, 0, bodyPosition.z));
                    if (planeX.Raycast(ray, out float distanceX))
                    {
                        return ray.GetPoint(distanceX);
                    }
                    break;
                case MovementMode.Y:
                    Plane planeY = new Plane(Vector3.forward, new Vector3(bodyPosition.x, 0, 0));
                    if (planeY.Raycast(ray, out float distanceY))
                    {
                        return ray.GetPoint(distanceY);
                    }
                    break;
                case MovementMode.Z:
                    Plane planeZ = new Plane(Vector3.up, new Vector3(0, bodyPosition.y, 0));
                    if (planeZ.Raycast(ray, out float distanceZ))
                    {
                        return ray.GetPoint(distanceZ);
                    }
                    break;
            }

            return Vector3.zero;
        }

        private Vector3 GetGamepadMovement()
        {
            if (SnapType == MovementSnapType.SnapToGrid && Time.time < _lastGamepadMoveTime + _snapInterval)
                return Vector3.zero;

            float horizontal = UnityEngine.Input.GetAxis("Horizontal");
            float vertical = UnityEngine.Input.GetAxis("Vertical");

            float threshold = 0.1f;

            if (Mathf.Abs(horizontal) > threshold || Mathf.Abs(vertical) > threshold)
            {
                Vector3 move = Vector3.zero;
                float moveMultiplier = SnapType == MovementSnapType.SnapToGrid ? _gridSnapUnits : _speed * _deltaTime;

                switch (_moveAlong)
                {
                    case MovementMode.XY:
                        move = _mainCamera.transform.TransformDirection(new Vector3(horizontal, vertical, 0)) * moveMultiplier;
                        break;
                    case MovementMode.XZ:
                        move = _mainCamera.transform.TransformDirection(new Vector3(horizontal, 0, vertical)) * moveMultiplier;
                        break;
                    case MovementMode.YZ:
                        move = _mainCamera.transform.TransformDirection(new Vector3(0, vertical, horizontal)) * moveMultiplier;
                        break;
                    case MovementMode.X:
                        move = _mainCamera.transform.TransformDirection(new Vector3(horizontal, 0, 0)) * moveMultiplier;
                        break;
                    case MovementMode.Y:
                        move = _mainCamera.transform.TransformDirection(new Vector3(0, vertical, 0)) * moveMultiplier;
                        break;
                    case MovementMode.Z:
                        move = _mainCamera.transform.TransformDirection(new Vector3(0, 0, horizontal)) * moveMultiplier;
                        break;
                }

                _lastGamepadMoveTime = Time.time;
                return move;
            }

            return Vector3.zero;
        }

        private void ApplyPointerInput(IMonaBody body, Vector3 offset, Vector3 pointerWorldPosition)
        {
            Vector3 pointerPosition = SnapToGrid(pointerWorldPosition); // + offset);

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

        private void ApplyGamepadInput(IMonaBody body, Vector3 move)
        {
            Vector3 bodyPosition = body.GetPosition();
            Vector3 newPosition = bodyPosition + move;

            newPosition = SnapToGrid(newPosition);
            newPosition = ClampPositionToMaxDistance(newPosition);

            body.TeleportPosition(newPosition);
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
            }
        }
    }
}