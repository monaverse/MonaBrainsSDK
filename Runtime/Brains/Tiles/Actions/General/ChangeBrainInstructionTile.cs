using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Brains.Core.ScriptableObjects;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ChangeBrainInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "ChangeBrain";
        public const string NAME = "Add / Remove Brain";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeBrainInstructionTile);

        public bool IsAnimationTile => true;

        [SerializeField] private BrainActionType _mainAction = BrainActionType.AddBrain;
        [BrainPropertyEnum(true)] public BrainActionType MainAction { get => _mainAction; set => _mainAction = value; }

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private AddBrainType _brainsToAdd = AddBrainType.ThisBrain;
        [BrainPropertyShow(nameof(MainAction), (int)BrainActionType.AddBrain)]
        [BrainPropertyEnum(true)] public AddBrainType BrainsToAdd { get => _brainsToAdd; set => _brainsToAdd = value; }

        [SerializeField] private RemoveBrainType _brainsToRemove = RemoveBrainType.ThisBrain;
        [BrainPropertyShow(nameof(MainAction), (int)BrainActionType.RemoveBrain)]
        [BrainPropertyEnum(true)] public RemoveBrainType BrainsToRemove { get => _brainsToRemove; set => _brainsToRemove = value; }

        [SerializeField] private string _brainString;
        [SerializeField] private string _brainStringName;
        [BrainPropertyShow(nameof(BrainStringDisplay), (int)ElementDisplayType.Show)]
        [BrainProperty(true)] public string BrainString { get => _brainString; set => _brainString = value; }
        [BrainPropertyValueName("BrainString", typeof(IMonaVariablesStringValue))] public string BrainStringName { get => _brainStringName; set => _brainStringName = value; }

        [SerializeField] private bool _includeAttached = false;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyPoolNextSpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.LastSkin)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }

        [SerializeField] private bool _includeChildren = false;
        [BrainProperty(false)] public bool IncludeChildren { get => _includeChildren; set => _includeChildren = value; }

        private IMonaBrain _brain;
        private List<MonaBrainGraph> _brainGraphsToAdd = new List<MonaBrainGraph>();
        private List<MonaBrainGraph> _availableBrainGraphs = new List<MonaBrainGraph>();
        private List<MonaBrainRunner> _selectedBrainRunners = new List<MonaBrainRunner>();

        public ElementDisplayType BrainStringDisplay
        {
            get
            {
                if (_mainAction == BrainActionType.AddBrain && (_brainsToAdd == AddBrainType.WithName || _brainsToAdd == AddBrainType.ContainingString))
                    return ElementDisplayType.Show;
                else if (_mainAction == BrainActionType.RemoveBrain && (_brainsToRemove == RemoveBrainType.WithName || _brainsToRemove == RemoveBrainType.ContainingString))
                    return ElementDisplayType.Show;

                return ElementDisplayType.Hide;
            }
        }

        public enum BrainActionType
        {
            AddBrain,
            RemoveBrain
        }

        public enum AddBrainType
        {
            ThisBrain = 0,
            WithName = 10,
            ContainingString = 20
        }

        public enum RemoveBrainType
        {
            ThisBrain = 0,
            WithName = 10,
            ContainingString = 20,
            All = 30
        }

        public enum ElementDisplayType
        {
            Show = 0,
            Hide = 1
        }

        public ChangeBrainInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
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

        private bool ModifyAllChildren
        {
            get
            {
                return _includeChildren;
            }
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_brainStringName))
                _brainString = _brain.Variables.GetString(_brainStringName);


            _selectedBrainRunners.Clear();

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    SetBrainOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    SetBrainOnWholeEntity(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    SetBrainOnParents(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    SetBrainOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    SetBrainOnBody(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    SetBrainOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllChildren)
                        SetBrainOnEntityAndChildren(targetBody);
                    else if (ModifyAllAttached)
                        SetBrainOnWholeEntity(targetBody);
                    else
                        SetBrainOnBody(targetBody);
                    break;
            }

            SetBrainsOnSelectedBrainRunners();

            return Complete(InstructionTileResult.Success);
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
                case MonaBrainBroadcastType.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }

        private void SetBrainOnTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetBrainOnWholeEntity(tagBodies[i]);
                else
                    SetBrainOnBody(tagBodies[i]);
            }
        }

        private void SetBrainOnEntityAndChildren(IMonaBody body)
        {
            IMonaBody topBody = body;

            SetBrainOnBody(topBody);
            SetBrainOnChildren(topBody);
        }

        private void SetBrainOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetBrainOnBody(topBody);
            SetBrainOnChildren(topBody);
        }

        private void SetBrainOnParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetBrainOnBody(parent);
            SetBrainOnParents(parent);
        }

        private void SetBrainOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetBrainOnBody(children[i]);
                SetBrainOnChildren(children[i]);
            }
        }

        private void SetBrainOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetBrainOnWholeEntity(spawned[i]);
                else
                    SetBrainOnBody(spawned[i]);
            }
        }

        private void SetBrainOnBody(IMonaBody body)
        {
            MonaBrainRunner brainRunner = body.Transform.GetComponent<MonaBrainRunner>();

            if (_mainAction == BrainActionType.AddBrain && brainRunner == null)
                brainRunner = body.Transform.gameObject.AddComponent<MonaBrainRunner>();

            if (brainRunner == null)
                return;

            _selectedBrainRunners.Add(brainRunner);
        }

        private void SetBrainsOnSelectedBrainRunners()
        {
            int thisRunnerIndex = -1;

            if (_mainAction == BrainActionType.AddBrain)
                SetBrainGraphsToAdd();

            for (int i = 0; i < _selectedBrainRunners.Count; i++)
            {
                if (_mainAction == BrainActionType.AddBrain)
                {
                    AddBrains(_selectedBrainRunners[i]);
                }
                else
                {
                    if ((IMonaBrainRunner)_selectedBrainRunners[i] == _brain.Runner)
                    {
                        thisRunnerIndex = i;
                        continue;
                    }

                    RemoveBrains(_selectedBrainRunners[i]);
                }       
            }

            if (_mainAction == BrainActionType.RemoveBrain && thisRunnerIndex >= 0)
                RemoveBrains(_selectedBrainRunners[thisRunnerIndex]);
        }

        private void AddBrains(MonaBrainRunner runner)
        {
            for (int i = 0; i < _brainGraphsToAdd.Count; i++)
                runner.AddBrainGraph((MonaBrainGraph)_brainGraphsToAdd[i]);

            runner.BrainInstances.Clear();
            runner.RestartBrains();
        }

        private void RemoveBrains(MonaBrainRunner runner)
        {
            Debug.Log($"{nameof(RemoveBrains)} {runner.Body.Transform.name}");
            switch (_brainsToRemove)
            {
                case RemoveBrainType.ThisBrain:
                    runner.RemoveBrainGraph((MonaBrainGraph)_brain);
                    break;
                case RemoveBrainType.WithName:
                    runner.RemoveBrainGraph(_brainString);
                    break;
                case RemoveBrainType.ContainingString:
                    runner.RemoveBrainGraph(_brainString, true);
                    break;
                case RemoveBrainType.All:
                    runner.RemoveAllBrainGraphs();
                    break;
            }

            runner.RestartBrains();
        }

        private void SetBrainGraphsToAdd()
        {
            _brainGraphsToAdd.Clear();

            if (_brainsToAdd == AddBrainType.ThisBrain)
            {
                _brainGraphsToAdd.Add((MonaBrainGraph)_brain);
                return;
            }

            _availableBrainGraphs = MonaGlobalBrainRunner.Instance.PlayerBrainGraphs;

            var resourceGraphs = Resources.FindObjectsOfTypeAll<MonaBrainGraph>();
            for (var i = 0; i < resourceGraphs.Length; i++)
            {
                if (_availableBrainGraphs.Contains(resourceGraphs[i]))
                    continue;

                _availableBrainGraphs.Add(resourceGraphs[i]);
            }

            for (int i = 0; i < _availableBrainGraphs.Count; i++)
            {
                if (_brainGraphsToAdd.Contains(_availableBrainGraphs[i]))
                    continue;

                switch (_brainsToAdd)
                {
                    case AddBrainType.WithName:
                        if (_availableBrainGraphs[i].Name == _brainString)
                            _brainGraphsToAdd.Add(_availableBrainGraphs[i]);
                        break;
                    case AddBrainType.ContainingString:
                        if (_availableBrainGraphs[i].Name.Contains(_brainString))
                            _brainGraphsToAdd.Add(_availableBrainGraphs[i]);
                        break;
                }
            }
        }

        public override void Unload(bool destroy = false)
        {
            base.Unload();
        }

    }
}