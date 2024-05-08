using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class TeleportToPositionInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "TeleportToPositionInstructionTile";
        public const string NAME = "Teleport To";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(TeleportToPositionInstructionTile);

        [SerializeField] private MonaBrainTransformType _target = MonaBrainTransformType.WorldSpace;
        [BrainPropertyEnum(true)] public MonaBrainTransformType Target { get => _target; set => _target = value; }

        [SerializeField] private MonaBrainSelfTransformType _targetPosition = MonaBrainSelfTransformType.CurrentWorld;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformType.Self)]
        [BrainPropertyEnum(true)] public MonaBrainSelfTransformType TargetPosition { get => _targetPosition; set => _targetPosition = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTransformType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private Vector3 _value;
        [SerializeField] private string[] _valueValueName = new string[4];
        [BrainPropertyShow(nameof(OffsetType), (int)ObjectOffsetType.NoOffset)]
        [BrainProperty(true)] public Vector3 Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesVector3Value))] public string[] ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private Vector3 _offset;
        [SerializeField] private string[] _offsetName = new string[4];
        [BrainPropertyShow(nameof(OffsetType), (int)ObjectOffsetType.UseOffset)]
        [BrainProperty(true)] public Vector3 Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesVector3Value))] public string[] OffsetName { get => _offsetName; set => _offsetName = value; }

        public ObjectOffsetType OffsetType =>
            _target == MonaBrainTransformType.WorldSpace || _target == MonaBrainTransformType.LocalSpace ?
            ObjectOffsetType.NoOffset :
            ObjectOffsetType.UseOffset;

        private IMonaBrain _brain;

        public TeleportToPositionInstructionTile() { }

        public enum SelfPositionType
        {
            CurrentWorld,
            CurrentLocal,
            InitialWorld,
            InitialLocal
        }

        public enum ObjectOffsetType
        {
            NoOffset = 0,
            UseOffset = 10
        }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (HasVector3Values(_valueValueName))
                _value = GetVector3Value(_brain, _valueValueName);

            if (HasVector3Values(_offsetName))
                _offset = GetVector3Value(_brain, _offsetName);

            if (OffsetType == ObjectOffsetType.NoOffset)
            {
                _brain.Body.TeleportPosition(_value, true, _target == MonaBrainTransformType.LocalSpace);
                return Complete(InstructionTileResult.Success);
            }

            GetTargetPosition(out Vector3 position, out bool targetFound, out bool useLocalSpace);

            if (!targetFound)
                return Complete(InstructionTileResult.Success);

            _brain.Body.TeleportPosition(position + _offset, true, useLocalSpace);
            return Complete(InstructionTileResult.Success);
        }

        private void GetTargetPosition(out Vector3 position, out bool targetFound, out bool useLocalSpace)
        {
            useLocalSpace = false;

            switch (_target)
            {
                case MonaBrainTransformType.Self:
                    useLocalSpace = _targetPosition == MonaBrainSelfTransformType.CurrentLocal || _targetPosition == MonaBrainSelfTransformType.InitialLocal;
                    position = GetMyPosition();
                    targetFound = true;
                    return;
                case MonaBrainTransformType.Parent:
                    Transform parent = _brain.Body.Transform.parent;
                    if (!parent)
                        break;
                    position = parent.position;
                    targetFound = true;
                    return;
                case MonaBrainTransformType.Child:
                    Transform child = _brain.Body.Transform.GetComponentInChildren<Transform>(true);
                    if (!child)
                        break;
                    position = child.position;
                    targetFound = true;
                    return;
                default:
                    var targetBody = GetTargetBody();
                    if (targetBody == null)
                        break;
                    position = targetBody.GetPosition();
                    targetFound = true;
                    return;
            }

            position = Vector3.zero;
            targetFound = false;
        }

        private Vector3 GetMyPosition()
        {
            switch (_targetPosition)
            {
                case MonaBrainSelfTransformType.CurrentLocal:
                    return _brain.Body.Transform.localPosition;
                case MonaBrainSelfTransformType.InitialWorld:
                    return _brain.Body.InitialPosition;
                case MonaBrainSelfTransformType.InitialLocal:
                    return _brain.Body.InitialLocalPosition;
                default:
                    return _brain.Body.GetPosition();
            }
        }

        private IMonaBody GetTargetBody()
        {
            switch (_target)
            {
                case MonaBrainTransformType.Tag: return _brain.Body.GetClosestTag(_targetTag);
                case MonaBrainTransformType.OnConditionTarget: return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainTransformType.OnHitTarget: return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainTransformType.MySpawner: return _brain.Body.Spawner;
                case MonaBrainTransformType.LastSpawnedByMe: return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainTransformType.MyPoolPreviouslySpawned: return _brain.Body.PoolBodyPrevious;
                case MonaBrainTransformType.MyPoolNextSpawned: return _brain.Body.PoolBodyNext;
                case MonaBrainTransformType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    return brain != null ? brain.Body : null;
                case MonaBrainTransformType.AnySpawnedByMe:
                    var spawned = _brain.SpawnedBodies;
                    return spawned.Count > 0 ? spawned[UnityEngine.Random.Range(0, spawned.Count)] : null;
            }
            return null;
        }
    }
}