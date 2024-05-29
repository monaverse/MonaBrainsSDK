using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using System;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using System.Collections.Generic;
using Unity.Profiling;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageToTypeInstructionTile : BroadcastMessageInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "BroadcastMessage";
        public const string NAME = "Send Message";
        public const string CATEGORY = "Broadcasting";
        public override Type TileType => typeof(BroadcastMessageToTypeInstructionTile);

        [SerializeField] private MonaBrainBroadcastType _messageTarget = MonaBrainBroadcastType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType MessageTarget { get => _messageTarget; set => _messageTarget = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private string _message;
        [SerializeField] private string _messageName;
        [BrainProperty(true)] public string Message { get => _message; set => _message = value; }
        [BrainPropertyValueName("Message", typeof(IMonaVariablesStringValue))] public string MessageName { get => _messageName; set => _messageName = value; }

        [SerializeField] private bool _includeAttached;
        [SerializeField] private string _includeAttachedName;
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.OnSelectTarget)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.MySpawner)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.MyPoolNextSpawned)]
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

        [SerializeField] private bool _appendPlayerId;
        [BrainPropertyShow(nameof(MessageTarget), (int)MonaBrainBroadcastType.Tag)]
        [BrainProperty(false)] public bool AddPlayerIdToTag { get => _appendPlayerId; set => _appendPlayerId = value; }

        private IMonaBrain _brain;
        private Dictionary<IMonaBody, IMonaBrainRunner> _runnerCache = new Dictionary<IMonaBody, IMonaBrainRunner>();

        public BroadcastMessageToTypeInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        private bool SendToAllAttached
        {
            get
            {
                switch (_messageTarget)
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

        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(BroadcastMessageToTypeInstructionTile)}.{nameof(Do)}");

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            //_profilerDo.Begin();

            if (!string.IsNullOrEmpty(_messageName))
                _message = _brain.Variables.GetString(_messageName);

            if (!string.IsNullOrEmpty(_includeAttachedName))
                _includeAttached = _brain.Variables.GetBool(_includeAttachedName);

            switch (_messageTarget)
            {
                case MonaBrainBroadcastType.Tag:
                    BroadcastToTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    BroadcastToWholeEntity(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    BroadcastToParents(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    BroadcastToChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    BroadcastMessage(_brain, _message, _brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    BroadcastToAllSpawned();
                    break;
                case MonaBrainBroadcastType.MyBodyArray:
                    BroadcastToBodyArray(_brain);
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (SendToAllAttached)
                        BroadcastToWholeEntity(targetBody);
                    else
                        BroadcastMessage(_brain, _message, targetBody);
                    break;
            }

            //_profilerDo.End();
            return Complete(InstructionTileResult.Success);
        }

        private void BroadcastToBodyArray(IMonaBrain brain)
        {
            var bodyArray = brain.Variables.GetBodyArray(_bodyArray);

            for (var i = 0; i < bodyArray.Count; i++)
            {
                if (SendToAllAttached)
                    BroadcastToWholeEntity(bodyArray[i]);
                else
                {
                    var runner = GetCachedRunner(bodyArray[i]);
                    if (runner != null)
                    {
                        for (var j = 0; j < runner.BrainInstances.Count; j++)
                        {
                            if (SendToAllAttached)
                                BroadcastToWholeEntity(runner.BrainInstances[j].Body);
                            else
                                BroadcastMessage(_brain, _message, runner.BrainInstances[j]);
                        }
                    }
                }
            }
        }

        private void BroadcastToTag()
        {
            var tag = _tag;
            if (_appendPlayerId)
            {
                tag = $"{tag}{_brain.Player.PlayerId.ToString("00")}";
                //Debug.Log($"{nameof(BroadcastMessageToTagInstructionTile)} {tag}");
            }

            var bodies = MonaBody.FindByTag(tag);
            if (bodies.Count == 0)
                bodies = MonaBody.FindByTag(_tag);

            for (var i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];
                var runner = GetCachedRunner(body);
                if (runner != null)
                {
                    for (var j = 0; j < runner.BrainInstances.Count; j++)
                    {
                        if (SendToAllAttached)
                            BroadcastToWholeEntity(runner.BrainInstances[j].Body);
                        else
                            BroadcastMessage(_brain, _message, runner.BrainInstances[j]);
                    }
                }
            }
        }

        private void BroadcastToWholeEntity(IMonaBody body)
        {
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(BroadcastMessageToSelfInstructionTile)} {_message}");

            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            BroadcastMessage(_brain, _message, topBody);
            BroadcastToChildren(topBody);
        }

        private void BroadcastToSelf()
        {
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(BroadcastMessageToSelfInstructionTile)} {_message}");

            IMonaBody topBody = _brain.Body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            BroadcastMessage(_brain, _message, topBody);
            BroadcastToChildren(topBody);
        }

        private void BroadcastToParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            BroadcastMessage(_brain, _message, parent);
            BroadcastToParents(parent);
        }

        private void BroadcastToChildren(IMonaBody body)
        {
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(BroadcastToChildren)} {_message} {body.Transform.name}", body.Transform.gameObject);

            var children = body.Children();

            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] == null || !children[i].GetActive())
                    continue;

                BroadcastMessage(_brain, _message, children[i]);
                BroadcastToChildren(children[i]);
            }
        }

        private void BroadcastToAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null || !spawned[i].GetActive())
                    continue;

                if (SendToAllAttached)
                    BroadcastToWholeEntity(spawned[i]);
                else
                    BroadcastMessage(_brain, _message, spawned[i]);
            }
        }

        private IMonaBody GetTarget()
        {
            switch (_messageTarget)
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
            }
            return null;
        }
    }
}