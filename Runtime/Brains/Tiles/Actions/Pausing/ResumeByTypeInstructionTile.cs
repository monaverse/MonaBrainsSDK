using UnityEngine;
using System;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ResumeByTypeInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "ResumeByType";
        public const string NAME = "Resume Body";
        public const string CATEGORY = "Pause / Resume";
        public override Type TileType => typeof(ResumeByTypeInstructionTile);

        [SerializeField] protected MonaBrainPausableTargetType _target = MonaBrainPausableTargetType.All;
        [BrainPropertyEnum(true)] public MonaBrainPausableTargetType Target { get => _target; set => _target = value; }

        [SerializeField] protected string _targetTag = "Pausable";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private bool _includeAttached = true;
        [SerializeField] private string _includeAttachedName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.OnHitTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

        public ResumeByTypeInstructionTile() { }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            switch (_target)
            {
                case MonaBrainPausableTargetType.Tag:
                    SetBodiesByTag();
                    break;
                case MonaBrainPausableTargetType.All:
                    SetForAll();
                    break;
                case MonaBrainPausableTargetType.Self:
                    SetBodiesFromWholeEntity(_brain.Body);
                    break;
                case MonaBrainPausableTargetType.Parents:
                    SetBodiesFromParents(_brain.Body);
                    break;
                case MonaBrainPausableTargetType.Children:
                    SetBodiesFromChildren(_brain.Body);
                    break;
                case MonaBrainPausableTargetType.ThisBodyOnly:
                    SetForBody(_brain.Body);
                    break;
                case MonaBrainPausableTargetType.AllSpawnedByMe:
                    SetBodiesFromAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (HandleAllAttached)
                        SetBodiesFromWholeEntity(targetBody);
                    else
                        SetForBody(targetBody);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private bool HandleAllAttached
        {
            get
            {
                switch (_target)
                {
                    case MonaBrainPausableTargetType.All:
                        return false;
                    case MonaBrainPausableTargetType.Self:
                        return false;
                    case MonaBrainPausableTargetType.Parent:
                        return false;
                    case MonaBrainPausableTargetType.Parents:
                        return false;
                    case MonaBrainPausableTargetType.Children:
                        return false;
                    case MonaBrainPausableTargetType.ThisBodyOnly:
                        return false;
                }

                return _includeAttached;
            }
        }

        private void SetForAll()
        {
            var globalBodies = GameObject.FindObjectsOfType<MonaBody>();

            for (int i = 0; i < globalBodies.Length; i++)
            {
                if (globalBodies[i] != null)
                    globalBodies[i].Resume();
            }
        }

        private void SetForBody(IMonaBody body)
        {
            if (body != null)
                body.Resume();
        }

        private void SetBodiesByTag()
        {
            bool handleAttached = HandleAllAttached;
            var tagBodies = MonaBody.FindByTag(_targetTag);

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (handleAttached) SetBodiesFromWholeEntity(tagBodies[i]);
                else SetForBody(tagBodies[i]);
            }
        }

        private void SetBodiesFromWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetForBody(topBody);
            SetBodiesFromChildren(topBody);
        }

        private void SetBodiesFromParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetForBody(parent);
            SetBodiesFromParents(parent);
        }

        private void SetBodiesFromChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetForBody(children[i]);
                SetBodiesFromChildren(children[i]);
            }
        }

        private void SetBodiesFromAllSpawned()
        {
            bool handleAttached = HandleAllAttached;
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (handleAttached)
                    SetBodiesFromWholeEntity(spawned[i]);
                else
                    SetForBody(spawned[i]);
            }
        }

        private IMonaBody GetTarget()
        {
            switch (_target)
            {
                case MonaBrainPausableTargetType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainPausableTargetType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainPausableTargetType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainPausableTargetType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainPausableTargetType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainPausableTargetType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainPausableTargetType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainPausableTargetType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }
    }
}