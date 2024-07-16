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
using static Mona.SDK.Brains.Tiles.Conditions.Behaviours.SphereColliderTriggerBehaviour;

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

        [SerializeField] private string _tag = "Default";
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

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;
        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValue { get => _distanceValueName; set => _distanceValueName = value; }

        [SerializeField] private SpaceType _space = SpaceType.Local;
        [BrainPropertyEnum(false)] public SpaceType Space { get => _space; set => _space = value; }

        private IMonaBrain _brain;
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

            if (!string.IsNullOrEmpty(_distanceValueName))
                _distance = _brain.Variables.GetFloat(_distanceValueName);

            if (!string.IsNullOrEmpty(_negateName))
                _negate = _brain.Variables.GetBool(_negateName);

            bool result = _negate ? !TargetFoundInDirection(_brain.Body) : TargetFoundInDirection(_brain.Body);

            if (_brain == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            return result ?
                Complete(InstructionTileResult.Success) :
                Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }

        private bool TargetFoundInDirection(IMonaBody body)
        {
            _bodyLayers.Clear();
            SetOriginalBodyLayers(body);
            SetBodyLayer(body, LayerMask.NameToLayer(_ignoreRaycastLayer));

            Vector3 direction = _space == SpaceType.Local ?
                body.Transform.TransformDirection(TrueDirection) :
                TrueDirection;

            Ray ray = new Ray(body.GetPosition(), direction);
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(ray, out hit, _distance, _checkLayerMask))
            {
                IMonaBody hitBody = hit.transform.GetComponent<IMonaBody>();

                ResetOriginalBodyLayers(body);
                return hitBody == null || !hitBody.HasMonaTag(_tag) ? false : true;
            }

            ResetOriginalBodyLayers(body);
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