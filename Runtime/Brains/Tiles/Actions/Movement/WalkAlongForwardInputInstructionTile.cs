using Mona.SDK.Brains.Core.Animation;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class WalkAlongForwardInputInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "WalkAlongForward";
        public const string NAME = "Walk Along Forward Input";
        public const string CATEGORY = "Character";
        public override Type TileType => typeof(WalkAlongForwardInputInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.InputForwardBack;

        private IMonaAnimationController _controller;

        public override void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            base.Preload(brainInstance, page, instruction);
            _controller = _brain.Root.GetComponent<IMonaAnimationController>();
        }

        protected override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            if (_movingState == MovingStateType.Moving || _mode == MoveModeType.Instant)
            {
                switch (_brain.PropertyType)
                {
                    case MonaBrainPropertyType.GroundedCreature: TickGroundedCreature(deltaTime); break;
                    default: TickDefault(deltaTime); break;
                }
            }
        }

        protected override void StoppedMoving()
        {
            switch (_brain.PropertyType)
            {
                case MonaBrainPropertyType.GroundedCreature: StopGroundedCreature(); break;
                default: StopDefault(); break;
            }
        }

        private void TickGroundedCreature(float deltaTime)
        {
            _controller.SetWalk(GetSpeed());
            _controller.SetMotionSpeed(GetMotionSpeed(DirectionType));
        }

        private void StopGroundedCreature()
        {
            _controller.SetWalk(0);
        }

        private void TickDefault(float deltaTime)
        {

        }

        private void StopDefault()
        {

        }
    }
}