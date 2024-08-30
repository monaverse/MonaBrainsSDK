using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.Variables.Interfaces;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Tiles.Actions.Variables.Enums;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Variables
{
    [Serializable]
    public class ChangeBodyArrayInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IOnStartInstructionTile
    {
        public const string ID = "ChangeBodyArrayValue";
        public const string NAME = "Change Body Array";
        public const string CATEGORY = "Values";
        public override Type TileType => typeof(ChangeBodyArrayInstructionTile);

        [SerializeField] private ArrayActionType _action = ArrayActionType.Add;
        [BrainPropertyEnum(true)] public ArrayActionType ActionType { get => _action; set => _action = value; }

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Tag;
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.Add)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.Remove)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.AddOrRemove)]
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.Add)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.Remove)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.AddOrRemove)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _childString;
        [SerializeField] private string _childStringName;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(true)] public string ChildString { get => _childString; set => _childString = value; }
        [BrainPropertyValueName("ChildString", typeof(IMonaVariablesStringValue))] public string ChildStringName { get => _childStringName; set => _childStringName = value; }

        [SerializeField] private string _targetArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string TargetArray { get => _targetArray; set => _targetArray = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private bool _networkNewBodies;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(true)] public bool NetworkNewBodies { get => _networkNewBodies; set => _networkNewBodies = value; }

        [SerializeField] private bool _modifyAllAttached;
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.Add)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.Remove)]
        [BrainPropertyShow(nameof(ActionType), (int)ArrayActionType.AddOrRemove)]
        [BrainProperty(false)] public bool ModifyAllAttached { get => _modifyAllAttached; set => _modifyAllAttached = value; }

        private IMonaBrain _brain;

        public ChangeBodyArrayInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain != null)
            {

                if (!string.IsNullOrEmpty(_childStringName))
                    _childString = _brain.Variables.GetString(_childStringName);

                var myValue = _brain.Variables.GetVariable(_bodyArray);

                if(_action == ArrayActionType.Clear)
                {
                    ModifyValueOnBody(myValue, _brain.Body);
                    return Complete(InstructionTileResult.Success);
                }

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
                    case MonaBrainBroadcastType.ChildrenWithName:
                        ModifyOnChildrenWithName(myValue, _brain.Body);
                        break;
                    case MonaBrainBroadcastType.ChildrenContainingName:
                        ModifyOnChildrenContainingName(myValue, _brain.Body);
                        break;
                    case MonaBrainBroadcastType.ThisBodyOnly:
                        ModifyValueOnBody(myValue, _brain.Body);
                        break;
                    case MonaBrainBroadcastType.AllSpawnedByMe:
                        ModifyOnAllSpawned(myValue);
                        break;
                    case MonaBrainBroadcastType.MyBodyArray:
                        ModifyOnBodyArray(myValue);
                        break;
                    default:
                        IMonaBody targetBody = GetTarget();

                        if (targetBody == null)
                            break;

                        if (ModifyAllAttached)
                            ModifyOnWholeEntity(myValue, targetBody);
                        else
                            ModifyValueOnBody(myValue, targetBody);

                        break;
                }

                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private void ModifyOnTag(IMonaVariablesValue myValue)
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
            {
                return;
            }

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(myValue, tagBodies[i]);
                else
                    ModifyValueOnBody(myValue, tagBodies[i]);
            }
        }

        private void ModifyOnWholeEntity(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ModifyValueOnBody(myValue, topBody);
            ModifyOnChildren(myValue, topBody);
        }

        private void ModifyOnParents(IMonaVariablesValue myValue, IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
            {
                return;
            }

            ModifyValueOnBody(myValue, parent);
            ModifyOnParents(myValue, parent);
        }

        private void ModifyOnChildren(IMonaVariablesValue myValue, IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                ModifyValueOnBody(myValue, children[i]);
                ModifyOnChildren(myValue, children[i]);
            }
        }

        private void ModifyOnChildrenWithName(IMonaVariablesValue myValue, IMonaBody body)
        {

            var children = body.Transform.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null || child.name.ToLower() != _childString.ToLower())
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if (childBody == null)
                    childBody = CreateMonaBody(child);

                ModifyValueOnBody(myValue, childBody);
            }

        }

        private void ModifyOnChildrenContainingName(IMonaVariablesValue myValue, IMonaBody body)
        {

            var children = body.Transform.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child == null || !child.name.ToLower().Contains(_childString.ToLower()))
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if (childBody == null)
                    childBody = CreateMonaBody(child);

                ModifyValueOnBody(myValue, childBody);
            }

        }

        private IMonaBody CreateMonaBody(Transform body)
        {
            var childBody = body.gameObject.AddComponent<MonaBody>();
            childBody.SyncType = MonaBodyNetworkSyncType.NotNetworked;
            if (_networkNewBodies)
            {
                childBody.SyncType = MonaBodyNetworkSyncType.NetworkTransform;

                var guid = Guid.NewGuid();
                ((MonaBodyBase)childBody).Guid = new SerializableGuid(guid);
                ((MonaBodyBase)childBody).ManualMakeUnique(guid.ToString(), 0, 0, false);
            }
            return childBody;
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
                    ModifyValueOnBody(myValue, spawned[i]);
            }
        }

        private void ModifyOnBodyArray(IMonaVariablesValue myValue)
        {
            var arrayBodies = _brain.Variables.GetBodyArray(_targetArray);

            for (int i = 0; i < arrayBodies.Count; i++)
            {
                if (arrayBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(myValue, arrayBodies[i]);
                else
                    ModifyValueOnBody(myValue, arrayBodies[i]);
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
                case MonaBrainBroadcastType.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
            }
            return null;
        }

        private void ModifyValueOnBody(IMonaVariablesValue myValue, IMonaBody target)
        {
            var value = ((IMonaVariablesBodyArrayValue)myValue).Value;
            switch (_action)
            {
                case ArrayActionType.Add:
                    if (!value.Contains(target))
                        value.Add(target);
                    break;
                case ArrayActionType.Remove:
                    if (value.Contains(target))
                        value.Remove(target);
                    break;
                case ArrayActionType.AddOrRemove:
                    if (!value.Contains(target))
                        value.Add(target);
                    else if (value.Contains(target))
                        value.Remove(target);
                    break;
                case ArrayActionType.Clear:
                    value.Clear();
                    break;
            }
            //Debug.Log($"{nameof(ChangeBodyArrayInstructionTile)} {myValue.Name} {value.Count}");
            myValue.Change();
        }
    }
}