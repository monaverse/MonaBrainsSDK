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

        [SerializeField] private string _message;
        [BrainProperty(true)] public string Message { get => _message; set => _message = value; }

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

        private IMonaBrainRunner GetCachedRunner(IMonaBody body)
        {
            if (!_runnerCache.ContainsKey(body))
                _runnerCache.Add(body, body.Transform.GetComponent<IMonaBrainRunner>());
            return _runnerCache[body];
        }

        public override InstructionTileResult Do()
        {
            switch (_messageTarget)
            {
                case MonaBrainBroadcastType.Tag:
                    BroadcastToTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    BroadcastToSelf();
                    break;
                case MonaBrainBroadcastType.Children:
                    BroadcastToChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    BroadcastToAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();
                    if (targetBody != null)
                        BroadcastMessage(_brain, _message, targetBody);
                    break;
            }

            return Complete(InstructionTileResult.Success);
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
                        BroadcastMessage(_brain, _message, runner.BrainInstances[j]);
                }
            }
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

        private void BroadcastToChildren(IMonaBody body)
        {
            if (_brain.LoggingEnabled)
                Debug.Log($"{nameof(BroadcastToChildren)} {_message} {body.Transform.name}", body.Transform.gameObject);

            var children = body.Children();

            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
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
                if (spawned[i] != null)
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
                case MonaBrainBroadcastType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastType.MySpawner:
                    return _brain.Variables.GetBody(MonaBrainConstants.TAG_SPAWNER);
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
            }
            return null;
        }
    }
}