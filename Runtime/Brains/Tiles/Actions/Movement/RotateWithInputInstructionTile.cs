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
    public class RotateWithInputInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, ITickAfterInstructionTile
    {
        public const string ID = "RotateWithInput";
        public const string NAME = "Rotate With Input";
        public const string CATEGORY = "Adv Rotation";
        public override Type TileType => typeof(RotateWithInputInstructionTile);

        [SerializeField] private MonaBrainTransformTargetType _target = MonaBrainTransformTargetType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainTransformTargetType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag = "Player";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformTargetType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformTargetType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private OrbitInputDeviceType _device = OrbitInputDeviceType.GenericMovement;
        [BrainPropertyEnum(true)] public OrbitInputDeviceType Device { get => _device; set => _device = value; }

        [SerializeField] private Vector2 _directionVector;
        [SerializeField] private string[] _directionVectorName;
        [BrainPropertyShow(nameof(Device), (int)OrbitInputDeviceType.Vector2)]
        [BrainProperty(true)] public Vector2 DirectionVector { get => _directionVector; set => _directionVector = value; }
        [BrainPropertyValueName("DirectionVector", typeof(IMonaVariablesVector2Value))] public string[] DirectionVectorName { get => _directionVectorName; set => _directionVectorName = value; }

        [SerializeField] private float _speed = 90f;
        [SerializeField] private string _speedName;
        [BrainPropertyShow(nameof(SnapType), (int)MovementSnapType.NoSnap)]
        [BrainProperty(true)] public float Speed { get => _speed; set => _speed = value; }
        [BrainPropertyValueName("Speed", typeof(IMonaVariablesFloatValue))] public string SpeedName { get => _speedName; set => _speedName = value; }

        [SerializeField] private InputAxisUsageType _inputAxis = InputAxisUsageType.Both;
        [BrainPropertyEnum(false)] public InputAxisUsageType InputAxis { get => _inputAxis; set => _inputAxis = value; }

        [SerializeField] private InputRotationType _rotationType = InputRotationType.Local;
        [BrainPropertyEnum(false)] public InputRotationType RotationType { get => _rotationType; set => _rotationType = value; }

        [SerializeField] private MovementMode _rotationAxis = MovementMode.XY;
        [BrainPropertyShow(nameof(RotationType), (int)InputRotationType.Local)]
        [BrainPropertyShow(nameof(RotationType), (int)InputRotationType.World)]
        [BrainPropertyEnum(false)] public MovementMode RotationAxis { get => _rotationAxis; set => _rotationAxis = value; }

        [SerializeField] private float _angleSnap = 0f;
        [SerializeField] private string __angleSnapName;
        [BrainProperty(false)] public float AngleSnap { get => _angleSnap; set => _angleSnap = value; }
        [BrainPropertyValueName("AngleSnap", typeof(IMonaVariablesFloatValue))] public string AngleSnapName { get => __angleSnapName; set => __angleSnapName = value; }

        [SerializeField] private float _snapInterval = 0.1f;
        [SerializeField] private string _snapIntervalName;
        [BrainPropertyShow(nameof(SnapType), (int)MovementSnapType.AngleSnap)]
        [BrainProperty(false)] public float SnapInterval { get => _snapInterval; set => _snapInterval = value; }
        [BrainPropertyValueName("SnapInterval", typeof(IMonaVariablesFloatValue))] public string SnapIntervalName { get => _snapIntervalName; set => _snapIntervalName = value; }

        [SerializeField] private bool _invertDeviceX = false;
        [SerializeField] private string _invertDeviceXName;
        [BrainProperty(false)] public bool InvertDeviceX { get => _invertDeviceX; set => _invertDeviceX = value; }
        [BrainPropertyValueName("InvertDeviceX", typeof(IMonaVariablesBoolValue))] public string InvertInputXName { get => _invertDeviceXName; set => _invertDeviceXName = value; }

        [SerializeField] private bool _invertDeviceY = true;
        [SerializeField] private string _invertDeviceYName;
        [BrainProperty(false)] public bool InvertDeviceY { get => _invertDeviceY; set => _invertDeviceY = value; }
        [BrainPropertyValueName("InvertDeviceY", typeof(IMonaVariablesBoolValue))] public string InvertYInputName { get => _invertDeviceYName; set => _invertDeviceYName = value; }

        [SerializeField] private bool _swapDeviceAxes = false;
        [SerializeField] private string _swapDeviceAxesName;
        [BrainProperty(false)] public bool SwapDeviceAxes { get => _swapDeviceAxes; set => _swapDeviceAxes = value; }
        [BrainPropertyValueName("SwapDeviceAxes", typeof(IMonaVariablesBoolValue))] public string SwapDeviceAxesName { get => _swapDeviceAxesName; set => _swapDeviceAxesName = value; }

        private float _previousTime = 0;
        private float _lastGamepadMoveTime = 0;
        private float _deltaTime;
        private IMonaBrain _brain;
        private IMonaBrainInput _brainInput;
        private List<IMonaBody> _targetBodies = new List<IMonaBody>();

        public MovementSnapType SnapType => _angleSnap > 0 ? MovementSnapType.AngleSnap : MovementSnapType.NoSnap;

        public RotateWithInputInstructionTile() { }

        private bool UsePointer
        {
            get
            {
                switch (_device)
                {
                    case OrbitInputDeviceType.GenericPointer:
                    case OrbitInputDeviceType.Mouse:
                    case OrbitInputDeviceType.TouchScreen:
                        return true;
                }
                return false;
            }
        }

        public enum MovementSnapType
        {
            NoSnap = 0,
            AngleSnap = 1
        }

        [Serializable]
        public enum OrbitDirectionType
        {
            Up = 0,
            Down = 1,
            Right = 2,
            Left = 3,
            UseVector = 10,
            InputDevice = 20
        }

        public enum OrbitInputDeviceType
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

        [Serializable]
        public enum InputAxisUsageType
        {
            Both = 0,
            DominantAxis = 10,
            VerticalOnly = 20,
            HorizontalOnly = 30
        }

        public enum InputRotationType
        {
            Local = 0,
            World = 10,
            WorldCameraRelative = 20
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

        private Transform _proxy;

        public virtual void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;
            _brainInput = MonaGlobalBrainRunner.Instance.GetBrainInput();

            if (_proxy == null)
                _proxy = (new GameObject($"ProxyRotator{_brain.Name}")).transform;

            if (_device != OrbitInputDeviceType.Vector2)
                _brainInput.StartListening(this);
        }

        public override void Unload(bool destroyed = false)
        {
            if (destroyed)
            {
                if(_proxy != null && _proxy.gameObject != null)
                    GameObject.Destroy(_proxy.gameObject);
                _proxy = null;
            }
            if (_device != OrbitInputDeviceType.Vector2)
                _brainInput.StopListening(this);
        }

        float GetAndUpdateDeltaTime()
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

            if (HasVector2Values(_directionVectorName))
                _directionVector = GetVector2Value(_brain, _directionVectorName);

            if (!string.IsNullOrEmpty(_speedName))
                _speed = _brain.Variables.GetFloat(_speedName);

            if (!string.IsNullOrEmpty(__angleSnapName))
                _angleSnap = _brain.Variables.GetFloat(__angleSnapName);

            if (!string.IsNullOrEmpty(_snapIntervalName))
                _snapInterval = _brain.Variables.GetFloat(_snapIntervalName);

            if (!string.IsNullOrEmpty(_invertDeviceXName))
                _invertDeviceX = _brain.Variables.GetBool(_invertDeviceXName);

            if (!string.IsNullOrEmpty(_invertDeviceYName))
                _invertDeviceY = _brain.Variables.GetBool(_invertDeviceYName);

            if (!string.IsNullOrEmpty(_swapDeviceAxesName))
                _swapDeviceAxes = _brain.Variables.GetBool(_swapDeviceAxesName);

            _deltaTime = GetAndUpdateDeltaTime();

            if (SnapType == MovementSnapType.AngleSnap && Time.time < _lastGamepadMoveTime + _snapInterval)
                return Complete(InstructionTileResult.Success);

            _lastGamepadMoveTime = Time.time;

            SetTargetBodies();
            Vector2 deviceVector = GetDeviceVector();

            for (int i = 0; i < _targetBodies.Count; i++)
                RotateBody(_targetBodies[i], deviceVector);

            return Complete(InstructionTileResult.Success);
        }

        private void RotateBody(IMonaBody body, Vector2 deviceVector)
        {
            if (body == null)
                return;

            switch (_rotationType)
            {
                case InputRotationType.Local:
                    RotateLocal(body, deviceVector);
                    break;
                case InputRotationType.World:
                    RotateGlobal(body, deviceVector, false);
                    break;
                case InputRotationType.WorldCameraRelative:
                    RotateGlobal(body, deviceVector, true);
                    break;
            }
        }

        private void RotateLocal(IMonaBody body, Vector2 deviceVector)
        {
            float h = deviceVector.y;
            float v = deviceVector.x;
            float n = 0f;

            Quaternion rotation = Quaternion.identity;

            switch (_rotationAxis)
            {
                case MovementMode.XY:
                    rotation = Quaternion.Euler(h, v, n);
                    break;
                case MovementMode.XZ:
                    rotation = Quaternion.Euler(v, n, h);
                    break;
                case MovementMode.YZ:
                    rotation = Quaternion.Euler(n, v, h);
                    break;
                case MovementMode.X:
                    float x = GreatestVectorValue(deviceVector);
                    rotation = Quaternion.Euler(x, n, n);
                    break;
                case MovementMode.Y:
                    float y = GreatestVectorValue(deviceVector);
                    rotation = Quaternion.Euler(n, y, n);
                    break;
                case MovementMode.Z:
                    float z = GreatestVectorValue(deviceVector);
                    rotation = Quaternion.Euler(n, n, z);
                    break;
            }

            body.SetRotation(rotation);
        }

        private float GreatestVectorValue(Vector2 vector)
        {
            return Mathf.Abs(vector.x) > Mathf.Abs(vector.y) ? vector.x : vector.y;
        }

        private void RotateGlobal(IMonaBody body, Vector2 deviceVector, bool cameraRelative)
        {
            Vector3 verticalAxis = cameraRelative ? GetCameraRelativeVerticalAxis() : GetGlobalVerticalAxis();

            if (cameraRelative || (int)_rotationAxis < 3)
            {
                Vector3 horizontalAxis = cameraRelative ? Vector3.up : GetGlobalHorizontalAxis();
                _proxy.rotation = body.Transform.rotation;
                _proxy.Rotate(horizontalAxis, deviceVector.x, Space.World);
                _proxy.Rotate(verticalAxis, deviceVector.y, Space.World);
                body.TeleportRotation(_proxy.rotation);
            }
            else
            {
                float rotation = GreatestVectorValue(deviceVector);
                _proxy.rotation = body.Transform.rotation;
                _proxy.Rotate(verticalAxis, rotation, Space.World);
                body.TeleportRotation(_proxy.rotation);
            }
        }

        private Vector3 GetCameraRelativeVerticalAxis()
        {
            Camera camera = MonaGlobalBrainRunner.Instance.SceneCamera;

            if (Vector3.Dot(camera.transform.forward, Vector3.forward) > 0.5f)
            {
                return Vector3.right;
            }
            else if (Vector3.Dot(camera.transform.forward, Vector3.back) > 0.5f)
            {
                return Vector3.left;
            }
            else if (Vector3.Dot(camera.transform.forward, Vector3.right) > 0.5f)
            {
                return Vector3.back;
            }
            else if (Vector3.Dot(camera.transform.forward, Vector3.left) > 0.5f)
            {
                return Vector3.forward;
            }

            return Vector3.right;
        }

        private Vector3 GetGlobalVerticalAxis()
        {
            switch (_rotationAxis)
            {
                case MovementMode.YZ:
                case MovementMode.Z:
                    return Vector3.forward;
                case MovementMode.Y:
                    return Vector3.up;
                default:
                    return Vector3.right;
            }
        }

        private Vector3 GetGlobalHorizontalAxis()
        {
            switch (_rotationAxis)
            {
                case MovementMode.X:
                    return Vector3.right;
                case MovementMode.XZ:
                case MovementMode.Z:
                    return Vector3.forward;
                default:
                    return Vector3.up;
            }
        }

        private Vector2 GetDeviceVector()
        {
            Vector2 inputVector = Vector2.zero;

            switch (_device)
            {
                case OrbitInputDeviceType.GenericPointer:
                case OrbitInputDeviceType.TouchScreen:
                case OrbitInputDeviceType.Mouse:
                    inputVector = GetMouseVector();
                    break;
                case OrbitInputDeviceType.RightAnalogStick:
                case OrbitInputDeviceType.GenericLook:
                    inputVector = SetInputVector(_brainInput.ProcessInput(false, SDK.Core.Input.Enums.MonaInputType.Look, SDK.Core.Input.Enums.MonaInputState.None).LookValue);
                    break;
                case OrbitInputDeviceType.DigitalPad:
                case OrbitInputDeviceType.LeftAnalogStick:
                case OrbitInputDeviceType.GenericMovement:
                    inputVector = SetInputVector(_brainInput.ProcessInput(false, SDK.Core.Input.Enums.MonaInputType.Move, SDK.Core.Input.Enums.MonaInputState.None).MoveValue);//GetMoveVector(); //SetInputVector(_instruction.InstructionInput.MoveValue);
                    break;
                case OrbitInputDeviceType.Vector2:
                    inputVector = _directionVector;
                    break;
            }

            switch (_inputAxis)
            {
                case InputAxisUsageType.DominantAxis:
                    if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
                        inputVector.y = 0;
                    else
                        inputVector.x = 0;
                    break;
                case InputAxisUsageType.VerticalOnly:
                    inputVector.x = 0;
                    break;
                case InputAxisUsageType.HorizontalOnly:
                    inputVector.y = 0;
                    break;
            }

            if (SnapType == MovementSnapType.NoSnap)
            {
                inputVector *= UsePointer ? _speed : _speed * _deltaTime;
            }
            else
            {
                inputVector.x = MathF.Sign(inputVector.x) * _angleSnap;
                inputVector.y = MathF.Sign(inputVector.y) * _angleSnap;
            }

            if (_swapDeviceAxes)
                inputVector = new Vector2(inputVector.y, inputVector.x);

            return inputVector;
        }

        private Vector2 GetMouseVector()
        {
            float x = UnityEngine.Input.GetAxis("Mouse X");
            float y = UnityEngine.Input.GetAxis("Mouse Y");
            return SetInputVector(x, y);
        }

        private Vector2 SetInputVector(Vector2 inputVector)
        {
            return SetInputVector(inputVector.x, inputVector.y);
        }

        private Vector2 SetInputVector(float x, float y)
        {
            if (!_invertDeviceX)
                x *= -1f;

            if (_invertDeviceY)
                y *= -1f;

            var input = new Vector2(x, y);
            if (input.magnitude > 1f)
                return input.normalized;
            else
                return input;
        }

        private void SetTargetBodies()
        {
            _targetBodies.Clear();

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
    }
}