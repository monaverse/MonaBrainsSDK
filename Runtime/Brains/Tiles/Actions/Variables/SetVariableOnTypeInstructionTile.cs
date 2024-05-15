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
        [BrainProperty(true)] public string TargetVariable { get => _targetVariable; set => _targetVariable = value; }

        [SerializeField] private bool _includeAttached = true;
        [SerializeField] private string _includeAttachedName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnHitTarget)]
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

        private IMonaBrain _brain;
        private Dictionary<IMonaBody, IMonaBrainRunner> _runnerCache = new Dictionary<IMonaBody, IMonaBrainRunner>();

        public SetVariableOnTypeInstructionTile() { }

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

        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(Do)}");

        public override InstructionTileResult Do()
        {
            if (_brain == null || (_variableType == VariableUsageType.Any && string.IsNullOrEmpty(_myVariable)))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _profilerDo.Begin();

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
                _myVector2 = GetVector3Value(_brain, _myVector2Name);

            if (HasVector3Values(_myVector3Name))
                _myVector3 = GetVector3Value(_brain, _myVector3Name);

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    ModifyOnTag(myValue);
                    break;
                case MonaBrainBroadcastType.Self:
                    ModifyOnWholeEntity(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    ModifyOnParents(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    ModifyOnChildren(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    ModifyValueOnBrains(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    ModifyOnAllSpawned(myValue);
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        ModifyOnWholeEntity(myValue, targetBody);
                    else
                        ModifyValueOnBrains(myValue, targetBody);

                    break;
            }

            _profilerDo.End();
            return Complete(InstructionTileResult.Success);
        }

        static readonly ProfilerMarker _profileModifyOnTag = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyOnTag)}");

        private void ModifyOnTag(IMonaVariablesValue myValue)
        {
            _profileModifyOnTag.Begin();
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
            {
                _profileModifyOnTag.End();
                return;
            }

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(myValue, tagBodies[i]);
                else
                    ModifyValueOnBrains(myValue, tagBodies[i]);
            }
            _profileModifyOnTag.End();
        }

        static readonly ProfilerMarker _profileModifyOnWholeEntity = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyOnWholeEntity)}");

        private void ModifyOnWholeEntity(IMonaVariablesValue myValue, IMonaBody body)
        {
            _profileModifyOnWholeEntity.Begin();
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ModifyValueOnBrains(myValue, topBody);
            ModifyOnChildren(myValue, topBody);
            _profileModifyOnWholeEntity.End();
        }

        static readonly ProfilerMarker _profileModifyOnParents = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyOnParents)}");

        private void ModifyOnParents(IMonaVariablesValue myValue, IMonaBody body)
        {
            _profileModifyOnParents.Begin();
            IMonaBody parent = body.Parent;

            if (parent == null)
            {
                _profileModifyOnParents.End();
                return;
            }

            ModifyValueOnBrains(myValue, parent);
            ModifyOnParents(myValue, parent);
            _profileModifyOnParents.End();
        }

        private void ModifyOnChildren(IMonaVariablesValue myValue, IMonaBody body)
        {
        
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                ModifyValueOnBrains(myValue, children[i]);
                ModifyOnChildren(myValue, children[i]);
            }
        
        }

        private void ModifyOnAllSpawned(IMonaVariablesValue myValue)
        {
        
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(myValue, spawned[i]);
                else
                    ModifyValueOnBrains(myValue, spawned[i]);
            }
            
        }

        static readonly ProfilerMarker _profileGetTarget = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(GetTarget)}");

        private IMonaBody GetTarget()
        {
            _profileGetTarget.Begin();
            switch (_target)
            {
                case MonaBrainBroadcastType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainBroadcastType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                    {
                        _profileGetTarget.End();
                        return brain.Body;
                    }
                    break;
                case MonaBrainBroadcastType.OnConditionTarget:
                    _profileGetTarget.End();
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainBroadcastType.OnHitTarget:
                    _profileGetTarget.End();
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    _profileGetTarget.End();
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    _profileGetTarget.End();
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    _profileGetTarget.End();
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    _profileGetTarget.End();
                    return _brain.Body.PoolBodyNext;
            }
            _profileGetTarget.End();
            return null;
        }

        static readonly ProfilerMarker _profileModifyValueOnBrains = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.{nameof(ModifyValueOnBrains)}");


        static readonly ProfilerMarker markerGet = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.Get");
        static readonly ProfilerMarker markerSet = new ProfilerMarker($"MonaBrains.{nameof(SetVariableOnTypeInstructionTile)}.Set");


        private void ModifyValueOnBrains(IMonaVariablesValue myValue, IMonaBody body)
        {
            if (body.ActiveTransform == null)
                return;

            _profileModifyValueOnBrains.Begin();
            var runner = GetCachedRunner(body);

            if (runner == null)
            {
                _profileModifyValueOnBrains.End();
                return;
            }

            for (int i = 0; i < runner.BrainInstances.Count; i++)
            {
                var brainVariables = runner.BrainInstances[i].Variables;

                if (brainVariables == null)
                    continue;

                switch (_variableType)
                {
                    case VariableUsageType.Number:
                        brainVariables.Set(_targetVariable, _myNumber, true, false);
                        break;
                    case VariableUsageType.String:
                        brainVariables.Set(_targetVariable, _myString, true, false);
                        break;
                    case VariableUsageType.Boolean:
                        brainVariables.Set(_targetVariable, _myBool, true, false);
                        break;
                    case VariableUsageType.Vector2:
                        brainVariables.Set(_targetVariable, _myVector2, true, false);
                        break;
                    case VariableUsageType.Vector3:
                        brainVariables.Set(_targetVariable, _myVector3, true, false);
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
                        if (targetValue is IMonaVariablesStringValue)
                            brainVariables.Set(_targetVariable, _brain.Variables.GetValueAsString(_myVariable), false);
                        else if (targetValue is IMonaVariablesFloatValue && myValue is IMonaVariablesFloatValue)
                            brainVariables.Set(_targetVariable, _brain.Variables.GetFloat(_myVariable), false);
                        else if (targetValue is IMonaVariablesBoolValue && myValue is IMonaVariablesBoolValue)
                            brainVariables.Set(_targetVariable, _brain.Variables.GetBool(_myVariable), false);
                        else if (targetValue is IMonaVariablesVector2Value && myValue is IMonaVariablesVector2Value)
                            brainVariables.Set(_targetVariable, _brain.Variables.GetVector2(_myVariable), false);
                        else if (targetValue is IMonaVariablesVector3Value && myValue is IMonaVariablesVector3Value)
                            brainVariables.Set(_targetVariable, _brain.Variables.GetVector3(_myVariable), false);
                        markerSet.End();
                        break;
                }
            }
            _profileModifyValueOnBrains.End();
        }
    }
}