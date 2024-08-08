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

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class GetVariableOnTypeInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "GetValue";
        public const string NAME = "Get Value On";
        public const string CATEGORY = "Variables";
        public override Type TileType => typeof(GetVariableOnTypeInstructionTile);

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _targetVariable;
        [SerializeField] private string _targetVariableName;
        [BrainProperty(true)] public string TargetVariable { get => _targetVariable; set => _targetVariable = value; }
        [BrainPropertyValueName("TargetVariable", typeof(IMonaVariablesStringValue))] public string TargetVariableName { get => _targetVariableName; set => _targetVariableName = value; }

        [SerializeField] private string _storeResultOn;
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string StoreResultOn { get => _storeResultOn; set => _storeResultOn = value; }

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

        private IMonaBrain _brain;
        private Dictionary<IMonaBody, IMonaBrainRunner> _runnerCache = new Dictionary<IMonaBody, IMonaBrainRunner>();

        public GetVariableOnTypeInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        private bool _childBodyValueFound = false;

        private bool GetFromAllAttached
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

        public override InstructionTileResult Do()
        {
            if (_brain == null || string.IsNullOrEmpty(_storeResultOn))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _childBodyValueFound = false;

            var myValue = _brain.Variables.GetVariable(_storeResultOn);

            if (!string.IsNullOrEmpty(_includeAttachedName))
                _includeAttached = _brain.Variables.GetBool(_includeAttachedName);

            if (!string.IsNullOrEmpty(_targetVariableName))
                _targetVariable = _brain.Variables.GetString(_targetVariableName);

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    GetFromTag(myValue);
                    break;
                case MonaBrainBroadcastType.Self:
                    GetFromWholeEntity(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    GetFromParents(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    GetFromChildren(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    GetValueFromBrains(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    GetFromAllSpawned(myValue);
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (GetFromAllAttached)
                        GetFromWholeEntity(myValue, targetBody);
                    else
                        GetValueFromBrains(myValue, targetBody);

                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private void GetFromTag(IMonaVariablesValue myValue)
        {
            var tagBodies = MonaBodyFactory.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (_childBodyValueFound)
                    return;

                if (GetFromAllAttached)
                    GetFromWholeEntity(myValue, tagBodies[i]);
                else if (GetValueFromBrains(myValue, tagBodies[i]))
                    break;

            }
        }

        private void GetFromWholeEntity(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            if (GetValueFromBrains(myValue, topBody))
                return;

            GetFromChildren(myValue, topBody);
        }

        private void GetFromParents(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null || GetValueFromBrains(myValue, parent))
                return;

            GetFromParents(myValue, parent);
        }

        private void GetFromChildren(IMonaVariablesValue myValue, IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                if (_childBodyValueFound)
                    return;

                if (GetValueFromBrains(myValue, children[i]))
                {
                    _childBodyValueFound = true;
                    return;
                }

                GetFromChildren(myValue, children[i]);
            }
        }

        private void GetFromAllSpawned(IMonaVariablesValue myValue)
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (_childBodyValueFound)
                    return;

                if (GetFromAllAttached)
                    GetFromWholeEntity(myValue, spawned[i]);
                else if (GetValueFromBrains(myValue, spawned[i]))
                    break;
            }
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
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:;
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
                case MonaBrainBroadcastType.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
            }
            return null;
        }

        private bool GetValueFromBrains(IMonaVariablesValue myValue, IMonaBody body)
        {
            bool valueFound = false;
            
            if (body.ActiveTransform == null || body == _brain.Body)
                return false;

            var runner = GetCachedRunner(body);

            if (runner == null)
                return false;

            //Debug.Log("AA: Get Value '" + _targetVariable + "' - body and runner found on object: " + body.Transform.gameObject.name);

            for (int i = 0; i < runner.BrainInstances.Count; i++)
            {
                var brainVariables = runner.BrainInstances[i].Variables;

                if (brainVariables == null)
                    continue;
                
                var targetValue = brainVariables.GetVariable(_targetVariable);

                if (targetValue == null)
                    continue;

                //Debug.Log("AA: Get Value - Can get value '" + _targetVariable + "' from Brain: " + runner.BrainInstances[i].Name + " | Value = " + brainVariables.GetValueAsString(_targetVariable));

                if (myValue is IMonaVariablesStringValue)
                    _brain.Variables.Set(_storeResultOn, brainVariables.GetValueAsString(_targetVariable));
                else if (targetValue is IMonaVariablesFloatValue && myValue is IMonaVariablesFloatValue)
                    _brain.Variables.Set(_storeResultOn, brainVariables.GetFloat(_targetVariable, false));
                else if (targetValue is IMonaVariablesBoolValue && myValue is IMonaVariablesBoolValue)
                    _brain.Variables.Set(_storeResultOn, brainVariables.GetBool(_targetVariable, false));
                else if (targetValue is IMonaVariablesVector2Value && myValue is IMonaVariablesVector2Value)
                    _brain.Variables.Set(_storeResultOn, brainVariables.GetVector2(_targetVariable, false));
                else if (targetValue is IMonaVariablesVector3Value && myValue is IMonaVariablesVector3Value)
                    _brain.Variables.Set(_storeResultOn, brainVariables.GetVector3(_targetVariable, false));

                valueFound = true;
                break;
            }

            return valueFound;
        }
    }
}