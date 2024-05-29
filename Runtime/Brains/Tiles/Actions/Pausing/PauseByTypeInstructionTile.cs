using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class PauseByTypeInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "PauseByType";
        public const string NAME = "Pause Body";
        public const string CATEGORY = "Pause / Resume";
        public override Type TileType => typeof(PauseByTypeInstructionTile);

        [SerializeField] protected MonaBrainPausableTargetType _target = MonaBrainPausableTargetType.Tag;
        [BrainPropertyEnum(true)] public MonaBrainPausableTargetType Target { get => _target; set => _target = value; }

        [SerializeField] protected string _targetTag = "Pausable";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] protected MonaBrainPausableTargetType _exclude = MonaBrainPausableTargetType.Self;
        [BrainPropertyEnum(true)] public MonaBrainPausableTargetType Exclude { get => _exclude; set => _exclude = value; }

        [SerializeField] protected string _excludeTag = "Unpausable";
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string ExcludeTag { get => _excludeTag; set => _excludeTag = value; }

        [SerializeField] protected ExcludeMeType _myHandling = ExcludeMeType.ExcludeMe;
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.Tag)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.Parent)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.Parents)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.Children)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MessageSender)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MySpawner)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MyPoolNextSpawned)]
        [BrainPropertyEnum(false)] public ExcludeMeType MyHandling { get => _myHandling; set => _myHandling = value; }

        [SerializeField] private bool _targetAttached = true;
        [SerializeField] private string _targetAttachedName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainPausableTargetType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool TargetAttached { get => _targetAttached; set => _targetAttached = value; }
        [BrainPropertyValueName("TargetAttached", typeof(IMonaVariablesBoolValue))] public string TargetAttachedName { get => _targetAttachedName; set => _targetAttachedName = value; }

        [SerializeField] private bool _excludeAttached = true;
        [SerializeField] private string _excludeAttachedName;
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.Tag)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MessageSender)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MySpawner)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Exclude), (int)MonaBrainPausableTargetType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool ExcludeAttached { get => _excludeAttached; set => _excludeAttached = value; }
        [BrainPropertyValueName("ExcludeAttached", typeof(IMonaVariablesBoolValue))] public string ExcludeAttachedName { get => _excludeAttachedName; set => _excludeAttachedName = value; }

        public PauseByTypeInstructionTile() { }

        private List<IMonaBody> _excludedBodies = new List<IMonaBody>();
        private List<IMonaBody> _includedBodies = new List<IMonaBody>();
        private List<IMonaBody> _pausableBodies = new List<IMonaBody>();

        public enum ExcludeMeType
        {
            ExcludeMe,
            ExcludeAllOfMe,
            AllowInclusion
        }

        private IMonaBrain _brain;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (_exclude == MonaBrainPausableTargetType.All)
                return Complete(InstructionTileResult.Success);

            _excludedBodies.Clear();
            _includedBodies.Clear();
            _pausableBodies.Clear();

            switch (_myHandling)
            {
                case ExcludeMeType.ExcludeMe:
                    SetForBody(false, _brain.Body);
                    break;
                case ExcludeMeType.ExcludeAllOfMe:
                    SetBodiesFromWholeEntity(false, _brain.Body);
                    break;
            }

            SetBodiesFromTarget(true);
            SetBodiesFromTarget(false);

            PauseValidBodies();
            return Complete(InstructionTileResult.Success);
        }

        private void SetBodiesFromTarget(bool include)
        {
            switch (include ? _target : _exclude)
            {
                case MonaBrainPausableTargetType.Tag:
                    SetBodiesByTag(include);
                    break;
                case MonaBrainPausableTargetType.All:
                    SetForAll(include);
                    break;
                case MonaBrainPausableTargetType.Self:
                    SetBodiesFromWholeEntity(include, _brain.Body);
                    break;
                case MonaBrainPausableTargetType.Parents:
                    SetBodiesFromParents(include, _brain.Body);
                    break;
                case MonaBrainPausableTargetType.Children:
                    SetBodiesFromChildren(include, _brain.Body);
                    break;
                case MonaBrainPausableTargetType.ThisBodyOnly:
                    SetForBody(include, _brain.Body);
                    break;
                case MonaBrainPausableTargetType.AllSpawnedByMe:
                    SetBodiesFromAllSpawned(include);
                    break;
                default:
                    IMonaBody targetBody = GetTarget(include);

                    if (targetBody == null)
                        break;

                    if (HandleAllAttached(include))
                        SetBodiesFromWholeEntity(include, targetBody);
                    else
                        SetForBody(include, targetBody);
                    break;
            }
        }

        private bool HandleAllAttached(bool include)
        {
            switch (include ? _target : _exclude)
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

            return include ? _targetAttached : _excludeAttached;
        }

        private void SetForBody(bool include, IMonaBody body)
        {
            if (include) _includedBodies.Add(body);
            else _excludedBodies.Add(body);
        }

        private void SetForAll(bool include)
        {
            var globalBodies = GameObject.FindObjectsOfType<MonaBody>();

            if (include) _includedBodies.AddRange(globalBodies);
            else _excludedBodies.AddRange(globalBodies);
        }

        private void SetBodiesByTag(bool include)
        {
            string tag = include ? _targetTag : _excludeTag;
            bool handleAttached = HandleAllAttached(include);
            var tagBodies = MonaBody.FindByTag(tag);

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (handleAttached) SetBodiesFromWholeEntity(include, tagBodies[i]);
                else SetForBody(include, tagBodies[i]);
            }
        }

        private void SetBodiesFromWholeEntity(bool include, IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetForBody(include, topBody);
            SetBodiesFromChildren(include, topBody);
        }

        private void SetBodiesFromParents(bool include, IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetForBody(include, parent);
            SetBodiesFromParents(include, parent);
        }

        private void SetBodiesFromChildren(bool include, IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetForBody(include, children[i]);
                SetBodiesFromChildren(include, children[i]);
            }
        }

        private void SetBodiesFromAllSpawned(bool include)
        {
            bool handleAttached = HandleAllAttached(include);
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (handleAttached)
                    SetBodiesFromWholeEntity(include, spawned[i]);
                else
                    SetForBody(include, spawned[i]);
            }
        }

        private IMonaBody GetTarget(bool include)
        {
            switch (include ? _target : _exclude)
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
                case MonaBrainPausableTargetType.OnSelectTarget:
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

        private void PauseValidBodies()
        {
            bool pauseMe = false;
            IMonaBody myBody = _brain.Body;

            for (int i = 0; i < _includedBodies.Count; i++)
            {
                if (_excludedBodies.Contains(_includedBodies[i]))
                    continue;

                _pausableBodies.Add(_includedBodies[i]);
            }

            for (int i = 0; i < _pausableBodies.Count; i++)
            {
                if (_pausableBodies[i] == myBody)
                {
                    pauseMe = true;
                    continue;
                }

                _pausableBodies[i].Pause();
            }

            if (pauseMe)
                myBody.Pause();
        }
    }
}