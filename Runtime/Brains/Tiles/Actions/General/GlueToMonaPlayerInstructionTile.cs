using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using Mona.Brains.Core.Enums;
using Mona.Core.Body.Enums;
using Mona.Brains.Core.Brain;
using Mona.Core.Body;
using Mona.Core.Events;
using Mona.Brains.Tiles.Actions.General.Interfaces;

namespace Mona.Brains.Tiles.Actions.General
{
    [Serializable]
    public class GlueToMonaPlayerInstructionTile : InstructionTile, IGlueToMonaPlayerInstructionTile, IDisposable, IActionInstructionTile
    {
        public const string ID = "GlueToMonaPlayer";
        public const string NAME = "Glue To\n Mona Player";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(GlueToMonaPlayerInstructionTile);

        [SerializeField]
        private MonaPlayerBodyParts _monaPart = MonaPlayerBodyParts.Camera;
        [BrainPropertyEnum(true)]
        public MonaPlayerBodyParts MonaPart { get => _monaPart; set => _monaPart = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        private IMonaBrain _brain;
        private IMonaBody _playerPart;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        private bool _isListening;

        private bool _wasGlued;
        private bool _glued
        {
            get => _brain.State.GetBool(MonaBrainConstants.RESULT_GLUED);
            set => _brain.State.Set(MonaBrainConstants.RESULT_GLUED, value);
        }

        public GlueToMonaPlayerInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
            OnFixedTick = HandleFixedTick;             
        }

        public void Dispose()
        {
            EventBus.Unregister(new EventHook(MonaBrainConstants.FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            if (_isListening)
            {
                if (_brain.Player != null && _brain.Player.PlayerBody != null && _playerPart == null)
                {
                    _playerPart = _brain.Player.PlayerBody.FindChildByTag(_monaPart.ToString());
                    _glued = true;
                    _wasGlued = true;
                }

                if (_glued)
                {
                    if (_playerPart != null)
                    {
                        _brain.Body.SetLayer("LocalPlayer", true, true);
                        _brain.Body.SetPosition(_playerPart.ActiveTransform.position + _playerPart.ActiveTransform.rotation * _offset, true, true);
                        _brain.Body.SetRotation(_playerPart.ActiveTransform.rotation, true, true);
                        _brain.Body.SetScale(_scale, true);
                    }
                }
                else if(_wasGlued)
                {
                    EventBus.Unregister(new EventHook(MonaBrainConstants.FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
                    _brain.Body.SetLayer("Default", true, true);
                    _isListening = false;
                }
            }
        }

        public override InstructionTileResult Do()
        {
            if(!_isListening)
            {
                _playerPart = null;
                _isListening = true;
                EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaBrainConstants.FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}