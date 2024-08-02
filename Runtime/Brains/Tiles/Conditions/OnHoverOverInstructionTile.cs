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

        [SerializeField] private string _targetArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string TargetArray { get => _targetArray; set => _targetArray = value; }

        [SerializeField] private MonaBrainHovererTargetType _hoverer = MonaBrainHovererTargetType.MousePointer;
        [BrainPropertyEnum(true)] public MonaBrainHovererTargetType Hoverer { get => _hoverer; set => _hoverer = value; }

        [SerializeField] private string _hovererTag = "Default";
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.Tag)]
        [BrainPropertyMonaTag(true)] public string HovererTag { get => _hovererTag; set => _hovererTag = value; }

        [SerializeField] private string _hovererArray;
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string HovererArray { get => _hovererArray; set => _hovererArray = value; }

        [SerializeField] private Vector3 _worldPosition;
        [SerializeField] private string[] _worldPositionName;
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.WorldPosition)]
        [BrainProperty(true)] public Vector3 WorldPosition { get => _worldPosition; set => _worldPosition = value; }
        [BrainPropertyValueName("MyVector3", typeof(IMonaVariablesVector3Value))] public string[] WorldPositionName { get => _worldPositionName; set => _worldPositionName = value; }

        [SerializeField] private bool _negate;
        [SerializeField] private string _negateName;
        [BrainProperty(true)] public bool Negate { get => _negate; set => _negate = value; }
        [BrainPropertyValueName("Negate", typeof(IMonaVariablesBoolValue))] public string NegateName { get => _negateName; set => _negateName = value; }

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;
        [BrainPropertyShow(nameof(Hoverer), (int)MonaBrainHovererTargetType.MousePointer)]
        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }

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
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }
        [BrainPropertyValueName("IncludeAttached", typeof(IMonaVariablesBoolValue))] public string IncludeAttachedName { get => _includeAttachedName; set => _includeAttachedName = value; }

        private const float _minBounds = 0.0001f;

        private bool HoverAllAttached
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

            if (!string.IsNullOrEmpty(_negateName))
                _negate = _brain.Variables.GetBool(_negateName);

            bool targetIsHoveredOver = TargetIsHoveredOver();
            bool success = (targetIsHoveredOver && !_negate) || (!targetIsHoveredOver && _negate);

            if (success)
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
                        return TargetIsHoveredOver(brain.Body);
                    break;
                case MonaBrainBroadcastType.OnConditionTarget:
                    return TargetIsHoveredOver(_brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET));
                case MonaBrainBroadcastType.OnSelectTarget:
                    return TargetIsHoveredOver(_brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET));
                case MonaBrainBroadcastType.MySpawner:
                    return TargetIsHoveredOver(_brain.Body.Spawner);
                case MonaBrainBroadcastType.LastSpawnedByMe:
                    return TargetIsHoveredOver(_brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED));
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return TargetIsHoveredOver(_brain.Body.PoolBodyPrevious);
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return TargetIsHoveredOver(_brain.Body.PoolBodyNext);
                case MonaBrainBroadcastType.MyBodyArray:
                    return BodyArrayIsHoveredOver();
            }

            return false;
        }

        private bool TargetIsHoveredOver(IMonaBody body)
        {
            return HoverAllAttached ? WholeTargetEntityIsHoveredOver(body) : IsHoveredOver(body);
        }

        private bool TargetTagIsHoveredOver()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return false;

            bool hoverOverAll = HoverAllAttached;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (!hoverOverAll && IsHoveredOver(tagBodies[i]))
                    return true;

                if (hoverOverAll && WholeTargetEntityIsHoveredOver(tagBodies[i]))
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

        private bool BodyArrayIsHoveredOver()
        {
            var bodyArray = _brain.Variables.GetBodyArray(_targetArray);

            if (bodyArray.Count < 1)
                return false;

            bool hoverOverAll = HoverAllAttached;

            for (int i = 0; i < bodyArray.Count; i++)
            {
                if (bodyArray[i] == null)
                    continue;

                if (!hoverOverAll && IsHoveredOver(bodyArray[i]))
                    return true;

                if (hoverOverAll && WholeTargetEntityIsHoveredOver(bodyArray[i]))
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
            var pos = Vector3.zero;
            if (Mouse.current != null)
                pos = Mouse.current.position.ReadValue();

            Ray ray;
            if (MonaGlobalBrainRunner.Instance.PlayerCamera != null)
                ray = MonaGlobalBrainRunner.Instance.PlayerCamera.ScreenPointToRay(pos);
            else if (Camera.main != null)
                ray = Camera.main.ScreenPointToRay(pos);
            else
                return false;
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit, _distance))
            {
                bool found = false;
                Transform t = hit.collider.transform;
                while (t != null)
                {
                    if (t == targetBody.Transform)
                    {
                        found = true;
                        break;
                    }
                    t = t.parent;
                }

                if (found)
                {
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_TARGET, targetBody);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_POINT, hit.point);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_NORMAL, hit.normal);
                    return true;
                }
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

        private bool TagArrayIsOver(IMonaBody targetBody)
        {
            var bodyArray = _brain.Variables.GetBodyArray(_targetArray);

            if (bodyArray.Count < 1)
                return false;

            for (int i = 0; i < bodyArray.Count; i++)
            {
                if (bodyArray[i] == null)
                    continue;

                return BodyIsOver(targetBody, bodyArray[i]);
            }

            return false;
        }

        private bool BodyIsOver(IMonaBody targetBody, IMonaBody occluderBody)
        {
            if (targetBody == null || occluderBody == null)
                return false;

            bool bodyIsOver = OccluderIsInFront(targetBody, occluderBody) && BodiesOverlap(targetBody, occluderBody);

            if (bodyIsOver)
                _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_TARGET, targetBody);

            return bodyIsOver;
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