﻿using Mona.SDK.Brains.Core.Animation;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Tiles.Actions.Physics.Enums;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using System;
using Unity.VisualScripting;
using UnityEngine;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class JumpInstructionTile : ApplyForceUpInstructionTile, IAnimationInstructionTile
    {
        public new const string ID = "Jump";
        public new const string NAME = "Jump";
        public new const string CATEGORY = "Character";
        public override Type TileType => typeof(JumpInstructionTile);

        public bool IsAnimationTile => true;

        private IMonaAnimationController _controller;
        private Action<MonaBodyFixedTickEvent> OnJumpTick;

        public override void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            base.Preload(brainInstance, page, instruction);
            _controller = _brain.Root.GetComponent<IMonaAnimationController>();
        }

        protected override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
        }

        private int _jumpFrame;
        protected override void Pushed()
        {
            _controller.Jump();
            _jumpFrame = Time.frameCount;
            AddJumpTickDelegate();
        }

        private void AddJumpTickDelegate()
        {
            OnJumpTick = HandleJumpTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnJumpTick);
        }

        private void HandleJumpTick(MonaBodyFixedTickEvent evt)
        {
            if(_brain.Body.Grounded && Time.frameCount - _jumpFrame > 2)
            {
                _controller.Landed();
                RemoveJumpTickDelegate();
            }
        }

        private void RemoveJumpTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnJumpTick);
        }

        protected override void StoppedPushing()
        {
        }

    }
}