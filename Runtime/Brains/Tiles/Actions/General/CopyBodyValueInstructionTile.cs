using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System.Text;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CopyBodyValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "CopyBodyValue";
        public const string NAME = "Copy Body Value";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CopyBodyValueInstructionTile);

        [SerializeField] private MonaBodyValueType _source = MonaBodyValueType.Position;
        [BrainPropertyEnum(true)] public MonaBodyValueType Source { get => _source; set => _source = value; }

        [SerializeField] private BodyDirectionType _direction = BodyDirectionType.Forward;
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Direction)]
        [BrainPropertyEnum(true)] public BodyDirectionType Direction { get => _direction; set => _direction = value; }

        [SerializeField] private Vector3 _customDirection = Vector3.forward;
        [SerializeField] private string[] _customDirectionName;
        [BrainPropertyShow(nameof(DisplayCustomDirection), (int)UIDisplayType.Show)]
        [BrainProperty(true)] public Vector3 CustomDirection { get => _customDirection; set => _customDirection = value; }
        [BrainPropertyValueName("CustomDirection", typeof(IMonaVariablesVector3Value))] public string[] CustomDirectionName { get => _customDirectionName; set => _customDirectionName = value; }

        [SerializeField] private MonaBrainBroadcastTypeSingleTarget _body = MonaBrainBroadcastTypeSingleTarget.Self;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastTypeSingleTarget Body { get => _body; set => _body = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(Body), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private string _nameToFind;
        [SerializeField] private string _nameToFindName;
        [BrainPropertyShow(nameof(Body), (int)MonaBrainBroadcastTypeSingleTarget.ChildWithName)]
        [BrainPropertyShow(nameof(Body), (int)MonaBrainBroadcastTypeSingleTarget.ChildContainingName)]
        [BrainProperty(true)] public string NameToFind { get => _nameToFind; set => _nameToFind = value; }
        [BrainPropertyValueName("NameToFind", typeof(IMonaVariablesStringValue))] public string NameToFindName { get => _nameToFindName; set => _nameToFindName = value; }

        [SerializeField] private TargetVariableType _targetType;
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Velocity)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.StartScale)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.StartRotation)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.StartPosition)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Scale)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Rotation)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Position)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Direction)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.ChildIndex)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.SiblingIndex)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.BoundsExtents)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.BoundsCenter)]
        [BrainPropertyEnum(false)] public TargetVariableType TargetType { get => _targetType; set => _targetType = value; }

        [SerializeField] string _targetValue;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Vector3)]
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        [SerializeField] private string _targetNumber;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Number)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string TargetNumber { get => _targetNumber; set => _targetNumber = value; }

        [SerializeField] private string _targetString;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.String)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string TargetString { get => _targetString; set => _targetString = value; }

        [SerializeField] private string _targetBool;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Bool)]
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), true)] public string TargetBool { get => _targetBool; set => _targetBool = value; }

        [SerializeField] private StringCopyType _copyType;
        [BrainPropertyShow(nameof(CopyTypeDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(false)]
        public StringCopyType CopyType { get => _copyType; set => _copyType = value; }

        [SerializeField] private VectorThreeAxis _axis = VectorThreeAxis.Y;
        [BrainPropertyShow(nameof(AxisDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(true)]
        public VectorThreeAxis Axis { get => _axis; set => _axis = value; }

        [SerializeField] private bool _includeChildren = true;
        [SerializeField] private string _includeChildrenName;
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.BoundsExtents)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.BoundsCenter)]
        [BrainProperty(false)] public bool IncludeChildren { get => _includeChildren; set => _includeChildren = value; }
        [BrainPropertyValueName("IncludeChildren", typeof(IMonaVariablesBoolValue))] public string IncludeChildrenName { get => _includeChildrenName; set => _includeChildrenName = value; }

        [SerializeField] private Vector3 _offset;
        [SerializeField] private string[] _offsetName;
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Position)]
        [BrainProperty(false)] public Vector3 Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesVector3Value))] public string[] MyVector3Name { get => _offsetName; set => _offsetName = value; }

        [SerializeField] private bool _networkNewBodies;
        [SerializeField] private string _networkNewBodiesName;
        [BrainPropertyShow(nameof(Body), (int)MonaBrainBroadcastTypeSingleTarget.Child)]
        [BrainPropertyShow(nameof(Body), (int)MonaBrainBroadcastTypeSingleTarget.ChildWithName)]
        [BrainPropertyShow(nameof(Body), (int)MonaBrainBroadcastTypeSingleTarget.ChildContainingName)]
        [BrainProperty(false)] public bool NetworkNewBodies { get => _networkNewBodies; set => _networkNewBodies = value; }
        [BrainPropertyValueName("NetworkNewBodies", typeof(IMonaVariablesBoolValue))] public string NetworkNewBodiesName { get => _networkNewBodiesName; set => _networkNewBodiesName = value; }

        public bool SourceIsStringOnly => Source == MonaBodyValueType.PlayerName || Source == MonaBodyValueType.ReadMe || Source == MonaBodyValueType.BodyName;
        public bool SourceNumberIsSingleNumber => Source == MonaBodyValueType.ChildIndex || Source == MonaBodyValueType.ChildCount || Source == MonaBodyValueType.SiblingIndex || Source == MonaBodyValueType.Velocity;
        public UIDisplayType DisplayCustomDirection => Source == MonaBodyValueType.Direction && Direction == BodyDirectionType.Custom ? UIDisplayType.Show : UIDisplayType.Hide;

        public TargetVariableType TrueTargetType
        {
            get
            {
                switch (Source)
                {
                    case MonaBodyValueType.ChildCount:
                    case MonaBodyValueType.PlayerId:
                    case MonaBodyValueType.ClientId:
                        return TargetVariableType.Number;
                    case MonaBodyValueType.PlayerName:
                    case MonaBodyValueType.ReadMe:
                    case MonaBodyValueType.BodyName:
                        return TargetVariableType.String;
                    case MonaBodyValueType.IsLocalPlayer:
                        return TargetVariableType.Bool;
                }

                return _targetType;
            }
        }

        public UIDisplayType CopyTypeDisplay
        {
            get
            {
                if (SourceIsStringOnly)
                    return UIDisplayType.Hide;

                return TrueTargetType == TargetVariableType.String ? UIDisplayType.Show : UIDisplayType.Hide;
            }
        }

        public UIDisplayType AxisDisplay
        {
            get
            {
                if (SourceIsStringOnly || SourceNumberIsSingleNumber)
                    return UIDisplayType.Hide;
                else if (TargetType == TargetVariableType.Number)
                    return Source != MonaBodyValueType.Velocity ? UIDisplayType.Show : UIDisplayType.Hide;
                else if (TargetType == TargetVariableType.String && CopyType == StringCopyType.SingleAxis)
                    return UIDisplayType.Show;

                return UIDisplayType.Hide;
            }
        }

        public enum TargetVariableType
        {
            Vector3 = 0,
            Number = 10,
            String = 20,
            Bool = 30
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

        private IMonaBrain _brain;

        public CopyBodyValueInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (HasVector3Values(_customDirectionName))
                _customDirection = GetVector3Value(_brain, _customDirectionName);

            if (HasVector3Values(_offsetName))
                _offset = GetVector3Value(_brain, _offsetName);

            if (!string.IsNullOrEmpty(_nameToFindName))
                _nameToFind = _brain.Variables.GetString(_nameToFindName);

            if (!string.IsNullOrEmpty(_includeChildrenName))
                _includeChildren = _brain.Variables.GetBool(_includeChildrenName);

            if (!string.IsNullOrEmpty(_networkNewBodiesName))
                _networkNewBodies = _brain.Variables.GetBool(_networkNewBodiesName);

            IMonaBody body = GetBody();

            if (body == null)
                return Complete(InstructionTileResult.Success);

            switch (_source)
            {
                case MonaBodyValueType.StartPosition:
                    SetVariable(body.InitialPosition); break;
                case MonaBodyValueType.Rotation:
                    SetVariable(body.GetRotation().eulerAngles); break;
                case MonaBodyValueType.StartRotation:
                    SetVariable(body.InitialRotation.eulerAngles); break;
                case MonaBodyValueType.Scale:
                    SetVariable(body.GetScale()); break;
                case MonaBodyValueType.StartScale:
                    SetVariable(body.InitialScale); break;
                case MonaBodyValueType.Velocity:
                    SetVelocity(); break;
                case MonaBodyValueType.Direction:
                    SetVariable(GetBodyDirection(body)); break;
                case MonaBodyValueType.ChildIndex:
                    SetVariable((float)body.ChildIndex); break;
                case MonaBodyValueType.SiblingIndex:
                    SetVariable((float)body.ActiveTransform.GetSiblingIndex()); break;
                case MonaBodyValueType.ChildCount:
                    SetVariable((float)body.Transform.childCount); break;
                case MonaBodyValueType.PlayerId:
                    //Debug.Log($"{nameof(CopyBodyValueInstructionTile)} playerId {body.PlayerId} {body.ClientId} {body.PlayerName} ", body.Transform.gameObject);
                    SetVariable((float)body.PlayerId); break;
                case MonaBodyValueType.ClientId:
                    SetVariable((float)body.ClientId); break;
                case MonaBodyValueType.PlayerName:
                    SetVariable(body.PlayerName); break;
                case MonaBodyValueType.IsLocalPlayer:
                    SetVariable(body.IsAttachedToLocalPlayer()); break;
                case MonaBodyValueType.ReadMe:
                    var runner = body.Transform.GetComponent<IMonaBrainRunner>();
                    if (runner != null)
                    {
                        var readme = new StringBuilder();
                        for (var i = 0; i < runner.BrainInstances.Count; i++)
                        {
                            if (i > 0 && !string.IsNullOrEmpty(runner.BrainInstances[i].ReadMe.Replace(" ", ""))) readme.Append("\n");
                            readme.Append(runner.BrainInstances[i].ReadMe);
                        }
                        SetVariable(readme.ToString());
                    }
                    break;
                case MonaBodyValueType.BodyName:
                    SetVariable(body.Transform.gameObject.name); break;
                case MonaBodyValueType.BoundsExtents:
                    SetBoundsValue(body); break;
                case MonaBodyValueType.BoundsCenter:
                    SetBoundsValue(body); break;
                default:
                    SetVariable(body.GetPosition(_offset)); break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetBody()
        {
            switch (_body)
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

        private Vector3 GetBodyDirection(IMonaBody body)
        {
            switch (_direction)
            {
                case BodyDirectionType.Forward:
                    return body.ActiveTransform.forward;
                case BodyDirectionType.Back:
                    return body.ActiveTransform.forward * -1f;
                case BodyDirectionType.Right:
                    return body.ActiveTransform.right;
                case BodyDirectionType.Left:
                    return body.ActiveTransform.right * -1f;
                case BodyDirectionType.Up:
                    return body.ActiveTransform.up;
                case BodyDirectionType.Down:
                    return body.ActiveTransform.up * -1f;
                case BodyDirectionType.Custom:
                    return body.ActiveTransform.TransformDirection(_customDirection);
            }

            return Vector3.forward;
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

        private void SetVariable(bool result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetValue, result ? Vector3.one : Vector3.zero);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, result ? 1 : 0);
                    break;
                case TargetVariableType.String:
                    _brain.Variables.Set(_targetString, result ? "True" : "False");
                    break;
                case TargetVariableType.Bool:
                    _brain.Variables.Set(_targetBool, result);
                    break;
            }
        }

        private void SetVariable(float result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetValue, result);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, result);
                    break;
                case TargetVariableType.String:
                    _brain.Variables.Set(_targetString, result.ToString());
                    break;
                case TargetVariableType.Bool:
                    _brain.Variables.Set(_targetBool, result > 0f ? true : false);
                    break;
            }
        }

        private void SetVariable(Vector3 result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetValue, result);
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
                case TargetVariableType.Bool:
                    _brain.Variables.Set(_targetBool, result.magnitude > Mathf.Epsilon ? true : false);
                    break;
            }
        }

        private void SetBoundsValue(IMonaBody body)
        {
            Transform parent = body.Transform.parent != null ? body.Transform.parent : body.Transform;
            Bounds bounds = new Bounds(parent.position, Vector3.zero);

            if (_includeChildren)
            {
                Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].GetComponent<LineRenderer>() == null)
                        bounds.Encapsulate(renderers[i].bounds);
                }
            }
            else
            {
                Renderer renderer = parent.GetComponent<Renderer>();

                if (renderer != null)
                    bounds.Encapsulate(renderer.bounds);
            }

            Vector3 result = _source == MonaBodyValueType.BoundsExtents ?
                bounds.extents * 2f : bounds.center;

            SetVariable(result);
        }

        private void SetVariable(string result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.String:
                    _brain.Variables.Set(_targetString, result);
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

        private void SetVelocity()
        {
            Vector3 velocity = _brain.Body.GetVelocity();
            float velocityMagnitude = velocity.magnitude;

            switch (_targetType)
            {
                case TargetVariableType.Vector3:
                    SetVariable(velocity);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, velocityMagnitude);
                    break;
                case TargetVariableType.String:
                    if (_copyType == StringCopyType.Vector3)
                        _brain.Variables.Set(_targetString, velocity.ToString());
                    else
                        _brain.Variables.Set(_targetString, velocityMagnitude.ToString());
                    break;
            }
        }
    }
}