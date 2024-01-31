using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class AttachToTargetPartInstructionTile : InstructionTile, IAttachToTargetPartInstructionTile, IActionInstructionTile
    {
        public const string ID = "AttachToTargetPart";
        public const string NAME = "Attach To Target Part";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToTargetPartInstructionTile);

        [SerializeField] private string _target;
        [SerializeField] private string _targetValueName;
        [BrainProperty(true)] public string Target { get => _target; set => _target = value; }
        [BrainPropertyValueName] public string TargetValueName { get => _targetValueName; set => _targetValueName = value; }

        [SerializeField]
        private string _part = "Default";
        [BrainPropertyMonaTag]
        public string Part { get => _part; set => _part = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        private IMonaBrain _brain;

        public AttachToTargetPartInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            IMonaBody body = null;
            if(!string.IsNullOrEmpty(_targetValueName))
            {
                var value = _brain.State.GetValue(_targetValueName);
                if (value is IMonaStateBrainValue)
                    body = ((IMonaStateBrainValue)value).Value.Body;
                else if (value is IMonaStateBodyValue)
                    body = ((IMonaStateBodyValue)value).Value;
            }

            if (body != null)
            {
                var playerPart = body.FindChildByTag(_part.ToString());
                if (playerPart == null) playerPart = body;
                _brain.Body.SetScale(_scale, true);
                _brain.Body.SetTransformParent(playerPart.ActiveTransform);
                _brain.Body.SetPosition(playerPart.ActiveTransform.position + playerPart.ActiveTransform.parent.TransformDirection(_offset), true, true);
                _brain.Body.SetRotation(playerPart.ActiveTransform.rotation, true, true);
            }
         
            return Complete(InstructionTileResult.Success);
        }
    }
}