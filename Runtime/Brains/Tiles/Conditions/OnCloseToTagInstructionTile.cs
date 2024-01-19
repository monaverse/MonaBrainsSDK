using Mona.Brains.Core;
using Mona.Brains.Core.Brain;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Tiles.Conditions.Behaviours;
using Mona.Brains.Tiles.Conditions.Interfaces;
using System;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnCloseToTagInstructionTile : InstructionTile, IOnCloseToTagInstructionTile, IDisposable, IConditionInstructionTile
    {
        public const string ID = "OnCloseToTag";
        public const string NAME = "On Close To Tag";
        public const string CATEGORY = "Condition";
        public override Type TileType => typeof(OnCloseToTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainProperty]
        public string MonaTag { get => _tag; set => _tag = value; }

        [SerializeField]
        private float _distance = 2f;
        [BrainProperty]
        public float Distance { get => _distance; set => _distance = value; }

        [SerializeField]
        private float _fieldOfView = 45f;
        [BrainProperty]
        public float FieldOfView { get => _fieldOfView; set => _fieldOfView = value; }

        private IMonaBrain _brain;
        private SphereColliderTriggerBehaviour _collider;
        private GameObject _gameObject;

        public OnCloseToTagInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public void Dispose()
        {
            GameObject.Destroy(_collider);
        }

        public override InstructionTileResult Do()
        {
            _collider.SetRadius(_distance);
            var body = _collider.FindForwardMostBodyWithMonaTagInFieldOfView(_tag, _fieldOfView);
            if (body != null)
            {
                Debug.Log($"{nameof(OnCloseToTagInstructionTile)}.{nameof(Do)} found: {body}");
                _brain.State.Set(MonaBrainConstants.RESULT_TARGET, body);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NOTHING_CLOSE_BY);
        }
    }
}