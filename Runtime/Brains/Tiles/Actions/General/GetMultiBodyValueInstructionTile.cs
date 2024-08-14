using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System.Text;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class GetMultiBodyValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "GetMultiBodyValue";
        public const string NAME = "Get Multi Body Value";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(GetMultiBodyValueInstructionTile);

        [SerializeField] private MultiBodyValueType _valueType = MultiBodyValueType.Distance;
        [BrainPropertyEnum(true)] public MultiBodyValueType ValueType { get => _valueType; set => _valueType = value; }

        [SerializeField] private MonaBrainBroadcastTypeSingleTarget _originBody = MonaBrainBroadcastTypeSingleTarget.Self;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastTypeSingleTarget OriginBody { get => _originBody; set => _originBody = value; }

        [SerializeField] private string _originTag = "Player";
        [BrainPropertyShow(nameof(OriginBody), (int)MonaBrainBroadcastTypeSingleTarget.Tag)]
        [BrainPropertyMonaTag(true)] public string OriginTag { get => _originTag; set => _originTag = value; }

        [SerializeField] private string _originChild;
        [SerializeField] private string _originChildName;
        [BrainPropertyShow(nameof(OriginBody), (int)MonaBrainBroadcastTypeSingleTarget.ChildWithName)]
        [BrainPropertyShow(nameof(OriginBody), (int)MonaBrainBroadcastTypeSingleTarget.ChildContainingName)]
        [BrainProperty(true)] public string OriginChild { get => _originChild; set => _originChild = value; }
        [BrainPropertyValueName("OriginChild", typeof(IMonaVariablesStringValue))] public string OriginChildName { get => _originChildName; set => _originChildName = value; }

        [SerializeField] private MonaBrainBroadcastType _targetBody = MonaBrainBroadcastType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType TargetBody { get => _targetBody; set => _targetBody = value; }

        [SerializeField] private string _targetTag = "Default";
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _targetChild;
        [SerializeField] private string _targetChildName;
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(true)] public string TargetChild { get => _targetChild; set => _targetChild = value; }
        [BrainPropertyValueName("TargetChild", typeof(IMonaVariablesStringValue))] public string TargetChildName { get => _targetChildName; set => _targetChildName = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private DirectionReturnType _directionValue = DirectionReturnType.Vector;
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.Direction)]
        [BrainPropertyEnum(false)] public DirectionReturnType DirectionValue { get => _directionValue; set => _directionValue = value; }

        [SerializeField] private TargetVariableType _targetType;
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.Direction)]
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.RayHitPosition)]
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.RayHitNormal)]
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.HitNormalAlignAngle)]
        [BrainPropertyEnum(false)] public TargetVariableType TargetType { get => _targetType; set => _targetType = value; }

        [SerializeField] string _targetVector;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Vector3)]
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string TargetVector { get => _targetVector; set => _targetVector = value; }

        [SerializeField] private string _targetNumber;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Number)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string TargetNumber { get => _targetNumber; set => _targetNumber = value; }

        [SerializeField] private string _targetString;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.String)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string TargetString { get => _targetString; set => _targetString = value; }

        [SerializeField] private StringCopyType _copyType;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.String)]
        [BrainPropertyEnum(false)]
        public StringCopyType CopyType { get => _copyType; set => _copyType = value; }

        [SerializeField] private VectorThreeAxis _axis = VectorThreeAxis.Y;
        [BrainPropertyShow(nameof(AxisDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)]
        public VectorThreeAxis Axis { get => _axis; set => _axis = value; }

        [SerializeField] private BodyDistanceType _distanceType = BodyDistanceType.CompareTargets;
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.Distance)]
        [BrainPropertyEnum(false)] public BodyDistanceType DistanceType { get => _distanceType; set => _distanceType = value; }

        [SerializeField] private BodyDirectionType _direction = BodyDirectionType.Forward;
        [BrainPropertyShow(nameof(DirectionDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public BodyDirectionType Direction { get => _direction; set => _direction = value; }

        [SerializeField] private Vector3 _customDirection = Vector3.forward;
        [SerializeField] private string[] _customDirectionName;
        [BrainPropertyShow(nameof(CustomDirectionDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public Vector3 CustomDirection { get => _customDirection; set => _customDirection = value; }
        [BrainPropertyValueName("CustomDirection", typeof(IMonaVariablesVector3Value))] public string[] CustomDirectionName { get => _customDirectionName; set => _customDirectionName = value; }

        [SerializeField] private SpaceType _space = SpaceType.Local;
        [BrainPropertyShow(nameof(DirectionDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)] public SpaceType Space { get => _space; set => _space = value; }

        [SerializeField] private float _rangeMin = 1f;
        [SerializeField] private string _rangeMinName;
        [BrainPropertyShow(nameof(DirectionDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public float RangeMin { get => _rangeMin; set => _rangeMin = value; }
        [BrainPropertyValueName("RangeMin", typeof(IMonaVariablesFloatValue))] public string RangeMinName { get => _rangeMinName; set => _rangeMinName = value; }

        [SerializeField] private float _rangeMax = 100f;
        [SerializeField] private string _rangeMaxName;
        [BrainPropertyShow(nameof(DirectionDisplay), (int)UIDisplayType.Show)]
        [BrainProperty(false)] public float RangeMax { get => _rangeMax; set => _rangeMax = value; }
        [BrainPropertyValueName("RangeMax", typeof(IMonaVariablesFloatValue))] public string RangeMaxName { get => _rangeMaxName; set => _rangeMaxName = value; }

        [SerializeField] private float _rayHitOffset = 0f;
        [SerializeField] private string _rayHitOffsetName;
        [BrainPropertyShow(nameof(ValueType), (int)MultiBodyValueType.RayHitPosition)]
        [BrainProperty(false)] public float RayHitOffset { get => _rayHitOffset; set => _rayHitOffset = value; }
        [BrainPropertyValueName("RayHitOffset", typeof(IMonaVariablesFloatValue))] public string RayHitOffsetName { get => _rayHitOffsetName; set => _rayHitOffsetName = value; }

        [SerializeField] private bool _networkNewBodies;
        [SerializeField] private string _networkNewBodiesName;
        [BrainPropertyShow(nameof(OriginBody), (int)MonaBrainBroadcastTypeSingleTarget.Child)]
        [BrainPropertyShow(nameof(OriginBody), (int)MonaBrainBroadcastTypeSingleTarget.ChildWithName)]
        [BrainPropertyShow(nameof(OriginBody), (int)MonaBrainBroadcastTypeSingleTarget.ChildContainingName)]
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.Children)]
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(TargetBody), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(false)] public bool NetworkNewBodies { get => _networkNewBodies; set => _networkNewBodies = value; }
        [BrainPropertyValueName("NetworkNewBodies", typeof(IMonaVariablesBoolValue))] public string NetworkNewBodiesName { get => _networkNewBodiesName; set => _networkNewBodiesName = value; }

        public UIDisplayType CustomDirectionDisplay => DirectionDisplay == UIDisplayType.Show && Direction == BodyDirectionType.Custom ? UIDisplayType.Show : UIDisplayType.Hide;

        private const float _defaultDistance = -1f;
        private string _ignoreRaycastLayer = "Ignore Raycast";
        private IMonaBrain _brain;
        private LayerMask _checkLayerMask;
        private List<LayerMask> _bodyLayers = new List<LayerMask>();
        private List<IMonaBody> _targetBodies = new List<IMonaBody>();

        public UIDisplayType DirectionDisplay
        {
            get
            {
                switch (_valueType)
                {
                    case MultiBodyValueType.Distance:
                        return _distanceType == BodyDistanceType.FromRaycast ?
                            UIDisplayType.Show : UIDisplayType.Hide;

                    case MultiBodyValueType.DotProduct:
                    case MultiBodyValueType.RayHitPosition:
                    case MultiBodyValueType.RayHitNormal:
                    case MultiBodyValueType.HitNormalAlignAngle:
                        return UIDisplayType.Show;

                    case MultiBodyValueType.Direction:
                    default:
                        return UIDisplayType.Hide;
                }
            }
        }

        public TargetVariableType TrueTargetType
        {
            get
            {
                switch (ValueType)
                {
                    case MultiBodyValueType.Distance:
                    case MultiBodyValueType.DotProduct:
                        return TargetVariableType.Number;
                }

                return _targetType;
            }
        }

        public UIDisplayType AxisDisplay
        {
            get
            {
                if (_valueType == MultiBodyValueType.Direction || _valueType == MultiBodyValueType.RayHitPosition || _valueType == MultiBodyValueType.RayHitNormal || _valueType == MultiBodyValueType.HitNormalAlignAngle)
                {
                    if (TargetType == TargetVariableType.Number)
                        return UIDisplayType.Show;
                    else if (TargetType == TargetVariableType.String && CopyType == StringCopyType.SingleAxis)
                        return UIDisplayType.Show;
                }

                return UIDisplayType.Hide;
            }
        }

        private Vector3 TrueDirection
        {
            get
            {
                switch (_direction)
                {
                    case BodyDirectionType.Forward:
                        return Vector3.forward;
                    case BodyDirectionType.Back:
                        return Vector3.back;
                    case BodyDirectionType.Left:
                        return Vector3.left;
                    case BodyDirectionType.Right:
                        return Vector3.right;
                    case BodyDirectionType.Up:
                        return Vector3.up;
                    case BodyDirectionType.Down:
                        return Vector3.down;
                    default:
                        return _customDirection;
                }
            }
        }

        public enum TargetVariableType
        {
            Vector3 = 0,
            Number = 10,
            String = 20
        }

        public enum StringCopyType
        {
            Vector3 = 0,
            SingleAxis = 10
        }

        public enum UIDisplayType
        {
            Show = 0,
            Hide = 10
        }

        public enum DirectionReturnType
        {
            Vector = 0,
            Angles = 10
        }

        public enum BodyDistanceType
        {
            CompareTargets = 0,
            FromRaycast = 10
        }

        public GetMultiBodyValueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            int ignoreRaycastLayer = LayerMask.NameToLayer(_ignoreRaycastLayer);
            _checkLayerMask = ~(1 << ignoreRaycastLayer);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_originChildName))
                _originChild = _brain.Variables.GetString(_originChildName);

            if (!string.IsNullOrEmpty(_targetChildName))
                _targetChild = _brain.Variables.GetString(_targetChildName);

            if (!string.IsNullOrEmpty(_networkNewBodiesName))
                _networkNewBodies = _brain.Variables.GetBool(_networkNewBodiesName);

            if (HasVector3Values(_customDirectionName))
                _customDirection = GetVector3Value(_brain, _customDirectionName);

            if (!string.IsNullOrEmpty(_rangeMinName))
                _rangeMin = _brain.Variables.GetFloat(_rangeMinName);

            if (!string.IsNullOrEmpty(_rangeMaxName))
                _rangeMax = _brain.Variables.GetFloat(_rangeMaxName);

            if (!string.IsNullOrEmpty(_rayHitOffsetName))
                _rayHitOffset = _brain.Variables.GetFloat(_rayHitOffsetName);

            IMonaBody bodyA = GetOriginBody();

            if (bodyA == null)
                Complete(InstructionTileResult.Success);

            IMonaBody bodyB = GetClosestTargetBody(bodyA);

            if (bodyB == null)
            {
                if (_valueType == MultiBodyValueType.Distance)
                    SetVariable(_defaultDistance);

                return Complete(InstructionTileResult.Success);
            }

            switch (_valueType)
            {
                case MultiBodyValueType.Distance:
                    TryRayHit(bodyA, bodyB, out float distance, out _, out _);
                    SetVariable(distance);
                    break;
                case MultiBodyValueType.Direction:
                    SetVariable(GetDirection(bodyA, bodyB)); break;
                case MultiBodyValueType.DotProduct:
                    SetVariable(GetDotProduct(bodyA, bodyB)); break;
                case MultiBodyValueType.RayHitPosition:
                    if (TryRayHit(bodyA, bodyB, out _, out Vector3 hitPosition, out _))
                        SetVariable(hitPosition);
                    break;
                case MultiBodyValueType.RayHitNormal:
                    if (TryRayHit(bodyA, bodyB, out _, out _, out Vector3 hitNormal))
                        SetVariable(hitNormal);
                    break;
                case MultiBodyValueType.HitNormalAlignAngle:
                    if (TryRayHit(bodyA, bodyB, out _, out _, out Vector3 objectBNormal))
                    {
                        Vector3 rotationAngle = (Quaternion.FromToRotation(bodyA.Transform.up, objectBNormal) * bodyA.GetRotation()).eulerAngles;
                        SetVariable(rotationAngle);
                    }
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private bool TryRayHit(IMonaBody bodyA, IMonaBody bodyB, out float distance, out Vector3 hitPosition, out Vector3 hitNormal)
        {
            distance = _defaultDistance;
            hitPosition = hitNormal = Vector3.zero;

            if (_valueType == MultiBodyValueType.Distance && _distanceType == BodyDistanceType.CompareTargets)
            {
                distance = Vector3.Distance(bodyB.GetPosition(), bodyA.GetPosition());
                return true;
            }

            _bodyLayers.Clear();
            SetOriginalBodyLayers(bodyA);
            SetBodyLayer(bodyA, LayerMask.NameToLayer(_ignoreRaycastLayer));

            Vector3 direction = _space == SpaceType.Local ?
                bodyA.Transform.TransformDirection(TrueDirection) :
                TrueDirection;

            Ray ray = new Ray(bodyA.GetPosition(), direction);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit, _rangeMax, _checkLayerMask))
            {
                IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                ResetOriginalBodyLayers(bodyA);

                if (hitBody == null || hit.distance < _rangeMin)
                    return false;

                if (!HitTargetIsTargetBody(bodyA, bodyB, hitBody))
                    return false;

                distance = hit.distance;
                hitPosition = _valueType == MultiBodyValueType.RayHitPosition ?
                    hit.point + (direction.normalized * _rayHitOffset * -1f) :
                    hit.point;
                hitNormal = hit.normal;

                return true;
            }

            ResetOriginalBodyLayers(bodyA);
            return false;
        }

        private bool HitTargetIsTargetBody(IMonaBody originBody, IMonaBody preRegisteredTargetBody, IMonaBody hitBody)
        {
            _targetBodies.Clear();

            switch (_targetBody)
            {
                case MonaBrainBroadcastType.Tag:
                    return hitBody.HasMonaTag(_targetTag);
                case MonaBrainBroadcastType.Parents:
                    IMonaBody topBody = originBody;
                    while (topBody.Parent != null)
                    {
                        topBody = topBody.Parent;
                        _targetBodies.Add(topBody);
                    }
                    return _targetBodies.Contains(hitBody);
                case MonaBrainBroadcastType.Children:
                    _targetBodies = originBody.Children();
                    return _targetBodies.Contains(hitBody);
                case MonaBrainBroadcastType.ChildrenWithName:
                    _targetBodies = GetChildrenWithName(originBody, _targetChild);
                    return _targetBodies.Contains(hitBody);
                case MonaBrainBroadcastType.ChildrenContainingName:
                    _targetBodies = GetChildrenContainingName(originBody, _targetChild);
                    return _targetBodies.Contains(hitBody);
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    _targetBodies = _brain.SpawnedBodies;
                    return _targetBodies.Contains(hitBody);
                case MonaBrainBroadcastType.MyBodyArray:
                    _targetBodies = _brain.Variables.GetBodyArray(_bodyArray);
                    return _targetBodies.Contains(hitBody);
            }

            return hitBody == preRegisteredTargetBody;
        }

        private List<Transform> _rawChildren = new List<Transform>();
        private List<IMonaBody> _children = new List<IMonaBody>();

        private List<IMonaBody> GetChildren(IMonaBody body)
        {
            _rawChildren.Clear();
            _rawChildren.AddRange(body.Transform.GetComponentsInChildren<Transform>(true));
            _rawChildren.Remove(body.Transform);
            _children.Clear();

            _rawChildren.Clear();
            _rawChildren.AddRange(body.Transform.GetComponentsInChildren<Transform>(true));
            _rawChildren.Remove(body.Transform);

            _children.Clear();
            for (int i = 0; i < _rawChildren.Count; i++)
            {
                var child = _rawChildren[i];
                if (child == null)
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if (childBody == null)
                    childBody = CreateMonaBody(child);

                _children.Add(childBody);
            }
            return _children;
        }

        private List<IMonaBody> GetChildrenWithName(IMonaBody body, string nameToFind)
        {
            _rawChildren.Clear();
            _rawChildren.AddRange(body.Transform.GetComponentsInChildren<Transform>(true));
            _rawChildren.Remove(body.Transform);

            _children.Clear();
            for (int i = 0; i < _rawChildren.Count; i++)
            {
                var child = _rawChildren[i];
                if (child == null || child.name.ToLower() != nameToFind.ToLower())
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if (childBody == null)
                    childBody = CreateMonaBody(child);

                _children.Add(childBody);
            }
            return _children;
        }

        private List<IMonaBody> GetChildrenContainingName(IMonaBody body, string nameToFind)
        {
            _rawChildren.Clear();
            _rawChildren.AddRange(body.Transform.GetComponentsInChildren<Transform>(true));
            _rawChildren.Remove(body.Transform);

            _children.Clear();
            for (int i = 0; i < _rawChildren.Count; i++)
            {
                var child = _rawChildren[i];
                if (child == null || !child.name.ToLower().Contains(nameToFind.ToLower()))
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if (childBody == null)
                    childBody = CreateMonaBody(child);

                _children.Add(childBody);
            }
            return _children;
        }

        private IMonaBody CreateMonaBody(Transform body)
        {
            var childBody = body.gameObject.AddComponent<MonaBody>();
            childBody.SyncType = MonaBodyNetworkSyncType.NotNetworked;
            if (_networkNewBodies)
            {
                childBody.SyncType = MonaBodyNetworkSyncType.NetworkTransform;

                var guid = Guid.NewGuid();
                ((MonaBodyBase)childBody).Guid = new SerializableGuid(guid);
                ((MonaBodyBase)childBody).ManualMakeUnique(guid.ToString(), 0, 0, false);
            }
            return childBody;
        }

        private Vector3 GetDirection(IMonaBody bodyA, IMonaBody bodyB)
        {
            Vector3 directionVector = Vector3.Normalize(bodyB.GetPosition() - bodyA.GetPosition());

            if (_directionValue == DirectionReturnType.Vector)
                return directionVector;

            float yaw = Mathf.Atan2(directionVector.x, directionVector.z) * Mathf.Rad2Deg;
            float pitch = -Mathf.Asin(directionVector.y) * Mathf.Rad2Deg;

            return new Vector3(pitch, yaw, 0f);
        }

        private float GetDotProduct(IMonaBody bodyA, IMonaBody bodyB)
        {
            return Vector3.Dot(bodyA.Transform.forward, Vector3.Normalize(bodyB.GetPosition() - bodyA.GetPosition()));
        }

        private IMonaBody GetOriginBody()
        {
            switch (_originBody)
            {
                case MonaBrainBroadcastTypeSingleTarget.Tag:
                    return _brain.Body.GetClosestTag(_originTag);
                case MonaBrainBroadcastTypeSingleTarget.Self:
                    return _brain.Body;
                case MonaBrainBroadcastTypeSingleTarget.Parent:
                    return _brain.Body.Parent;
                case MonaBrainBroadcastTypeSingleTarget.Child:
                    var children = GetChildren(_brain.Body);
                    return children.Count > 0 ? children[0] : null;
                case MonaBrainBroadcastTypeSingleTarget.ChildWithName:
                    var childrenWithName = GetChildrenWithName(_brain.Body, _originChild);
                    return childrenWithName.Count > 0 ? childrenWithName[0] : null;
                case MonaBrainBroadcastTypeSingleTarget.ChildContainingName:
                    var childrenContainingName = GetChildrenContainingName(_brain.Body, _originChild);
                    return childrenContainingName.Count > 0 ? childrenContainingName[0] : null;
                case MonaBrainBroadcastTypeSingleTarget.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    return brain != null ? brain.Body : null;
                case MonaBrainBroadcastTypeSingleTarget.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainBroadcastTypeSingleTarget.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastTypeSingleTarget.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastTypeSingleTarget.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainBroadcastTypeSingleTarget.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastTypeSingleTarget.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
                case MonaBrainBroadcastTypeSingleTarget.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
                default:
                    return null;
            }
        }

        

        private IMonaBody GetClosestTargetBody(IMonaBody originBody)
        {
            switch (_targetBody)
            {
                case MonaBrainBroadcastType.Tag:
                    return originBody.GetClosestTag(_targetTag);
                case MonaBrainBroadcastType.Self:
                case MonaBrainBroadcastType.ThisBodyOnly:
                    return originBody;
                case MonaBrainBroadcastType.Parent:
                case MonaBrainBroadcastType.Parents:
                    return originBody.Parent;
                case MonaBrainBroadcastType.Children:
                    return ClosestBody(originBody, originBody.Children());
                case MonaBrainBroadcastType.ChildrenWithName:
                    return ClosestBody(originBody, GetChildrenWithName(originBody, _targetChild));
                case MonaBrainBroadcastType.ChildrenContainingName:
                    return ClosestBody(originBody, GetChildrenContainingName(originBody, _targetChild));
                case MonaBrainBroadcastType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    return brain != null ? brain.Body : null;
                case MonaBrainBroadcastType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainBroadcastType.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return originBody.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    return ClosestBody(originBody, _brain.SpawnedBodies);
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return originBody.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return originBody.PoolBodyNext;
                case MonaBrainBroadcastType.MyBodyArray:
                    return ClosestBody(originBody, _brain.Variables.GetBodyArray(_bodyArray));
                case MonaBrainBroadcastType.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
                default:
                    return null;
            }
        }

        private IMonaBody ClosestBody(IMonaBody originBody, List<IMonaBody> targetBodies)
        {
            if (targetBodies.Count < 1)
                return null;

            int closestIndex = 0;
            float closestDistance = float.PositiveInfinity;
            Vector3 originPosition = originBody.GetPosition();

            for (int i = 0; i < targetBodies.Count; i++)
            {
                float distance = Vector3.Distance(targetBodies[i].GetPosition(), originPosition);

                if (distance >= closestDistance)
                    continue;

                closestIndex = i;
                closestDistance = distance;
            }

            return targetBodies[closestIndex];
        }

        private void SetVariable(float result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetVector, result);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, result);
                    break;
                case TargetVariableType.String:
                    _brain.Variables.Set(_targetString, result.ToString());
                    break;
            }
        }

        private void SetVariable(Vector3 result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetVector, result);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, GetAxisValue(result));
                    break;
                case TargetVariableType.String:
                    if (_copyType == StringCopyType.Vector3)
                        _brain.Variables.Set(_targetString, result.ToString());
                    else
                        _brain.Variables.Set(_targetString, GetAxisValue(result).ToString());
                    break;
            }
        }

        private float GetAxisValue(Vector3 result)
        {
            switch (_axis)
            {
                case VectorThreeAxis.X:
                    return result.x;
                case VectorThreeAxis.Y:
                    return result.y;
                default:
                    return result.z;
            }
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
    }
}