using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnBodyInDirectionInstructionTile : InstructionTile, IInstructionTileWithPreload,
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "OnBodyInDirection";
        public const string NAME = "Body In Direction";
        public const string CATEGORY = "Vision";
        public override Type TileType => typeof(OnBodyInDirectionInstructionTile);

        [SerializeField] private MonaBrainBroadcastTypeSingleTarget _target = MonaBrainBroadcastTypeSingleTarget.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastTypeSingleTarget Target { get => _target; set => _target = value; }

        [SerializeField] private string _tag = "Default";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private string _nameToFind;
        [SerializeField] private string _nameToFindName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastTypeSingleTarget.ChildWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastTypeSingleTarget.ChildContainingName)]
        [BrainProperty(true)] public string NameToFind { get => _nameToFind; set => _nameToFind = value; }
        [BrainPropertyValueName("NameToFind", typeof(IMonaVariablesStringValue))] public string NameToFindName { get => _nameToFindName; set => _nameToFindName = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastTypeSingleTarget.FirstBodyInArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private BodyDirectionType _direction = BodyDirectionType.Forward;
        [BrainPropertyEnum(true)] public BodyDirectionType Direction { get => _direction; set => _direction = value; }

        [SerializeField] private Vector3 _customDirection = Vector3.forward;
        [SerializeField] private string[] _customDirectionName;
        [BrainPropertyShow(nameof(Direction), (int)BodyDirectionType.Custom)]
        [BrainProperty(true)] public Vector3 CustomDirection { get => _customDirection; set => _customDirection = value; }
        [BrainPropertyValueName("CustomDirection", typeof(IMonaVariablesVector3Value))] public string[] CustomDirectionName { get => _customDirectionName; set => _customDirectionName = value; }

        [SerializeField] private bool _negate;
        [SerializeField] private string _negateName;
        [BrainProperty(true)] public bool Negate { get => _negate; set => _negate = value; }
        [BrainPropertyValueName("Negate", typeof(IMonaVariablesBoolValue))] public string NegateName { get => _negateName; set => _negateName = value; }

        [SerializeField] private float _rangeMin = 1f;
        [SerializeField] private string _rangeMinName;
        [BrainProperty(false)] public float RangeMin { get => _rangeMin; set => _rangeMin = value; }
        [BrainPropertyValueName("RangeMin", typeof(IMonaVariablesFloatValue))] public string RangeMinName { get => _rangeMinName; set => _rangeMinName = value; }

        [SerializeField] private float _rangeMax = 100f;
        [SerializeField] private string _rangeMaxName;
        [BrainProperty(false)] public float RangeMax { get => _rangeMax; set => _rangeMax = value; }
        [BrainPropertyValueName("RangeMax", typeof(IMonaVariablesFloatValue))] public string RangeMaxName { get => _rangeMaxName; set => _rangeMaxName = value; }

        [SerializeField] private SpaceType _space = SpaceType.Local;
        [BrainPropertyEnum(false)] public SpaceType Space { get => _space; set => _space = value; }

        [SerializeField] private bool _networkNewBodies;
        [SerializeField] private string _networkNewBodiesName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastTypeSingleTarget.Child)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastTypeSingleTarget.ChildWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastTypeSingleTarget.ChildContainingName)]
        [BrainProperty(false)] public bool NetworkNewBodies { get => _networkNewBodies; set => _networkNewBodies = value; }
        [BrainPropertyValueName("NetworkNewBodies", typeof(IMonaVariablesBoolValue))] public string NetworkNewBodiesName { get => _networkNewBodiesName; set => _networkNewBodiesName = value; }

        private IMonaBrain _brain;
        private IMonaBody _trueTargetBody;
        private string _ignoreRaycastLayer = "Ignore Raycast";
        private LayerMask _checkLayerMask;
        private List<LayerMask> _bodyLayers = new List<LayerMask>();

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

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter, MonaTriggerType.OnTriggerExit, MonaTriggerType.OnFieldOfViewChanged };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnBodyInDirectionInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            int ignoreRaycastLayer = LayerMask.NameToLayer(_ignoreRaycastLayer);
            _checkLayerMask = ~(1 << ignoreRaycastLayer);
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_nameToFindName))
                _nameToFind = _brain.Variables.GetString(_nameToFindName);

            if (HasVector3Values(_customDirectionName))
                _customDirection = GetVector3Value(_brain, _customDirectionName);

            if (!string.IsNullOrEmpty(_rangeMinName))
                _rangeMin = _brain.Variables.GetFloat(_rangeMinName);

            if (!string.IsNullOrEmpty(_rangeMaxName))
                _rangeMax = _brain.Variables.GetFloat(_rangeMaxName);

            if (!string.IsNullOrEmpty(_negateName))
                _negate = _brain.Variables.GetBool(_negateName);

            if (!string.IsNullOrEmpty(_networkNewBodiesName))
                _networkNewBodies = _brain.Variables.GetBool(_networkNewBodiesName);

            if (_brain == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _trueTargetBody = GetBody();

            if (_trueTargetBody == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            bool result = _negate ? !TargetFoundInDirection(_brain.Body, _trueTargetBody) : TargetFoundInDirection(_brain.Body, _trueTargetBody);

            if (!result)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);

            _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, _trueTargetBody);
            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetBody()
        {
            switch (_target)
            {
                case MonaBrainBroadcastTypeSingleTarget.Tag:
                    return _brain.Body.GetClosestTag(_tag);
                case MonaBrainBroadcastTypeSingleTarget.Self:
                    return _brain.Body;
                case MonaBrainBroadcastTypeSingleTarget.Parent:
                    return _brain.Body.Parent;
                case MonaBrainBroadcastTypeSingleTarget.Child:
                    var children = GetChildren(_brain.Body);
                    return children.Count > 0 ? children[0] : null;
                case MonaBrainBroadcastTypeSingleTarget.ChildWithName:
                    var childrenWithName = GetChildrenWithName(_brain.Body, _nameToFind);
                    return childrenWithName.Count > 0 ? childrenWithName[0] : null;
                case MonaBrainBroadcastTypeSingleTarget.ChildContainingName:
                    var childrenContainingName = GetChildrenContainingName(_brain.Body, _nameToFind);
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
                case MonaBrainBroadcastTypeSingleTarget.FirstBodyInArray:
                    var arrayBodies = _brain.Variables.GetBodyArray(_bodyArray);
                    return arrayBodies.Count > 0 ? arrayBodies[0] : null;
                case MonaBrainBroadcastTypeSingleTarget.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
                default:
                    return null;
            }
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

        private bool TargetFoundInDirection(IMonaBody originBody, IMonaBody targetBody)
        {
            _bodyLayers.Clear();
            SetOriginalBodyLayers(originBody);
            SetBodyLayer(originBody, LayerMask.NameToLayer(_ignoreRaycastLayer));

            Vector3 direction = _space == SpaceType.Local ?
                originBody.Transform.TransformDirection(TrueDirection) :
                TrueDirection;

            Ray ray = new Ray(originBody.GetPosition(), direction);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit, _rangeMax, _checkLayerMask))
            {
                IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                ResetOriginalBodyLayers(originBody);

                if (hitBody == null || hit.distance < _rangeMin)
                    return false;

                _trueTargetBody = hitBody;

                if (_target == MonaBrainBroadcastTypeSingleTarget.Tag)
                    return hitBody.HasMonaTag(_tag);

                return hitBody == targetBody;
            }

            ResetOriginalBodyLayers(originBody);
            return false;
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