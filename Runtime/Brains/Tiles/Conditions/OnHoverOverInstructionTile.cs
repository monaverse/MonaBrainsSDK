using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Core.Tiles;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnHoverOverInstructionTile : OnInteractInstructionTile, ITickAfterInstructionTile
    {
        public new const string ID = "OnHoverOver";
        public new const string NAME = "On Screen Hover";
        public new const string CATEGORY = "Input";
        public override Type TileType => typeof(OnHoverOverInstructionTile);

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.Self;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag = "Player";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private MonaBrainHovererTargetType _hoverer = MonaBrainHovererTargetType.MousePointer;
        [BrainPropertyEnum(true)] public MonaBrainHovererTargetType Hoverer { get => _hoverer; set => _hoverer = value; }

        [SerializeField] private string _hovererTag = "Default";
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.Tag)]
        [BrainPropertyMonaTag(true)] public string HovererTag { get => _hovererTag; set => _hovererTag = value; }

        [SerializeField] private Vector3 _worldPosition;
        [SerializeField] private string[] _worldPositionName;
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.WorldPosition)]
        [BrainProperty(true)] public Vector3 WorldPosition { get => _worldPosition; set => _worldPosition = value; }
        [BrainPropertyValueName("MyVector3", typeof(IMonaVariablesVector3Value))] public string[] WorldPositionName { get => _worldPositionName; set => _worldPositionName = value; }

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.MousePointer)]
        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }

        private const float _minBounds = 0.0001f;

        protected override MonaInputState GetInputState() => MonaInputState.Pressed;

        public override void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            base.Preload(brainInstance, page, instruction);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_distanceValueName))
                _distance = _brain.Variables.GetFloat(_distanceValueName);

            if (TargetIsHoveredOver())
                return Complete(InstructionTileResult.Success);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }

        private bool TargetIsHoveredOver()
        {
            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    return TargetTagIsHoveredOver();
                case MonaBrainBroadcastType.Self:
                    return WholeTargetEntityIsHoveredOver(_brain.Body);
                case MonaBrainBroadcastType.Parent:
                    return IsHoveredOver(_brain.Body.Parent);
                case MonaBrainBroadcastType.Parents:
                    return TargetParentsAreHoveredOver(_brain.Body);
                case MonaBrainBroadcastType.Children:
                    return TargetChildrenAreHoveredOver(_brain.Body);
                case MonaBrainBroadcastType.ThisBodyOnly:
                    return IsHoveredOver(_brain.Body);
                case MonaBrainBroadcastType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return IsHoveredOver(brain.Body);
                    break;
                case MonaBrainBroadcastType.OnConditionTarget:
                    return IsHoveredOver(_brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET));
                case MonaBrainBroadcastType.OnSelectTarget:
                    return IsHoveredOver(_brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET));
                case MonaBrainBroadcastType.MySpawner:
                    return IsHoveredOver(_brain.Body.Spawner);
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return IsHoveredOver(_brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED));
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return IsHoveredOver(_brain.Body.PoolBodyPrevious);
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return IsHoveredOver(_brain.Body.PoolBodyNext);
            }

            return false;
        }

        private bool TargetTagIsHoveredOver()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return false;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (IsHoveredOver(tagBodies[i]))
                    return true;
            }

            return false;
        }

        private bool WholeTargetEntityIsHoveredOver(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            if (IsHoveredOver(topBody) || TargetChildrenAreHoveredOver(topBody))
                return true;

            return false;
        }

        private bool TargetParentsAreHoveredOver(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return false;

            if (IsHoveredOver(parent) || TargetParentsAreHoveredOver(parent))
                return true;

            return false;
        }

        private bool TargetChildrenAreHoveredOver(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                if (IsHoveredOver(children[i]) || TargetChildrenAreHoveredOver(children[i]))
                    return true;
            }

            return false;
        }

        private bool IsHoveredOver(IMonaBody targetBody)
        {
            switch (_hoverer)
            {
                case MonaBrainHovererTargetType.Tag:
                    return TagIsOver(targetBody);
                case MonaBrainHovererTargetType.MousePointer:
                    return MouseIsOver(targetBody);
                case MonaBrainHovererTargetType.WorldPosition:
                    // MUST ADD IN WORLD POSITION HANDLING
                    break;
                case MonaBrainHovererTargetType.MySpawner:
                    return BodyIsOver(targetBody, _brain.Body.Spawner);
                case MonaBrainHovererTargetType.LastSpawnedByMe:
                    return BodyIsOver(targetBody, _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED));
                case MonaBrainHovererTargetType.MyPoolPreviouslySpawned:
                    return BodyIsOver(targetBody, _brain.Body.PoolBodyPrevious);
                case MonaBrainHovererTargetType.MyPoolNextSpawned:
                    return BodyIsOver(targetBody, _brain.Body.PoolBodyNext);
            }

            return false;
        }

        private bool MouseIsOver(IMonaBody targetBody)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, _distance))
            {
                if (hit.transform == targetBody.Transform)
                    return true;
            }

            return false;
        }

        private bool TagIsOver(IMonaBody targetBody)
        {
            var tagBodies = MonaBody.FindByTag(_hovererTag);
            if (tagBodies.Count < 1)
                return false;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                return BodyIsOver(targetBody, tagBodies[i]);
            }

            return false;
        }

        private bool BodyIsOver(IMonaBody targetBody, IMonaBody occluderBody)
        {
            if (targetBody == null || occluderBody == null)
                return false;

            return OccluderIsInFront(targetBody, occluderBody) && BodiesOverlap(targetBody, occluderBody);
        }

        private bool BodiesOverlap(IMonaBody targetBody, IMonaBody occluderBody)
        {
            if (targetBody == null || occluderBody == null)
                return false;

            Bounds targetBounds = GetBodyBounds(targetBody);
            Bounds occluderBounds = GetBodyBounds(occluderBody);

            Vector3 targetScreenMin = Camera.main.WorldToScreenPoint(targetBounds.min);
            Vector3 targetScreenMax = Camera.main.WorldToScreenPoint(targetBounds.max);
            Vector3 occluderScreenMin = Camera.main.WorldToScreenPoint(occluderBounds.min);
            Vector3 occluderScreenMax = Camera.main.WorldToScreenPoint(occluderBounds.max);

            Rect targetScreenRect = new Rect(targetScreenMin.x, targetScreenMin.y, targetScreenMax.x - targetScreenMin.x, targetScreenMax.y - targetScreenMin.y);
            Rect checkScreenRect = new Rect(occluderScreenMin.x, occluderScreenMin.y, occluderScreenMax.x - occluderScreenMin.x, occluderScreenMax.y - occluderScreenMin.y);

            return targetScreenRect.Overlaps(checkScreenRect);
        }

        private Bounds GetBodyBounds(IMonaBody body)
        {
            if (body == null)
                return DefaultBounds(null);

            Collider collider = body.Transform.GetComponentInParent<Collider>();

            if (collider != null)
                return collider.bounds;

            Renderer renderer = body.Transform.GetComponentInParent<Renderer>();

            if (renderer != null)
                return renderer.bounds;

            return DefaultBounds(body);
        }

        private Bounds DefaultBounds(IMonaBody body)
        {
            if (body == null)
                return new Bounds(Vector3.zero, Vector3.zero);

            Vector3 position = body.GetPosition();
            Vector3 min = new Vector3(position.x - _minBounds, position.y - _minBounds, position.z - _minBounds);
            Vector3 max = new Vector3(position.x + _minBounds, position.y + _minBounds, position.z + _minBounds);

            return new Bounds(min, max);
        }

        private bool OccluderIsInFront(IMonaBody targetBody, IMonaBody occluderBody)
        {
            if (targetBody == null || occluderBody == null)
                return false;

            Vector3 cameraPosition = Camera.main.transform.position;

            Vector3 directionToTarget = targetBody.GetPosition() - cameraPosition;
            Vector3 directionToOccluder = occluderBody.GetPosition() - cameraPosition;

            return directionToOccluder.magnitude < directionToTarget.magnitude;
        }
    }
}