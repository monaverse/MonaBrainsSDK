using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals
{
    [Serializable]
    public class ToggleBoundingBoxInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "ToggleBoundingBox";
        public const string NAME = "ToggleBoundingBox";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(ToggleBoundingBoxInstructionTile);

        public bool IsAnimationTile => true;

        [SerializeField] private MonaBrainBroadcastType _target = MonaBrainBroadcastType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag = "Player";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private bool _display = true;
        [SerializeField] private string _displayName;
        [BrainProperty(true)] public bool Display { get => _display; set => _display = value; }
        [BrainPropertyValueName("Display", typeof(IMonaVariablesBoolValue))] public string DisplayName { get => _displayName; set => _displayName = value; }

        [SerializeField] private Color _colorValue = Color.white;
        [BrainProperty(true)] public Color ColorValue { get => _colorValue; set => _colorValue = value; }

        [SerializeField] private float _lineWidth = 0.05f;
        [SerializeField] private string _lineWidthName;
        [BrainProperty(false)] public float LineWidth { get => _lineWidth; set => _lineWidth = value; }
        [BrainPropertyValueName("LineWidth", typeof(IMonaVariablesFloatValue))] public string LineWidthName { get => _lineWidthName; set => _lineWidthName = value; }

        [SerializeField] private float _offset = 0.1f;
        [SerializeField] private string _offsetName;
        [BrainProperty(false)] public float Offset { get => _offset; set => _offset = value; }
        [BrainPropertyValueName("Offset", typeof(IMonaVariablesFloatValue))] public string OffsetName { get => _offsetName; set => _offsetName = value; }

        [SerializeField] private bool _foregrounded = false;
        [SerializeField] private string _foregroundedName;
        [BrainProperty(false)] public bool Foregrounded { get => _foregrounded; set => _foregrounded = value; }
        [BrainPropertyValueName("Foregrounded", typeof(IMonaVariablesBoolValue))] public string ForegroundedName { get => _foregroundedName; set => _foregroundedName = value; }

        [SerializeField] private bool _encompassChildren = true;
        [SerializeField] private string _encompassChildrenName;
        [BrainProperty(false)] public bool EncompassChildren { get => _encompassChildren; set => _encompassChildren = value; }
        [BrainPropertyValueName("EncompassChildren", typeof(IMonaVariablesBoolValue))] public string EncompassChildrenName { get => _encompassChildrenName; set => _encompassChildrenName = value; }

        [SerializeField] private bool _includeAttached = false;
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
        private Material _defaultMaterial;
        private Material _foregroundedMaterial;
        private List<IMonaBody> _targetBodies = new List<IMonaBody>();

        private Material LineMaterial => _foregrounded ? _foregroundedMaterial : _defaultMaterial;

        public ToggleBoundingBoxInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            _defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            _foregroundedMaterial = new Material(Shader.Find("MONA/Brains/BoundingBoxLine"));
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

        public override InstructionTileResult Do()
        {
            _targetBodies.Clear();

            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_displayName))
                _display = _brain.Variables.GetBool(_displayName);

            if (!string.IsNullOrEmpty(_lineWidthName))
                _lineWidth = _brain.Variables.GetFloat(_lineWidthName);

            if (!string.IsNullOrEmpty(_offsetName))
                _offset = _brain.Variables.GetFloat(_offsetName);

            if (!string.IsNullOrEmpty(_foregroundedName))
                _foregrounded = _brain.Variables.GetBool(_foregroundedName);

            if (!string.IsNullOrEmpty(_encompassChildrenName))
                _encompassChildren = _brain.Variables.GetBool(_encompassChildrenName);

            if (!string.IsNullOrEmpty(_includeAttachedName))
                _includeAttached = _brain.Variables.GetBool(_includeAttachedName);

            switch (_target)
            {
                case MonaBrainBroadcastType.Tag:
                    SetBoxOnTag();
                    break;
                case MonaBrainBroadcastType.Self:
                    SetBoxOnWholeEntity(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Parents:
                    SetBoxOnParents(_brain.Body);
                    break;
                case MonaBrainBroadcastType.Children:
                    SetBoxOnChildren(_brain.Body);
                    break;
                case MonaBrainBroadcastType.ThisBodyOnly:
                    SetBoxOnBody(_brain.Body);
                    break;
                case MonaBrainBroadcastType.AllSpawnedByMe:
                    SetBoxOnAllSpawned();
                    break;
                case MonaBrainBroadcastType.MyBodyArray:
                    SetBoxOnBodyArray();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        SetBoxOnWholeEntity(targetBody);
                    else
                        SetBoxOnBody(targetBody);
                    break;
            }

            ToggleBoundingBoxOnTargets();
            return Complete(InstructionTileResult.Success);
        }

        private void ToggleBoundingBoxOnTargets()
        {
            for (int i = 0; i < _targetBodies.Count; i++)
            {
                ToggleBoundingBoxOnBody(_targetBodies[i]);
            }
        }

        private void ToggleBoundingBoxOnBody(IMonaBody body)
        {
            if (body == null)
                return;

            MonaBodyBoundingBoxRenderer boxRenderer = body.Transform.GetComponentInChildren<MonaBodyBoundingBoxRenderer>(true);

            if (boxRenderer == null && _display)
                boxRenderer = CreateBoxRenderers(body);

            if (boxRenderer == null)
                return;

            IMonaBody boxParentBody = boxRenderer.transform.parent != null ? boxRenderer.transform.parent.GetComponent<IMonaBody>() : null;

            if (boxParentBody == null)
                return;

            if (boxParentBody != body)
                boxRenderer = CreateBoxRenderers(body);

            if (boxRenderer == null)
                return;

            boxRenderer.gameObject.SetActive(_display);

            if (!_display)
                return;

            SetBoxRendererParameters(boxRenderer);
        }

        private MonaBodyBoundingBoxRenderer CreateBoxRenderers(IMonaBody body)
        {
            GameObject lineObj = new GameObject("BoundingBox");
            lineObj.transform.SetParent(body.Transform);
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = LineMaterial;
            lineRenderer.startColor = _colorValue;
            lineRenderer.endColor = _colorValue;
            lineRenderer.startWidth = _lineWidth;
            lineRenderer.endWidth = _lineWidth;
            lineRenderer.positionCount = 19;
            lineRenderer.loop = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;

            MonaBodyBoundingBoxRenderer boxRenderer = lineObj.AddComponent<MonaBodyBoundingBoxRenderer>();
            SetBoxRendererParameters(boxRenderer);

            return boxRenderer;
        }

        private void SetBoxRendererParameters(MonaBodyBoundingBoxRenderer boxRenderer)
        {
            boxRenderer.BoxColor = _colorValue;
            boxRenderer.LineWidth = _lineWidth;
            boxRenderer.Offset = _offset;
            boxRenderer.EncompassChildren = _encompassChildren;
            boxRenderer.LineMaterial = LineMaterial;
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
                case MonaBrainBroadcastType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }

        private void SetBoxOnBodyArray()
        {
            var bodyArray = _brain.Variables.GetBodyArray(_bodyArray);

            for (var i = 0; i < bodyArray.Count; i++)
            {
                if (bodyArray[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetBoxOnWholeEntity(bodyArray[i]);
                else
                    SetBoxOnBody(bodyArray[i]);
            }
        }

        private void SetBoxOnTag()
        {
            var tagBodies = MonaBodyFactory.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetBoxOnWholeEntity(tagBodies[i]);
                else
                    SetBoxOnBody(tagBodies[i]);
            }
        }

        private void SetBoxOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetBoxOnBody(topBody);
            SetBoxOnChildren(topBody);
        }

        private void SetBoxOnParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetBoxOnBody(parent);
            SetBoxOnParents(parent);
        }

        private void SetBoxOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetBoxOnBody(children[i]);
                SetBoxOnChildren(children[i]);
            }
        }

        private void SetBoxOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetBoxOnWholeEntity(spawned[i]);
                else
                    SetBoxOnBody(spawned[i]);
            }
        }

        private void SetBoxOnBody(IMonaBody body)
        {
            _targetBodies.Add(body);
        }

        public override void Unload(bool destroy = false)
        {
            base.Unload();
        }

    }
}