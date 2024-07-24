using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Behaviours;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnBodyInDirectionInstructionTile : InstructionTile, IInstructionTileWithPreload,
        IConditionInstructionTile, IOnStartInstructionTile, IStartableInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "OnBodyInDirection";
        public const string NAME = "Body In Direction";
        public const string CATEGORY = "Vision";
        public override Type TileType => typeof(OnBodyInDirectionInstructionTile);

        [SerializeField] private MonaBrainBroadcastTypeSingleTarget _target = MonaBrainBroadcastTypeSingleTarget.Tag;
        [BrainPropertyEnum(true)] public MonaBrainBroadcastTypeSingleTarget Target { get => _target; set => _target = value; }

        [SerializeField] private string _tag = "Default";
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.Tag)]
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField] private BodyDirectionType _direction = BodyDirectionType.Forward;
        [BrainPropertyEnum(true)] public BodyDirectionType Direction { get => _direction; set => _direction = value; }

        [SerializeField] private Vector3 _customDirection = Vector3.forward;
        [SerializeField] private string[] _customDirectionName;
        [BrainPropertyShow(nameof(Direction), (int)BodyDirectionType.Custom)]
        [BrainProperty(true)] public Vector3 CustomDirection { get => _customDirection; set => _customDirection = value; }
        [BrainPropertyValueName("CustomDirection", typeof(IMonaVariablesVector3Value))] public string[] CustomDirectionName { get => _customDirectionName; set => _customDirectionName = value; }

        [SerializeField] private bool _negate;
        [SerializeField] private string _negateName;
        [BrainProperty(true)] public bool Negate { get => _negate; set => _negate = value; }
        [BrainPropertyValueName("Negate", typeof(IMonaVariablesBoolValue))] public string NegateName { get => _negateName; set => _negateName = value; }

        [SerializeField] private float _rangeMin = 1f;
        [SerializeField] private string _rangeMinName;
        [BrainProperty(false)] public float RangeMin { get => _rangeMin; set => _rangeMin = value; }
        [BrainPropertyValueName("RangeMin", typeof(IMonaVariablesFloatValue))] public string RangeMinName { get => _rangeMinName; set => _rangeMinName = value; }

        [SerializeField] private float _rangeMax = 100f;
        [SerializeField] private string _rangeMaxName;
        [BrainProperty(false)] public float RangeMax { get => _rangeMax; set => _rangeMax = value; }
        [BrainPropertyValueName("RangeMax", typeof(IMonaVariablesFloatValue))] public string RangeMaxName { get => _rangeMaxName; set => _rangeMaxName = value; }

        [SerializeField] private SpaceType _space = SpaceType.Local;
        [BrainPropertyEnum(false)] public SpaceType Space { get => _space; set => _space = value; }

        private IMonaBrain _brain;
        private IMonaBody _trueTargetBody;
        private string _ignoreRaycastLayer = "Ignore Raycast";
        private LayerMask _checkLayerMask;
        private List<LayerMask> _bodyLayers = new List<LayerMask>();

        private Vector3 TrueDirection
        {
            get
            {
                switch (_direction)
                {
                    case BodyDirectionType.Forward:
                        return Vector3.forward;
                    case BodyDirectionType.Back:
                        return Vector3.back;
                    case BodyDirectionType.Left:
                        return Vector3.left;
                    case BodyDirectionType.Right:
                        return Vector3.right;
                    case BodyDirectionType.Up:
                        return Vector3.up;
                    case BodyDirectionType.Down:
                        return Vector3.down;
                    default:
                        return _customDirection;
                }
            }
        }

        private List<MonaTriggerType> _triggerTypes = new List<MonaTriggerType>() { MonaTriggerType.OnTriggerEnter, MonaTriggerType.OnTriggerExit, MonaTriggerType.OnFieldOfViewChanged };
        public List<MonaTriggerType> TriggerTypes => _triggerTypes;

        public OnBodyInDirectionInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            int ignoreRaycastLayer = LayerMask.NameToLayer(_ignoreRaycastLayer);
            _checkLayerMask = ~(1 << ignoreRaycastLayer);
        }

        public override InstructionTileResult Do()
        {
            if (HasVector3Values(_customDirectionName))
                _customDirection = GetVector3Value(_brain, _customDirectionName);

            if (!string.IsNullOrEmpty(_rangeMinName))
                _rangeMin = _brain.Variables.GetFloat(_rangeMinName);

            if (!string.IsNullOrEmpty(_rangeMaxName))
                _rangeMax = _brain.Variables.GetFloat(_rangeMaxName);

            if (!string.IsNullOrEmpty(_negateName))
                _negate = _brain.Variables.GetBool(_negateName);

            if (_brain == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _trueTargetBody = GetBody();

            if (_trueTargetBody == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            bool result = _negate ? !TargetFoundInDirection(_brain.Body, _trueTargetBody) : TargetFoundInDirection(_brain.Body, _trueTargetBody);

            if (!result)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);

            _brain.Variables.Set(MonaBrainConstants.RESULT_TARGET, _trueTargetBody);
            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetBody()
        {
            switch (_target)
            {
                case MonaBrainBroadcastTypeSingleTarget.Tag:
                    return _brain.Body.GetClosestTag(_tag);
                case MonaBrainBroadcastTypeSingleTarget.Self:
                    return _brain.Body;
                case MonaBrainBroadcastTypeSingleTarget.Parent:
                    return _brain.Body.Parent;
                case MonaBrainBroadcastTypeSingleTarget.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    return brain != null ? brain.Body : null;
                case MonaBrainBroadcastTypeSingleTarget.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainBroadcastTypeSingleTarget.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainBroadcastTypeSingleTarget.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainBroadcastTypeSingleTarget.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainBroadcastTypeSingleTarget.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainBroadcastTypeSingleTarget.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
                case MonaBrainBroadcastTypeSingleTarget.LastSkin:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SKIN);
                default:
                    return null;
            }
        }

        private bool TargetFoundInDirection(IMonaBody originBody, IMonaBody targetBody)
        {
            _bodyLayers.Clear();
            SetOriginalBodyLayers(originBody);
            SetBodyLayer(originBody, LayerMask.NameToLayer(_ignoreRaycastLayer));

            Vector3 direction = _space == SpaceType.Local ?
                originBody.Transform.TransformDirection(TrueDirection) :
                TrueDirection;

            Ray ray = new Ray(originBody.GetPosition(), direction);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit, _rangeMax, _checkLayerMask))
            {
                IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                ResetOriginalBodyLayers(originBody);

                if (hitBody == null || hit.distance < _rangeMin)
                    return false;

                _trueTargetBody = hitBody;

                if (_target == MonaBrainBroadcastTypeSingleTarget.Tag)
                    return hitBody.HasMonaTag(_tag);

                return hitBody == targetBody;
            }

            ResetOriginalBodyLayers(originBody);
            return false;
        }

        private void SetOriginalBodyLayers(IMonaBody body)
        {
            Transform[] tfs = body.Transform.GetComponentsInChildren<Transform>();

            for (int i = 0; i < tfs.Length; i++)
                _bodyLayers.Add(tfs[i].gameObject.layer);
        }

        private void ResetOriginalBodyLayers(IMonaBody body)
        {
            Transform[] tfs = body.Transform.GetComponentsInChildren<Transform>();

            if (tfs.Length > _bodyLayers.Count)
                return;

            for (int i = 0; i < tfs.Length; i++)
                tfs[i].gameObject.layer = _bodyLayers[i];
        }

        private void SetBodyLayer(IMonaBody body, LayerMask layer)
        {
            Transform[] tfs = body.Transform.GetComponentsInChildren<Transform>();

            for (int i = 0; i < tfs.Length; i++)
                tfs[i].gameObject.layer = layer;
        }
    }
}