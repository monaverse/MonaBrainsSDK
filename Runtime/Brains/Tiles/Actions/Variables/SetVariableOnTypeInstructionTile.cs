using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using System.Collections.Generic;
using Unity.Profiling;
using Mona.SDK.Brains.Core.State;
using Unity.VisualScripting;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class SetVariableOnTypeInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "ModifyValue";
        public const string NAME = "Modify Value";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(SetVariableOnTypeInstructionTile);

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _targetChild;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(true)] public string TargetChildName { get => _targetChild; set => _targetChild = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private string _myVariable;
        [BrainPropertyShow(nameof(VariableType), (int)VariableUsageType.Any)]
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string MyVariable { get => _myVariable; set => _myVariable = value; }

        [SerializeField] private float _myNumber;
        [SerializeField] private string _myNumberName;
        [BrainPropertyShow(nameof(VariableType), (int)VariableUsageType.Number)]
        [BrainProperty(true)] public float MyNumber { get => _myNumber; set => _myNumber = value; }
        [BrainPropertyValueName("MyNumber", typeof(IMonaVariablesFloatValue))] public string MyNumberName { get => _myNumberName; set => _myNumberName = value; }

        [SerializeField] private string _myString;
        [SerializeField] private string _myStringName;
        [BrainPropertyShow(nameof(VariableType), (int)VariableUsageType.String)]
        [BrainProperty(true)] public string MyString { get => _myString; set => _myString = value; }
        [BrainPropertyValueName("MyString", typeof(IMonaVariablesStringValue))] public string MyStringName { get => _myStringName; set => _myStringName = value; }

        [SerializeField] private bool _myBool;
        [SerializeField] private string _myBoolName;
        [BrainPropertyShow(nameof(VariableType), (int)VariableUsageType.Boolean)]
        [BrainProperty(true)] public bool MyBool { get => _myBool; set => _myBool = value; }
        [BrainPropertyValueName("MyBool", typeof(IMonaVariablesBoolValue))] public string MyBoolName { get => _myBoolName; set => _myBoolName = value; }

        [SerializeField] private Vector2 _myVector2;
        [SerializeField] private string[] _myVector2Name;
        [BrainPropertyShow(nameof(VariableType), (int)VariableUsageType.Vector2)]
        [BrainProperty(true)] public Vector2 MyVector2 { get => _myVector2; set => _myVector2 = value; }
        [BrainPropertyValueName("MyVector2", typeof(IMonaVariablesVector2Value))] public string[] MyVector2Name { get => _myVector2Name; set => _myVector2Name = value; }

        [SerializeField] private Vector3 _myVector3;
        [SerializeField] private string[] _myVector3Name;
        [BrainPropertyShow(nameof(VariableType), (int)VariableUsageType.Vector3)]
        [BrainProperty(true)] public Vector3 MyVector3 { get => _myVector3; set => _myVector3 = value; }
        [BrainPropertyValueName("MyVector3", typeof(IMonaVariablesVector3Value))] public string[] MyVector3Name { get => _myVector3Name; set => _myVector3Name = value; }

        //[SerializeField] private ValueChangeType _operator = ValueChangeType.Set;
        //[BrainPropertyEnum(true)] public ValueChangeType Operator { get => _operator; set => _operator = value; }

        [SerializeField] private string _targetVariable;
        [SerializeField] private string _targetVariableName;
        [BrainProperty(true)] public string TargetVariable { get => _targetVariable; set => _targetVariable = value; }
        [BrainPropertyValueName("TargetVariable", typeof(IMonaVariablesStringValue))] public string TargetVariableName { get => _targetVariableName; set => _targetVariableName = value; }

        [SerializeField] private bool _includeAttached = true;
        [SerializeField] private string _includeAttachedName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

        [SerializeField] private bool _appendPlayerId;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainProperty(false)] public bool AddPlayerIdToTag { get => _appendPlayerId; set => _appendPlayerId = value; }

        [SerializeField] private VariableUsageType _variableType = VariableUsageType.Any;
        [BrainPropertyEnum(false)] public VariableUsageType VariableType { get => _variableType; set => _variableType = value; }

        [SerializeField] private bool _takeControl = false;
        [BrainPropertyEnum(false)] public bool TakeControl { get => _takeControl; set => _takeControl = value; }

        private IMonaBrain _brain;
        private Dictionary<IMonaBody, IMonaBrainRunner> _runnerCache = new Dictionary<IMonaBody, IMonaBrainRunner>();

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private float _timeout;

        private Dictionary<IMonaBrainVariables, WaitForControl> _waitingForControl = new Dictionary<IMonaBrainVariables, WaitForControl>();

        public SetVariableOnTypeInstructionTile() { }

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
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public void Preload(IMonaBrain brain) => _brain = brain;

        [Serializable]
        public enum VariableUsageType
        {
            Any = 0,
            Number = 10,
            String = 20,
            Boolean = 30,
            Vector2 = 40,
            Vector3 = 50
        }

        private bool ModifyAllAttached
        {
            get
            {
                switch (_target)
                {
                    case MonaBrainBroadcastType.Self:
                        return false;
                    case MonaBrainBroadcastType.Parent:
                        return false;
                    case MonaBrainBroadcastType.Parents:
                        return false;
                    case MonaBrainBroadcastType.Children:
                    case MonaBrainBroadcastType.ChildrenWithName:
                    case MonaBrainBroadcastType.ChildrenContainingName:
                        return false;
                    case MonaBrainBroadcastType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        private IMonaBrainRunner GetCachedRunner(IMonaBody body)
        {
            if (!_runnerCache.ContainsKey(body))
                _runnerCache.Add(body, body.Transform.GetComponent<IMonaBrainRunner>());
            return _runnerCache[body];
        }

        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(Do)}");

        public override InstructionTileResult Do()
        {
            //Debug.Log($"{nameof(SetVariableOnTypeInstructionTile)} {_myVariable}");

            if (_brain == null || (_variableType == VariableUsageType.Any && string.IsNullOrEmpty(_myVariable)))
            {
                Debug.Log($"{nameof(SetVariableOnTypeInstructionTile)} variable not found");
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
            }

            //_profilerDo.Begin();
            _waitingForControl.Clear();

            var myValue = _brain.Variables.GetVariable(_myVariable);

            if (!string.IsNullOrEmpty(_includeAttachedName))
                _includeAttached = _brain.Variables.GetBool(_includeAttachedName);

            if (!string.IsNullOrEmpty(_myNumberName))
                _myNumber = _brain.Variables.GetFloat(_myNumberName);

            if (!string.IsNullOrEmpty(_myStringName))
                _myString = _brain.Variables.GetString(_myStringName);

            if (!string.IsNullOrEmpty(_myBoolName))
                _myBool = _brain.Variables.GetBool(_myBoolName);

            if (HasVector2Values(_myVector2Name))
                _myVector2 = GetVector2Value(_brain, _myVector2Name);

            if (HasVector3Values(_myVector3Name))
                _myVector3 = GetVector3Value(_brain, _myVector3Name);

            if (!string.IsNullOrEmpty(_targetVariableName))
                _targetVariable = _brain.Variables.GetString(_targetVariableName);

            var result = InstructionTileResult.Success;
            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    result = ModifyOnTag(myValue);
                    break;
                case MonaBrainBroadcastType.Self:
                    result = ModifyOnWholeEntity(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    result = ModifyOnParents(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    result = ModifyOnChildren(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.ChildrenWithName:
                    result = ModifyOnChildrenWithName(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.ChildrenContainingName:
                    result = ModifyOnChildrenContainingName(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    result = ModifyValueOnBrains(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    result = ModifyOnAllSpawned(myValue);
                    break;
                case MonaBrainBroadcastType.MyBodyArray:
                    result = ModifyValueOnBodyArray(myValue, _brain);
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        result = ModifyOnWholeEntity(myValue, targetBody);
                    else
                        result = ModifyValueOnBrains(myValue, targetBody);

                    break;
            }

            if (result != InstructionTileResult.Success)
            {
                //Debug.Log($"{nameof(SetVariableOnTypeInstructionTile)} DO NOT HAVE CONTROL OF ALL VARIABLES waiting for control of #{_waitingForControl.Count}");
                _timeout = 1f;
                if (_waitingForControl.Count > 0)
                    AddEventDelegates();
                else
                    result = InstructionTileResult.Success;
            }

            //_profilerDo.End();
            return Complete(result);
        }

        private void AddEventDelegates()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveAllEventDelegates()
        {
            foreach(var pair in _waitingForControl)
                pair.Key.NetworkVariables.OnStateAuthorityChanged -= pair.Value.OnStateAuthorityChanged;
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveEventDelegates(IMonaBrainVariables brainVariables)
        {
            //Debug.Log($"{nameof(SetVariableOnTypeInstructionTile)} CONTROL CHANGED has control? {brainVariables.NetworkVariables.HasControl()}");
            brainVariables.NetworkVariables.OnStateAuthorityChanged -= _waitingForControl[brainVariables].OnStateAuthorityChanged;
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            _timeout -= evt.DeltaTime;
            if (_timeout < 0)
            {
                //Debug.Log($"Take control request timedout", _brain.Body.Transform.gameObject);
                RemoveAllEventDelegates();
                Complete(InstructionTileResult.Failure, true);
            }
        }

        private InstructionTileResult ModifyValueOnBodyArray(IMonaVariablesValue myValue, IMonaBrain brain)
        {
            var bodyArray = brain.Variables.GetBodyArray(_bodyArray);

            var result = InstructionTileResult.Success;
            for (var i = 0; i < bodyArray.Count; i++)
            {
                if (ModifyAllAttached)
                {
                    var r = ModifyOnWholeEntity(myValue, bodyArray[i]);
                    if (r != InstructionTileResult.Success)
                        result = r;
                }
                else
                {
                    var runner = GetCachedRunner(bodyArray[i]);
                    if (runner != null)
                    {
                        for (var j = 0; j < runner.BrainInstances.Count; j++)
                        {
                            InstructionTileResult r;
                            if (ModifyAllAttached)
                                r = ModifyOnWholeEntity(myValue, runner.BrainInstances[j].Body);
                            else
                                r = ModifyValueOnBrains(myValue, runner.BrainInstances[j].Body);
                            if (r != InstructionTileResult.Success)
                                result = r;
                        }
                    }
                }
            }
            return result;
        }

        static readonly ProfilerMarker _profileModifyOnTag = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyOnTag)}");

        private InstructionTileResult ModifyOnTag(IMonaVariablesValue myValue)
        {
            _profileModifyOnTag.Begin();
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
            {
                _profileModifyOnTag.End();
                return InstructionTileResult.Success;
            }

            var result = InstructionTileResult.Success;
            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                InstructionTileResult r;
                if (ModifyAllAttached)
                    r = ModifyOnWholeEntity(myValue, tagBodies[i]);
                else
                    r = ModifyValueOnBrains(myValue, tagBodies[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
            }
            _profileModifyOnTag.End();
            return result;
        }

        static readonly ProfilerMarker _profileModifyOnWholeEntity = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyOnWholeEntity)}");

        private InstructionTileResult ModifyOnWholeEntity(IMonaVariablesValue myValue, IMonaBody body)
        {
            _profileModifyOnWholeEntity.Begin();
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            var result = ModifyValueOnBrains(myValue, topBody);
            var r = ModifyOnChildren(myValue, topBody);

            if (r != InstructionTileResult.Success)
                result = r;

            _profileModifyOnWholeEntity.End();
            return result;
        }

        static readonly ProfilerMarker _profileModifyOnParents = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyOnParents)}");

        private InstructionTileResult ModifyOnParents(IMonaVariablesValue myValue, IMonaBody body)
        {
            _profileModifyOnParents.Begin();
            IMonaBody parent = body.Parent;

            if (parent == null)
            {
                _profileModifyOnParents.End();
                return InstructionTileResult.Success;
            }

            var result = ModifyValueOnBrains(myValue, parent);
            var r = ModifyOnParents(myValue, parent);
            if (r != InstructionTileResult.Success)
                result = r;
            _profileModifyOnParents.End();
            return result;
        }

        private InstructionTileResult ModifyOnChildren(IMonaVariablesValue myValue, IMonaBody body)
        {
        
            var children = body.Children();
            var result = InstructionTileResult.Success;

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                var r = ModifyValueOnBrains(myValue, children[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
                r = ModifyOnChildren(myValue, children[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
            }
            return result;
        }

        private InstructionTileResult ModifyOnChildrenWithName(IMonaVariablesValue myValue, IMonaBody body)
        {

            var children = body.Children();
            var result = InstructionTileResult.Success;

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null || children[i].Transform.name.ToLower() != _targetChild.ToLower())
                    continue;

                var r = ModifyValueOnBrains(myValue, children[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
                r = ModifyOnChildrenWithName(myValue, children[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
            }
            return result;
        }

        private InstructionTileResult ModifyOnChildrenContainingName(IMonaVariablesValue myValue, IMonaBody body)
        {

            var children = body.Children();
            var result = InstructionTileResult.Success;

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null || !children[i].Transform.name.ToLower().Contains(_targetChild.ToLower()))
                    continue;

                var r = ModifyValueOnBrains(myValue, children[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
                r = ModifyOnChildrenContainingName(myValue, children[i]);
                if (r != InstructionTileResult.Success)
                    result = r;
            }
            return result;
        }
        private InstructionTileResult ModifyOnAllSpawned(IMonaVariablesValue myValue)
        {        
            var spawned = _brain.SpawnedBodies;

            var result = InstructionTileResult.Success;
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                InstructionTileResult r;
                if (ModifyAllAttached)
                    r = ModifyOnWholeEntity(myValue, spawned[i]);
                else
                    r = ModifyValueOnBrains(myValue, spawned[i]);

                if (r != InstructionTileResult.Success)
                    result = r;
            }
            return result;
        }

        private IMonaBody GetTarget()
        {
            switch (_target)
            {
                case MonaBrainBroadcastType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainBroadcastType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainBroadcastType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainBroadcastType.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
                case MonaBrainBroadcastType.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
            }
            return null;
        }

        static readonly ProfilerMarker _profileModifyValueOnBrains = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyValueOnBrains)}");


        static readonly ProfilerMarker markerGet = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.Get");
        static readonly ProfilerMarker markerSet = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.Set");

        private struct WaitForControl
        {
            public Action OnStateAuthorityChanged;
        }

        private InstructionTileResult ModifyValueOnBrains(IMonaVariablesValue myValue, IMonaBody body)
        {
            if (body.ActiveTransform == null)
                return InstructionTileResult.Success;

            _profileModifyValueOnBrains.Begin();
            var runner = GetCachedRunner(body);

            if (runner == null)
            {
                _profileModifyValueOnBrains.End();
                return InstructionTileResult.Success;
            }

            var result = InstructionTileResult.Success;
            for (int i = 0; i < runner.BrainInstances.Count; i++)
            {
                var brainVariables = runner.BrainInstances[i].Variables;

                if (brainVariables == null)
                    continue;

                var shouldSetValue = !_takeControl || brainVariables.NetworkVariables == null || (_takeControl && body.HasControl());
                var shouldTakeControl = _takeControl && !body.HasControl();

                switch (_variableType)
                {
                    case VariableUsageType.Number:
                        if(shouldSetValue)
                            brainVariables.Set(_targetVariable, _myNumber, true, false);
                        else if(shouldTakeControl)
                        {
                            result = InstructionTileResult.Running;
                            var wait = new WaitForControl()
                            {
                                OnStateAuthorityChanged = () =>
                                {
                                    RemoveEventDelegates(brainVariables);
                                    _waitingForControl.Remove(brainVariables);
                                    if (brainVariables.NetworkVariables.HasControl())
                                        brainVariables.Set(_targetVariable, _myNumber, true, false);
                                }
                            };
                            _waitingForControl.Add(brainVariables, wait);
                            brainVariables.NetworkVariables.OnStateAuthorityChanged += wait.OnStateAuthorityChanged;
                            body.TakeControl();
                        }
                        break;
                    case VariableUsageType.String:
                        if (shouldSetValue)
                            brainVariables.Set(_targetVariable, _myString, true, false);
                        else if (shouldTakeControl)
                        {
                            result = InstructionTileResult.Running;
                            var wait = new WaitForControl()
                            {
                                OnStateAuthorityChanged = () =>
                                {
                                    RemoveEventDelegates(brainVariables);
                                    _waitingForControl.Remove(brainVariables);
                                    if (brainVariables.NetworkVariables.HasControl())
                                        brainVariables.Set(_targetVariable, _myString, true, false);
                                }
                            };
                            _waitingForControl.Add(brainVariables, wait);
                            brainVariables.NetworkVariables.OnStateAuthorityChanged += wait.OnStateAuthorityChanged;
                            body.TakeControl();
                        }
                        break;
                    case VariableUsageType.Boolean:
                        if (shouldSetValue)
                            brainVariables.Set(_targetVariable, _myBool, true, false);
                        else if (shouldTakeControl)
                        {
                            result = InstructionTileResult.Running;
                            var wait = new WaitForControl()
                            {
                                OnStateAuthorityChanged = () =>
                                {
                                    RemoveEventDelegates(brainVariables);
                                    _waitingForControl.Remove(brainVariables);
                                    if (brainVariables.NetworkVariables.HasControl())
                                        brainVariables.Set(_targetVariable, _myBool, true, false);
                                }
                            };
                            brainVariables.NetworkVariables.OnStateAuthorityChanged += wait.OnStateAuthorityChanged;
                            body.TakeControl();
                        }
                        break;
                    case VariableUsageType.Vector2:
                        if (shouldSetValue)
                            brainVariables.Set(_targetVariable, _myVector2, true, false);
                        else if (shouldTakeControl)
                        {
                            result = InstructionTileResult.Running;
                            var wait = new WaitForControl()
                            {
                                OnStateAuthorityChanged = () =>
                                {
                                    RemoveEventDelegates(brainVariables);
                                    _waitingForControl.Remove(brainVariables);
                                    if (brainVariables.NetworkVariables.HasControl())
                                        brainVariables.Set(_targetVariable, _myVector2, true, false);
                                }
                            };
                            brainVariables.NetworkVariables.OnStateAuthorityChanged += wait.OnStateAuthorityChanged;
                            body.TakeControl();
                        }
                        break;
                    case VariableUsageType.Vector3:
                        if (shouldSetValue)
                            brainVariables.Set(_targetVariable, _myVector3, true, false);
                        else if (shouldTakeControl)
                        {
                            result = InstructionTileResult.Running;
                            var wait = new WaitForControl()
                            {
                                OnStateAuthorityChanged = () =>
                                {
                                    RemoveEventDelegates(brainVariables);
                                    _waitingForControl.Remove(brainVariables);
                                    if (brainVariables.NetworkVariables.HasControl())
                                        brainVariables.Set(_targetVariable, _myVector3, true, false);
                                }
                            };
                            brainVariables.NetworkVariables.OnStateAuthorityChanged += wait.OnStateAuthorityChanged;
                            body.TakeControl();
                        }
                        break;
                    default:
                        markerGet.Begin();
                        var targetValue = brainVariables.GetVariable(_targetVariable);
                        if (targetValue == null)
                        {
                            markerGet.End();
                            continue;
                        }
                        markerGet.End();

                        markerSet.Begin();

                        if (shouldSetValue)
                        {
                            if (targetValue is IMonaVariablesStringValue)
                                brainVariables.Set(_targetVariable, _brain.Variables.GetValueAsString(_myVariable), true, false);
                            else if (targetValue is IMonaVariablesFloatValue && myValue is IMonaVariablesFloatValue)
                                brainVariables.Set(_targetVariable, _brain.Variables.GetFloat(_myVariable), true, false);
                            else if (targetValue is IMonaVariablesBoolValue && myValue is IMonaVariablesBoolValue)
                                brainVariables.Set(_targetVariable, _brain.Variables.GetBool(_myVariable), true, false);
                            else if (targetValue is IMonaVariablesVector2Value && myValue is IMonaVariablesVector2Value)
                                brainVariables.Set(_targetVariable, _brain.Variables.GetVector2(_myVariable), true, false);
                            else if (targetValue is IMonaVariablesVector3Value && myValue is IMonaVariablesVector3Value)
                                brainVariables.Set(_targetVariable, _brain.Variables.GetVector3(_myVariable), true, false);
                            else if (targetValue is IMonaVariablesBodyArrayValue && myValue is IMonaVariablesBodyArrayValue)
                            {
                                var list = new List<IMonaBody>();
                                list.AddRange(_brain.Variables.GetBodyArray(_myVariable));
                                brainVariables.Set(_targetVariable, list, false);
                            }
                        }
                        else if (shouldTakeControl)
                        {
                            result = InstructionTileResult.Running;
                            var wait = new WaitForControl()
                            {
                                OnStateAuthorityChanged = () =>
                                {
                                    RemoveEventDelegates(brainVariables);
                                    _waitingForControl.Remove(brainVariables);
                                    if (brainVariables.NetworkVariables.HasControl())
                                    {
                                        if (targetValue is IMonaVariablesStringValue)
                                            brainVariables.Set(_targetVariable, _brain.Variables.GetValueAsString(_myVariable), true, false);
                                        else if (targetValue is IMonaVariablesFloatValue && myValue is IMonaVariablesFloatValue)
                                            brainVariables.Set(_targetVariable, _brain.Variables.GetFloat(_myVariable), true, false);
                                        else if (targetValue is IMonaVariablesBoolValue && myValue is IMonaVariablesBoolValue)
                                            brainVariables.Set(_targetVariable, _brain.Variables.GetBool(_myVariable), true, false);
                                        else if (targetValue is IMonaVariablesVector2Value && myValue is IMonaVariablesVector2Value)
                                            brainVariables.Set(_targetVariable, _brain.Variables.GetVector2(_myVariable), true, false);
                                        else if (targetValue is IMonaVariablesVector3Value && myValue is IMonaVariablesVector3Value)
                                            brainVariables.Set(_targetVariable, _brain.Variables.GetVector3(_myVariable), true, false);
                                        else if (targetValue is IMonaVariablesBodyArrayValue && myValue is IMonaVariablesBodyArrayValue)
                                        {
                                            var list = new List<IMonaBody>();
                                            list.AddRange(_brain.Variables.GetBodyArray(_myVariable));
                                            brainVariables.Set(_targetVariable, list, false);
                                        }
                                    }
                                }
                            };
                            _waitingForControl.Add(brainVariables, wait);
                            brainVariables.NetworkVariables.OnStateAuthorityChanged += wait.OnStateAuthorityChanged;
                            body.TakeControl();
                        }

                        markerSet.End();
                        break;
                }
            }
            _profileModifyValueOnBrains.End();
            return result;
        }
    }
}