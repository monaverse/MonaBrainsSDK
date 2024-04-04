using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Body.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class SetDragInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload,
        IRigidbodyInstructionTile
    {
        public const string ID = "SetDrag";
        public const string NAME = "Set Drag";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(SetDragInstructionTile);

        [SerializeField] private DragType _dragType = DragType.Linear;
        [BrainPropertyEnum(true)] public DragType DragType { get => _dragType; set => _dragType = value; }

        [SerializeField] private float _drag = .2f;
        [SerializeField] private string _dragName;
        [BrainProperty(true)] public float Drag { get => _drag; set => _drag = value; }
        [BrainPropertyValueName("Drag", typeof(IMonaVariablesFloatValue))] public string DragName { get => _dragName; set => _dragName = value; }

        [SerializeField] private float _angularDrag = .2f;
        [SerializeField] private string _angularDragName;
        [BrainProperty(true)] public float AngularDrag { get => _angularDrag; set => _angularDrag = value; }
        [BrainPropertyValueName("AngularDrag", typeof(IMonaVariablesFloatValue))] public string AngularDragName { get => _angularDragName; set => _angularDragName = value; }

        private IMonaBrain _brain;

        public SetDragInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE); ;

            if (!string.IsNullOrEmpty(_dragName))
                _drag = _brain.Variables.GetFloat(_dragName);

            if (!string.IsNullOrEmpty(_angularDragName))
                _angularDrag = _brain.Variables.GetFloat(_angularDragName);

            var body = _brain.Body;
            body.SetDragType(_dragType);
            body.SetDrag(_drag);
            body.SetAngularDrag(_angularDrag);

            return Complete(InstructionTileResult.Success);
        }
    }
}