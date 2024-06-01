using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using System;
using UnityEngine;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Input.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class OnBoxSelectTagInstructionTile : InstructionTile, IConditionInstructionTile, IStartableInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction,
        IOnStartInstructionTile, ITickAfterInstructionTile, IOnBodyFilterInstructionTile
    {
        public const string ID = "OnBoxSelectTag";
        public const string NAME = "Box Select Tag";
        public const string CATEGORY = "Input";
        public override Type TileType => typeof(OnBoxSelectTagInstructionTile);

        [SerializeField] private Vector2 _tl;
        [SerializeField] private string[] _tlValueName;
        [BrainProperty(true)] public Vector2 TopLeftCorner { get => _tl; set => _tl = value; }
        [BrainPropertyValueName(nameof(TopLeftCorner), typeof(IMonaVariablesVector2Value))] public string[] TopLeftCornerValueName { get => _tlValueName; set => _tlValueName = value; }

        [SerializeField] private Vector2 _br;
        [SerializeField] private string[] _brValueName;
        [BrainProperty(true)] public Vector2 BottomRightCorner { get => _br; set => _br = value; }
        [BrainPropertyValueName(nameof(BottomRightCorner), typeof(IMonaVariablesVector2Value))] public string[] BottomRightValueName { get => _brValueName; set => _brValueName = value; }

        [SerializeField] private string _monaTag;
        [BrainPropertyMonaTag(true)] public string MonaTag { get => _monaTag; set => _monaTag = value; }

        [SerializeField] private float _distance = 100f;
        [SerializeField] private string _distanceValueName;

        [BrainProperty(false)] public float Distance { get => _distance; set => _distance = value; }
        [BrainPropertyValueName("Distance", typeof(IMonaVariablesFloatValue))] public string DistanceValueName { get => _distanceValueName; set => _distanceValueName = value; }

        [SerializeField] private bool _allowParent;
        [BrainProperty(false)] public bool AllowParent { get => _allowParent; set => _allowParent = value; }

        private IMonaBrain _brain;

        private const float _minBounds = 0.0001f;

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            _firstTile = _instruction.InstructionTiles.FindAll(x => x is IOnBodyFilterInstructionTile).IndexOf(this) == 0;
        }

        public override InstructionTileResult Do()
        {
            if (BoxSelect())
                return Complete(InstructionTileResult.Success);
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.NO_INPUT);
        }

        private List<IMonaBody> _monaBodies = new List<IMonaBody>();
        private bool BoxSelect()
        {
            if (HasVector2Values(_tlValueName))
                _tl = GetVector2Value(_brain, _tlValueName);

            if (HasVector2Values(_brValueName))
                _br = GetVector2Value(_brain, _brValueName);

            var cam = MonaGlobalBrainRunner.Instance.PlayerCamera;
            var tly = cam.pixelHeight - _tl.y;
            var bry = cam.pixelHeight - _br.y;
            Rect checkScreenRect = new Rect(_tl.x, tly, _br.x - _tl.x, bry - tly);
            if (checkScreenRect.width == 0 || checkScreenRect.height == 0) return false;

            var bodies = MonaBody.FindByTag(_monaTag);
            _monaBodies.Clear();
            for(var i = 0;i < bodies.Count; i++)
            {
                var body = bodies[i];
                if (body == null) continue;
                if (BodyScreenRectOverlaps(checkScreenRect, body))
                    _monaBodies.Add(body);
            }
            FilterBodiesOnInstruction(_monaBodies);
            return (_monaBodies.Count > 0);
        }

        private bool BodyScreenRectOverlaps(Rect checkScreenRect, IMonaBody targetBody)
        {
            if (targetBody == null)
                return false;

            Bounds targetBounds = GetBodyBounds(targetBody);

            Vector3 targetScreenMin = MonaGlobalBrainRunner.Instance.PlayerCamera.WorldToScreenPoint(targetBounds.min);
            Vector3 targetScreenMax = MonaGlobalBrainRunner.Instance.PlayerCamera.WorldToScreenPoint(targetBounds.max);

            Rect targetScreenRect = new Rect(targetScreenMin.x, targetScreenMin.y, targetScreenMax.x - targetScreenMin.x, targetScreenMax.y - targetScreenMin.y);

            //Debug.Log($"{nameof(BodyScreenRectOverlaps)} {targetScreenRect} {checkScreenRect} {targetBody.Transform.name} {targetScreenRect.Overlaps(checkScreenRect)}", targetBody.Transform.gameObject);
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

    }
}