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
        [BrainPropertyValue(typeof(IMonaVariablesValue))] public string MyVariable { get => _myVariable; set => _myVariable = value; }

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
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

        [SerializeField] private bool _appendPlayerId;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainProperty(false)] public bool AddPlayerIdToTag { get => _appendPlayerId; set => _appendPlayerId = value; }

        private IMonaBrain _brain;
        private Dictionary<IMonaBody, IMonaBrainRunner> _runnerCache = new Dictionary<IMonaBody, IMonaBrainRunner>();

        public SetVariableOnTypeInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

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
                    case MonaBrainBroadcastType.Children:
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
            if (_brain == null || string.IsNullOrEmpty(_myVariable))
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_includeAttachedName))
                _includeAttached = _brain.Variables.GetBool(_includeAttachedName);

            var myValue = _brain.Variables.GetVariable(_myVariable);

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    ModifyOnTag(myValue);
                    break;
                case MonaBrainBroadcastType.Self:
                    ModifyOnWholeEntity(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    ModifyOnChildren(myValue, _brain.Body);
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

            return Complete(InstructionTileResult.Success);
        }

        private void ModifyOnTag(IMonaVariablesValue myValue)
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(myValue, tagBodies[i]);
                else
                    ModifyValueOnBrains(myValue, tagBodies[i]);
            }
        }

        private void ModifyOnWholeEntity(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ModifyValueOnBrains(myValue, topBody);
            ModifyOnChildren(myValue, topBody);
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
                case MonaBrainBroadcastType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
            }
            return null;
        }

        private void ModifyValueOnBrains(IMonaVariablesValue myValue, IMonaBody body)
        {
            if (body.ActiveTransform == null)//RG - won't allow me to use one brain on a body to set another brain on the same body, commenting for now || (body == _brain.Body && _targetVariable == _myVariable))
                return;

            var runner = GetCachedRunner(body);

            if (runner == null)
                return;

            for (int i = 0; i < runner.BrainInstances.Count; i++)
            {
                var brainVariables = runner.BrainInstances[i].Variables;

                if (brainVariables == null)
                    continue;

                var tagrgetValue = brainVariables.GetVariable(_targetVariable);

                if (tagrgetValue == null)
                    continue;

                if (tagrgetValue is IMonaVariablesStringValue)
                {
                    brainVariables.Set(_targetVariable, _brain.Variables.GetValueAsString(_myVariable));
                }
                else if (tagrgetValue is IMonaVariablesFloatValue && myValue is IMonaVariablesFloatValue)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesFloatValue)myValue).Value);
                }
                else if (tagrgetValue is IMonaVariablesBoolValue && myValue is IMonaVariablesBoolValue)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesBoolValue)myValue).Value);
                }
                else if (tagrgetValue is IMonaVariablesVector2Value && myValue is IMonaVariablesVector2Value)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesVector2Value)myValue).Value);
                }
                else if (tagrgetValue is IMonaVariablesVector3Value && myValue is IMonaVariablesVector3Value)
                {
                    brainVariables.Set(_targetVariable, ((IMonaVariablesVector3Value)myValue).Value);
                }
            }
        }
    }
}