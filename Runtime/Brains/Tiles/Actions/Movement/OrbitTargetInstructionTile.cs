using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class OrbitTargetInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "OrbitTarget";
        public const string NAME = "Orbit Target";
        public const string CATEGORY = "Adv Rotation";
        public override Type TileType => typeof(OrbitTargetInstructionTile);

        [SerializeField] private MonaBrainOrbitTargetType _target = MonaBrainOrbitTargetType.WorldSpace;
        [BrainPropertyEnum(true)] public MonaBrainOrbitTargetType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private string[] _targetPositionName = new string[4];
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformType.LocalSpace)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformType.WorldSpace)]
        [BrainProperty(true)] public Vector3 TargetPosition { get => _targetPosition; set => _targetPosition = value; }
        [BrainPropertyValueName("TargetPosition", typeof(IMonaVariablesVector3Value))] public string[] TargetPositionName { get => _targetPositionName; set => _targetPositionName = value; }

        [SerializeField] private float _speed = 10f;
        [SerializeField] private string _speedName;
        [BrainProperty(true)] public float Speed { get => _speed; set => _speed = value; }
        [BrainPropertyValueName("Speed", typeof(IMonaVariablesFloatValue))] public string SpeedName { get => _speedName; set => _speedName = value; }

        [SerializeField] private OrbitDirectionType _direction = OrbitDirectionType.Right;
        [BrainPropertyEnum(true)] public OrbitDirectionType Direction { get => _direction; set => _direction = value; }

        [SerializeField] private OrbitInputDeviceType _device = OrbitInputDeviceType.Mouse;
        [BrainPropertyShow(nameof(Direction), (int)OrbitDirectionType.InputDevice)]
        [BrainPropertyEnum(true)] public OrbitInputDeviceType Device { get => _device; set => _device = value; }

        [SerializeField] private Vector2 _directionVector;
        [SerializeField] private string[] _directionVectorName;
        [BrainPropertyShow(nameof(Direction), (int)OrbitDirectionType.UseVector)]
        [BrainProperty(true)] public Vector2 DirectionVector { get => _directionVector; set => _directionVector = value; }
        [BrainPropertyValueName("DirectionVector", typeof(IMonaVariablesVector2Value))] public string[] DirectionVectorName { get => _directionVectorName; set => _directionVectorName = value; }

        [SerializeField] private InputAxisUsageType _inputAxis = InputAxisUsageType.Both;
        [BrainPropertyShow(nameof(Direction), (int)OrbitDirectionType.InputDevice)]
        [BrainPropertyEnum(false)] public InputAxisUsageType InputAxis { get => _inputAxis; set => _inputAxis = value; }

        [SerializeField] private bool _invertXInput = false;
        [SerializeField] private string _invertXInputName;
        [BrainPropertyShow(nameof(Direction), (int)OrbitDirectionType.InputDevice)]
        [BrainProperty(false)] public bool InvertXInput { get => _invertXInput; set => _invertXInput = value; }
        [BrainPropertyValueName("InvertXInput", typeof(IMonaVariablesBoolValue))] public string InvertXInputName { get => _invertXInputName; set => _invertXInputName = value; }

        [SerializeField] private bool _invertYInput = true;
        [SerializeField] private string _invertYInputName;
        [BrainPropertyShow(nameof(Direction), (int)OrbitDirectionType.InputDevice)]
        [BrainProperty(false)] public bool InvertYInput { get => _invertYInput; set => _invertYInput = value; }
        [BrainPropertyValueName("InvertYInput", typeof(IMonaVariablesBoolValue))] public string InvertYInputName { get => _invertYInputName; set => _invertYInputName = value; }

        [SerializeField] private bool _faceTarget = true;
        [SerializeField] private string _faceTargetName;
        [BrainProperty(false)] public bool FaceTarget { get => _faceTarget; set => _faceTarget = value; }
        [BrainPropertyValueName("FaceTarget", typeof(IMonaVariablesBoolValue))] public string FaceTargetName { get => _faceTargetName; set => _faceTargetName = value; }

        private IMonaBrain _brain;

        public OrbitTargetInstructionTile() { }

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
            Mouse = 0,
            GenericLook = 10,
            GenericMovement = 20,
            DigitalPad = 30,
            LeftAnalogStick = 40,
            RightAnalogStick = 50,
            TouchScreen = 60
        }

        [Serializable]
        public enum InputAxisUsageType
        {
            Both = 0,
            VerticalOnly = 10,
            HorizontalOnly = 20
        }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        Vector3 _bodyPosition;
        float _previousTime = 0;

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

            if (HasVector3Values(_targetPositionName))
                _targetPosition = GetVector3Value(_brain, _targetPositionName);

            if (!string.IsNullOrEmpty(_speedName))
                _speed = _brain.Variables.GetFloat(_speedName);

            if (HasVector2Values(_directionVectorName))
                _directionVector = GetVector2Value(_brain, _directionVectorName);

            if (!string.IsNullOrEmpty(_invertXInputName))
                _invertXInput = _brain.Variables.GetBool(_invertXInputName);

            if (!string.IsNullOrEmpty(_invertYInputName))
                _invertYInput = _brain.Variables.GetBool(_invertYInputName);

            if (!string.IsNullOrEmpty(_faceTargetName))
                _faceTarget = _brain.Variables.GetBool(_faceTargetName);

            GetTargetPosition(out Vector3 targetPosition, out bool targetFound, out bool useLocalSpace);

            if (!targetFound)
                return Complete(InstructionTileResult.Success);

            _bodyPosition = _brain.Body.GetPosition();
            Vector2 orbitVector = GetOrbitVector();
            Quaternion rotation = Quaternion.Euler(orbitVector.y * _speed, orbitVector.x * _speed, 0);
            Vector3 offset = rotation * (_bodyPosition - targetPosition);
            Vector3 newPosition = targetPosition + offset;

            _brain.Body.TeleportPosition(newPosition);

            if (_faceTarget)
            {
                Vector3 lookDirection = targetPosition - newPosition;
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

                /* make sure orbit honors bind values */
                lookRotation = _brain.Body.RotationBounds.BindValue(lookRotation, _brain.Body.ActiveTransform);

                newPosition = targetPosition - (lookRotation * Vector3.forward) * (targetPosition - newPosition).magnitude;
                _brain.Body.TeleportPosition(newPosition);

                _brain.Body.TeleportRotation(lookRotation);
            }

            return Complete(InstructionTileResult.Success);
        }

        private Vector2 GetOrbitVector()
        {
            float deltaTime = GetAndUpdateDeltaTime();

            switch (_direction)
            {
                case OrbitDirectionType.Up:
                    return new Vector2(0, 1 * deltaTime);
                case OrbitDirectionType.Down:
                    return new Vector2(0, -1 * deltaTime);
                case OrbitDirectionType.Right:
                    return new Vector2(-1 * deltaTime, 0);
                case OrbitDirectionType.Left:
                    return new Vector2(1 * deltaTime, 0);
                case OrbitDirectionType.UseVector:
                    return _directionVector * deltaTime;
                case OrbitDirectionType.InputDevice:
                    return GetDeviceVector();
            }

            return Vector2.zero;
        }

        private Vector2 GetDeviceVector()
        {
            switch (_device)
            {
                case OrbitInputDeviceType.Mouse:
                    return GetMouseVector();
            }

            return Vector2.zero;
        }

        private Vector2 GetMouseVector()
        {
            float x = UnityEngine.Input.GetAxis("Mouse X");
            float y = UnityEngine.Input.GetAxis("Mouse Y");

            return SetInputVector(x, y);
        }

        private Vector2 SetInputVector(float x, float y)
        {
            if (_invertXInput)
                x *= -1f;

            if (_invertYInput)
                y *= -1f;

            if (_inputAxis == InputAxisUsageType.VerticalOnly)
                x = 0;
            else if (_inputAxis == InputAxisUsageType.HorizontalOnly)
                y = 0;

            return new Vector2(x, y);
        }

        private void GetTargetPosition(out Vector3 position, out bool targetFound, out bool useLocalSpace)
        {
            useLocalSpace = false;

            switch (_target)
            {
                case MonaBrainOrbitTargetType.Parent:
                    Transform parent = _brain.Body.Transform.parent;
                    if (!parent)
                        break;
                    position = parent.position;
                    targetFound = true;
                    return;
                case MonaBrainOrbitTargetType.WorldSpace:
                    position = _targetPosition;
                    targetFound = true;
                    return;
                default:
                    var targetBody = GetTargetBody();
                    if (targetBody == null)
                        break;
                    position = targetBody.GetPosition();
                    targetFound = true;
                    return;
            }

            position = Vector3.zero;
            targetFound = false;
        }

        private IMonaBody GetTargetBody()
        {
            switch (_target)
            {
                case MonaBrainOrbitTargetType.Tag: return _brain.Body.GetClosestTag(_targetTag);
                case MonaBrainOrbitTargetType.OnConditionTarget: return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainOrbitTargetType.OnSelectTarget: return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainOrbitTargetType.MySpawner: return _brain.Body.Spawner;
                case MonaBrainOrbitTargetType.LastSpawnedByMe: return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainOrbitTargetType.MyPoolPreviouslySpawned: return _brain.Body.PoolBodyPrevious;
                case MonaBrainOrbitTargetType.MyPoolNextSpawned: return _brain.Body.PoolBodyNext;
                case MonaBrainOrbitTargetType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    return brain != null ? brain.Body : null;
                case MonaBrainOrbitTargetType.AnySpawnedByMe:
                    var spawned = _brain.SpawnedBodies;
                    return spawned.Count > 0 ? spawned[UnityEngine.Random.Range(0, spawned.Count)] : null;
            }
            return null;
        }
    }
}