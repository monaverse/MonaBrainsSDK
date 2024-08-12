using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;
using System.Text;
using System.Collections.Generic;
using Mona.SDK.Core.Body.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ChangeColliderInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "ChangeCollider";
        public const string NAME = "Change Collider";
        public const string CATEGORY = "Body Actions";
        public override Type TileType => typeof(ChangeColliderInstructionTile);

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _targetChild;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(true)] public string TargetName { get => _targetChild; set => _targetChild = value; }

        [SerializeField] private bool _networkNewBodies;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenWithName)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.ChildrenContainingName)]
        [BrainProperty(true)] public bool NetworkNewBodies { get => _networkNewBodies; set => _networkNewBodies = value; }

        [SerializeField] private ColliderModificationType _addOrRemove = ColliderModificationType.Add;
        [BrainPropertyEnum(true)] public ColliderModificationType AddOrRemove { get => _addOrRemove; set => _addOrRemove = value; }

        [SerializeField] private MonaBodyColliderType _colliderType = MonaBodyColliderType.Box;
        [BrainPropertyEnum(true)] public MonaBodyColliderType ColliderType { get => _colliderType; set => _colliderType = value; }

        [SerializeField]
        private float _radius = 1f;

        [SerializeField] private string _radiusValueName;
        [BrainPropertyShow(nameof(ColliderType), (int)MonaBodyColliderType.Sphere)]
        [BrainProperty] public float Radius { get => _radius; set => _radius = value; }
        [BrainPropertyValueName(nameof(Radius), typeof(IMonaVariablesFloatValue))] public string RadiusValueName { get => _radiusValueName; set => _radiusValueName = value; }

        [SerializeField] private bool _isTrigger = false;
        [BrainPropertyEnum(false)] public bool IsTrigger { get => _isTrigger; set => _isTrigger = value; }

        [SerializeField] private bool _onlyRenderers = true;
        [BrainPropertyEnum(true)] public bool OnlyRenderers { get => _onlyRenderers; set => _onlyRenderers = value; }

        [SerializeField] private bool _skipIfExists = false;
        [BrainPropertyShow(nameof(AddOrRemove), (int)ColliderModificationType.Add)]
        [BrainPropertyEnum(false)] public bool SkipIfExists { get => _skipIfExists; set => _skipIfExists = value; }

        public string Tag { get => _targetTag; set => _targetTag = value; }

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

        private IMonaBrain _brain;
        private string _stateProperty;

        public enum ColliderModificationType { Add, RemoveAll }

        public ChangeColliderInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
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
                    case MonaBrainBroadcastType.ChildrenWithName:
                    case MonaBrainBroadcastType.ChildrenContainingName:
                        return false;
                    case MonaBrainBroadcastType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        public override InstructionTileResult Do()
        {
            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    ModifyOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    ModifyOnWholeEntity(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    ModifyOnParents(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    ModifyOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ChildrenWithName:
                    ModifyOnChildrenWithName(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ChildrenContainingName:
                    ModifyOnChildrenContainingName(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    AddColliderToBody(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    ModifyOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        ModifyOnWholeEntity(targetBody);
                    else
                        AddColliderToBody(targetBody);

                    break;
            }

            //_brain.AddCollider(_tag);
            //Log();
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }

        private void Log()
        {
            if (_brain.LoggingEnabled)
            {
                var builder = new StringBuilder();
                for (var i = 0; i < _brain.Body.MonaTags.Count; i++)
                    builder.Append($"Body Tag: {i} {_brain.Body.MonaTags[i]}\n");
                for (var i = 0; i < _brain.MonaTags.Count; i++)
                    builder.Append($"Brain Tag: {i} {_brain.MonaTags[i]}\n");

                Debug.Log($"{nameof(ChangeColliderInstructionTile)} {_brain.Body.MonaTags.Count + _brain.MonaTags.Count} tags\n-----------------\n {builder.ToString()}");
            }
        }

        private void ModifyOnTag()
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
                    ModifyOnWholeEntity(tagBodies[i]);
                else
                    AddColliderToBody(tagBodies[i]);
            }
        }

        private void ModifyOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            AddColliderToBody(topBody);
            ModifyOnChildren(topBody);
        }

        private void ModifyOnParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
            {
                return;
            }

            AddColliderToBody(parent);
            ModifyOnParents(parent);
        }

        private void ModifyOnChildren(IMonaBody body)
        {

            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                AddColliderToBody(children[i]);
                ModifyOnChildren(children[i]);
            }

        }


        private List<Transform> _children = new List<Transform>();
        private void ModifyOnChildrenWithName(IMonaBody body)
        {
            _children.Clear();
            _children.AddRange(body.Transform.GetComponentsInChildren<Transform>(true));
            _children.Remove(body.Transform);

            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                if (child == null || child.name.ToLower() != _targetChild.ToLower())
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if(childBody == null)
                    childBody = CreateMonaBody(child);

                AddColliderToBody(childBody);
            }

        }

        private void ModifyOnChildrenContainingName(IMonaBody body)
        {
            _children.Clear();
            _children.AddRange(body.Transform.GetComponentsInChildren<Transform>(true));
            _children.Remove(body.Transform);

            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                if (child == null || !child.name.ToLower().Contains(_targetChild.ToLower()))
                    continue;

                var childBody = child.GetComponent<IMonaBody>();
                if (childBody == null)
                    childBody = CreateMonaBody(child);

                AddColliderToBody(childBody);
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

        private void ModifyOnAllSpawned()
        {

            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    ModifyOnWholeEntity(spawned[i]);
                else
                    AddColliderToBody(spawned[i]);
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
            }
            return null;
        }


        private void AddColliderToBody(IMonaBody body)
        {
            if (body== null)
                return;

            if (_addOrRemove == ColliderModificationType.Add)
            {
                var colliders = body.AddCollider(_colliderType, _onlyRenderers, _skipIfExists);
                if(_colliderType == MonaBodyColliderType.Sphere)
                { 
                    if (!string.IsNullOrEmpty(_radiusValueName))
                        _radius = _brain.Variables.GetFloat(_radiusValueName);

                    for (var i = 0; i < colliders.Count; i++)
                        ((SphereCollider)colliders[i]).radius = _radius;
                }

                if (_isTrigger)
                {
                    for (var i = 0; i < colliders.Count; i++)
                        colliders[i].isTrigger = _isTrigger;
                }
            }
            else
                body.RemoveColliders();

        }
    }
}