using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class TeleportToRotationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "TeleportToRotationInstructionTile";
        public const string NAME = "Teleport To Rotation";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(TeleportToRotationInstructionTile);

        [SerializeField] private Vector3 _value;
        [SerializeField] private string[] _valueValueName;
        [BrainProperty(true)] public Vector3 Value { get => _value; set => _value = value; }
        [BrainPropertyValueName("Value", typeof(IMonaVariablesVector3Value))] public string[] ValueValueName { get => _valueValueName; set => _valueValueName = value; }

        [SerializeField] private bool _useLocal;
        [BrainProperty(true)] public bool UseLocal { get => _useLocal; set => _useLocal = value; }

        private IMonaBrain _brain;

        public TeleportToRotationInstructionTile() { }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        } 

        public override InstructionTileResult Do()
        {
            if (HasVector3Values(_valueValueName))
                _value = GetVector3Value(_brain, _valueValueName);

            //Debug.Log($"{nameof(TeleportToRotationInstructionTile)} {_value} {_valueValueName}");
            if (_brain != null)
            {
                _brain.Body.OnTeleported -= HandleTeleported;
                _brain.Body.OnTeleported += HandleTeleported;
                if (_useLocal)
                {
                    if (_brain.Body.ActiveTransform.parent != null)
                        _brain.Body.TeleportRotation(_brain.Body.ActiveTransform.parent.rotation * Quaternion.Euler(_value), true);
                    else
                        _brain.Body.TeleportRotation(_brain.Body.ActiveTransform.rotation * Quaternion.Euler(_value), true);
                }
                else
                    _brain.Body.TeleportRotation(Quaternion.Euler(_value), true);
                return Complete(InstructionTileResult.Running);
            }   
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private void HandleTeleported()
        {
            //Debug.Log($"{nameof(HandleTeleported)} ", _brain.Body.Transform.gameObject);
            _brain.Body.OnTeleported -= HandleTeleported;
            Complete(InstructionTileResult.Success, true);
        }

    }
}