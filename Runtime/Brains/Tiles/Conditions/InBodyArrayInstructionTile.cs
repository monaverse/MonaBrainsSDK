using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class InBodyArrayInstructionTile : InstructionTile, IInstructionTileWithPreload, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "InBodyArray";
        public const string NAME = "In Body Array";
        public const string CATEGORY = "Values";
        public override Type TileType => typeof(InBodyArrayInstructionTile);

        private IMonaVariablesBodyArrayValue _value;

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private BodyArrayOperatorType _operator = BodyArrayOperatorType.ContainedIn;
        [BrainPropertyEnum(true)] public BodyArrayOperatorType Operator { get => _operator; set => _operator = value; }

        [SerializeField] private string _valueName;
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _valueName; set => _valueName = value; }

        [SerializeField] private BodyArrayFilterType _filter = BodyArrayFilterType.Any;
        [BrainPropertyEnum(false)] public BodyArrayFilterType Filter { get => _filter; set => _filter = value; }

        [SerializeField] private bool _searchAllAttached;
        [BrainProperty(false)] public bool SearchAllAttached { get => _searchAllAttached; set => _searchAllAttached = value; }

        private IMonaBrain _brain;

        public InBodyArrayInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            var myValue = _brain.Variables.GetVariable(_valueName);
            var result = false;

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    result = InBodyArrayOnTag(myValue);
                    break;
                case MonaBrainBroadcastType.Self:
                    result = InBodyArrayOnWholeEntity(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    result = InBodyArrayOnParents(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    result = InBodyArrayOnChildren(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    result = InBodyArrayValueOnBody(myValue, _brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    result = InBodyArrayOnAllSpawned(myValue);
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (SearchAllAttached)
                        result = InBodyArrayOnWholeEntity(myValue, targetBody);
                    else
                        result = InBodyArrayValueOnBody(myValue, targetBody);

                    break;
            }

            if (result)
            {
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool InBodyArrayOnTag(IMonaVariablesValue myValue)
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
            {
                return false;
            }

            if (_filter == BodyArrayFilterType.Any)
            {
                for (int i = 0; i < tagBodies.Count; i++)
                {
                    if (tagBodies[i] == null)
                        continue;

                    if (SearchAllAttached)
                    {
                        if (InBodyArrayOnWholeEntity(myValue, tagBodies[i]))
                            return true;
                    }
                    else
                    {
                        if (InBodyArrayValueOnBody(myValue, tagBodies[i]))
                            return true;
                    }
                }
            }

            var result = _filter == BodyArrayFilterType.Any ? false : true;

            if(_filter == BodyArrayFilterType.All)
            {
                for (int i = 0; i < tagBodies.Count; i++)
                {
                    if (tagBodies[i] == null)
                        continue;

                    if (SearchAllAttached)
                    {
                        if (!InBodyArrayOnWholeEntity(myValue, tagBodies[i]))
                            result = false;
                    }
                    else
                    {
                        if (!InBodyArrayValueOnBody(myValue, tagBodies[i]))
                            result = false;
                    }
                }
            }

            return result;
        }

        private bool InBodyArrayOnWholeEntity(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            if(_filter == BodyArrayFilterType.Any)
            {
                if (InBodyArrayValueOnBody(myValue, topBody))
                    return true;
                if (InBodyArrayOnChildren(myValue, topBody))
                    return true;
            }

            var result = _filter == BodyArrayFilterType.Any ? false : true;

            if (_filter == BodyArrayFilterType.All)
            {
                if (!InBodyArrayValueOnBody(myValue, topBody))
                    result = false;
                if (!InBodyArrayOnChildren(myValue, topBody))
                    result = false;
            }

            return result;
        }

        private bool InBodyArrayOnParents(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
            {
                return false;
            }

            if (_filter == BodyArrayFilterType.Any)
            {
                if (InBodyArrayValueOnBody(myValue, parent))
                    return true;
                if (InBodyArrayOnParents(myValue, parent))
                    return true;
            }

            var result = _filter == BodyArrayFilterType.Any ? false : true;

            if (_filter == BodyArrayFilterType.All)
            {
                if (!InBodyArrayValueOnBody(myValue, parent))
                    result = false;
                if (!InBodyArrayOnParents(myValue, parent))
                    result = false;
            }

            return result;
        }

        private bool InBodyArrayOnChildren(IMonaVariablesValue myValue, IMonaBody body)
        {
            var children = body.Children();

            if (_filter == BodyArrayFilterType.Any)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] == null)
                        continue;

                    if (InBodyArrayValueOnBody(myValue, children[i]))
                        return true;
                    if (InBodyArrayOnChildren(myValue, children[i]))
                        return true;
                }
            }

            var result = _filter == BodyArrayFilterType.Any ? false : true;

            if (_filter == BodyArrayFilterType.All)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] == null)
                        continue;

                    if (!InBodyArrayValueOnBody(myValue, children[i]))
                        result = false;
                    if (!InBodyArrayOnChildren(myValue, children[i]))
                        result = false;
                }
            }

            return result;

        }

        private bool InBodyArrayOnAllSpawned(IMonaVariablesValue myValue)
        {

            var spawned = _brain.SpawnedBodies;

            if (_filter == BodyArrayFilterType.Any)
            {
                for (int i = 0; i < spawned.Count; i++)
                {
                    if (spawned[i] == null)
                        continue;

                    if (SearchAllAttached)
                    {
                        if (InBodyArrayOnWholeEntity(myValue, spawned[i]))
                            return true;
                    }
                    else
                    {
                        if (InBodyArrayValueOnBody(myValue, spawned[i]))
                            return true;
                    }
                }
            }

            var result = _filter == BodyArrayFilterType.Any ? false : true;

            if (_filter == BodyArrayFilterType.All)
            {
                for (int i = 0; i < spawned.Count; i++)
                {
                    if (spawned[i] == null)
                        continue;

                    if (SearchAllAttached)
                    {
                        if (!InBodyArrayOnWholeEntity(myValue, spawned[i]))
                            result = false;
                    }
                    else
                    {
                        if (!InBodyArrayValueOnBody(myValue, spawned[i]))
                            result = false;
                    }
                }
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
                    {

                        return brain.Body;
                    }
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
            }
            return null;
        }

        private bool InBodyArrayValueOnBody(IMonaVariablesValue myValue, IMonaBody target)
        {
            var value = ((IMonaVariablesBodyArrayValue)myValue).Value;
            if(_operator == BodyArrayOperatorType.ContainedIn)
            {
                if (value.Contains(target))
                    return true;
            }
            else
            {
                if (!value.Contains(target))
                    return true;
            }
            return false;
        }

    }
}