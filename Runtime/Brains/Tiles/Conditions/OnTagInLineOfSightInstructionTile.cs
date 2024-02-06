using System;
using UnityEngine;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnTagInLineOfSightInstructionTile : InstructionTile, IOnTagInLineOfSightInstructionTile, IConditionInstructionTile
    {
        public const string ID = "OnTagInLineOfSight";
        public const string NAME = "On Tag In\nLine Of Sight";
        public const string CATEGORY = "Condition";
        public override Type TileType => typeof(OnTagInLineOfSightInstructionTile);

        [SerializeField]
        private string _originSource;

        [BrainProperty(false)]
        public string OriginSource { get => _originSource; set => _originSource = value; }

        [SerializeField]
        private string _originPart;

        [BrainProperty(false)]
        public string OriginPart { get => _originPart; set => _originPart = value; }

        [SerializeField]
        private MonaPlayerBodyParts _originPartType;

        [BrainPropertyEnum(false)]
        public MonaPlayerBodyParts OriginPartType { get => _originPartType; set => _originPartType = value; }

        [SerializeField]
        private Vector3 _direction;

        [BrainProperty(false)]
        public Vector3 Direction { get => _direction; set => _direction = value; }

        [SerializeField]
        private float _maxDistance;

        [BrainProperty(true)]
        public float MaxDistance { get => _maxDistance; set => _maxDistance = value; }

        [SerializeField]
        private string _targetMonaTag;

        [BrainProperty(false)]
        public string TargetMonaTag { get => _targetMonaTag; set => _targetMonaTag = value; }

        private IMonaBrain _brain;
        private IMonaBody _targetBody;
        private MonaPlayerBodyParts _lastPart;
        private string _lastPartString;
        private GameObject _gameObject;

        public OnTagInLineOfSightInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            IMonaBody body;
            if (_originPartType != MonaPlayerBodyParts.None)
            {
                var originPlayer = _brain.Player;
                if (originPlayer.PlayerBody == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_PLAYER);
                var part = GetPartString(_originPartType);
                body = originPlayer.PlayerBody.FindChildByTag(part);
                if (body == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_PART);
            }
            else
            {
                var originSource = _originSource != null ? _brain.Variables.GetBody(_originSource) : _brain.Body;
                if (originSource == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_ORIGIN);
                body = originSource.FindChildByTag(_originPart);
                if (body == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_PART);
            }

            RaycastHit hit;
            if (Physics.Raycast(body.GetPosition(), body.GetRotation() * _direction, out hit, _maxDistance, ~LayerMask.NameToLayer("LocalPlayer")))
            {
                var hitBody = hit.collider.GetComponentInParent<IMonaBody>();
                if (hitBody != null)
                {
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_TARGET, hitBody);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_POINT, hit.point);
                    _brain.Variables.Set(MonaBrainConstants.RESULT_HIT_NORMAL, hit.normal);
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_IN_SIGHT);
        }

        private string GetPartString(MonaPlayerBodyParts part)
        {
            //reduce garbage generation to one frame
            if (_lastPart != part)
            {
                _lastPart = part;
                _lastPartString = part.ToString();
            }
            return _lastPartString;
        }

    }
}