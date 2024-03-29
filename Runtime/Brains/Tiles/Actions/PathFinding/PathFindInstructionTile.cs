using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using UnityEngine.AI;
using Unity.VisualScripting;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Animation;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Events;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding
{
    [Serializable]
    public class PathFindInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IActivateInstructionTile, IPauseableInstructionTile
    {
        public const string ID = "PathFindInstructionTile";
        public const string NAME = "Path Find To Position";
        public const string CATEGORY = "Path Finding";
        public override Type TileType => typeof(PathFindInstructionTile);

        [SerializeField] private float _baseOffset = 0f;
        [SerializeField] private float _speed = 3.5f;
        [SerializeField] private float _angularSpeed = 120f;
        [SerializeField] private float _acceleration = 8f;
        [SerializeField] private float _stoppingDistance = 0f;
        [SerializeField] private bool _autoBraking = true;

        [SerializeField] private float _avoidRadius = .5f;
        [SerializeField] private float _height = 2f;

        [BrainProperty(false)] public float BaseOffset { get => _baseOffset; set => _baseOffset = value; }
        [BrainProperty(false)] public float Speed { get => _speed; set => _speed = value; }
        [BrainProperty(false)] public float AngularSpeed { get => _angularSpeed; set => _angularSpeed = value; }
        [BrainProperty(false)] public float Acceleration { get => _acceleration; set => _acceleration = value; }
        [BrainProperty(false)] public float StoppingDistance { get => _stoppingDistance; set => _stoppingDistance = value; }
        [BrainProperty(false)] public bool AutoBraking { get => _autoBraking; set => _autoBraking = value; }

        [BrainProperty(false)] public float AvoidRadius { get => _avoidRadius; set => _avoidRadius = value; }
        [BrainProperty(false)] public float Height { get => _height; set => _height = value; }

        protected IMonaBrain _brain;
        protected NavMeshAgent _agent;
        protected bool _active;
        protected IMonaAnimationController _controller;

        protected MovingStateType _movingState = MovingStateType.Stopped;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaBodyEvent> OnBodyEvent;
        private Action<MonaBodyAnimationControllerChangedEvent> OnAnimationControllerChanged;

        public PathFindInstructionTile() { }

        public override void SetThenCallback(IInstructionTileCallback thenCallback)
        {
            if (_thenCallback == null)
            {
                _thenCallback = new InstructionTileCallback();
                _thenCallback.Action = () =>
                {
                    RemoveFixedTickDelegate();
                    if (thenCallback != null) return thenCallback.Action.Invoke();
                    return InstructionTileResult.Success;
                };
            }
        }

        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;

            _agent = _brain.Body.ActiveTransform.GetComponent<NavMeshAgent>();
            if (_agent == null)
                _agent = _brain.Body.ActiveTransform.AddComponent<NavMeshAgent>();

            SetAgentSettings();

            OnAnimationControllerChanged = HandleAnimationControllerChanged;
            EventBus.Register<MonaBodyAnimationControllerChangedEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            SetupAnimation();
        }

        private void HandleAnimationControllerChanged(MonaBodyAnimationControllerChangedEvent evt)
        {
            SetupAnimation();
        }

        private void SetupAnimation()
        {
            if (_brain.Root != null)
                _controller = _brain.Root.GetComponent<IMonaAnimationController>();
            else
            {
                var children = _brain.Body.Children();
                for (var i = 0;i < children.Count; i++)
                {
                    var root = children[i].Transform.Find("Root");
                    if(root != null)
                    {
                        _controller = _brain.Root.GetComponent<IMonaAnimationController>();
                        if (_controller != null) break;
                    }
                }
            }
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                if (_brain != null)
                    UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                if (_movingState == MovingStateType.Moving)
                    LostControl();
                return;
            }

            if (_movingState == MovingStateType.Moving)
            {
                AddFixedTickDelegate();
            }

            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        public override void Unload()
        {
            RemoveFixedTickDelegate();
            EventBus.Unregister(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, _brain.Body), OnAnimationControllerChanged);

            //if(_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
            _agent.isStopped = true;
            //if(_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(MoveLocalInstructionTile)}.{nameof(Pause)}");
        }

        public bool Resume()
        {
            UpdateActive();
            if (_movingState == MovingStateType.Moving)
                _agent.isStopped = false;
            return _movingState == MovingStateType.Moving;
        }

        private void LostControl()
        {
            Debug.Log($"{nameof(PathFindInstructionTile)} {nameof(LostControl)}");
            _movingState = MovingStateType.Stopped;
            StoppedMoving();
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void StopMoving()
        {
            //Debug.Log($"INPUT stopmoving: {Name} {_progressName} {Progress}");
            _agent.isStopped = true;

            _timeMoving = 0;
            _movingState = MovingStateType.Stopped;
            StoppedMoving();
            Complete(InstructionTileResult.Success, true);
        }

        protected virtual void StoppedMoving()
        {
            switch (_brain.PropertyType)
            {
                case MonaBrainPropertyType.GroundedCreature: StopGroundedCreature(); break;
                default: StopDefault(); break;
            }
        }

        private void StopDefault()
        {

        }

        protected void AddFixedTickDelegate()
        {
            RemoveFixedTickDelegate();
            //Debug.Log($"{nameof(AddFixedTickDelegate)}, fr: {Time.frameCount}", _brain.Body.Transform.gameObject);

            OnFixedTick = HandleFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            OnBodyEvent = HandleBodyEvent;
            EventBus.Register<MonaBodyEvent>(new EventHook(MonaCoreConstants.MONA_BODY_EVENT, _brain.Body), OnBodyEvent);
        }

        protected void RemoveFixedTickDelegate()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            EventBus.Unregister(new EventHook(MonaBrainConstants.MONA_BRAINS_EVENT, _brain.Body), OnBodyEvent);
        }

        private void HandleBodyEvent(MonaBodyEvent evt)
        {
            if (evt.Type == MonaBodyEventType.OnStop)
                LostControl();
        }

        public InstructionTileResult Continue()
        {
            _movingState = MovingStateType.Moving;
            AddFixedTickDelegate();
            return Do();
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            Tick(evt.DeltaTime);

            if (_movingState == MovingStateType.Moving)
            {
                switch (_brain.PropertyType)
                {
                    case MonaBrainPropertyType.GroundedCreature: TickGroundedCreature(evt.DeltaTime); break;
                    default: TickDefault(evt.DeltaTime); break;
                }
            }
        }

        private float _timeMoving;
        private float _currentSpeed;
        private Vector3 _lastPosition;
        protected virtual void Tick(float deltaTime)
        {
            if (!_brain.Body.HasControl())
            {
                LostControl();
                return;
            }

            _currentSpeed = Mathf.Abs(Vector3.Distance(_brain.Body.GetPosition(), _lastPosition) / deltaTime);
            _lastPosition = _brain.Body.GetPosition();
            if (_currentSpeed < 0.01f)
                _currentSpeed = 0;

            //Debug.Log($"remaining: {_agent.remainingDistance} {_agent.pathStatus} pending: {_agent.pathPending} stopped: {_agent.isStopped}");

            if (!_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        StopMoving();
                    }
                }
            }
        }

        protected float GetSpeed()
        {
            return _currentSpeed;
        }

        private void TickGroundedCreature(float deltaTime)
        {
            if (_controller == null) return;
            var motion = 1f;
            var speed = GetSpeed();
            _controller.SetWalk(speed);
            _controller.SetMotionSpeed(motion);
        }

        private void StopGroundedCreature()
        {
            if (_controller == null) return;
            _controller.SetWalk(0);
        }

        private void TickDefault(float deltaTime)
        {

        }


        public override InstructionTileResult Do()
        {
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        protected void SetAgentSettings()
        {
            _agent.baseOffset = _baseOffset;
            _agent.speed = _speed;
            _agent.angularSpeed = _angularSpeed;
            _agent.acceleration = _acceleration;
            _agent.stoppingDistance = _stoppingDistance;
            _agent.autoBraking = _autoBraking;
            _agent.radius = _avoidRadius;
            _agent.height = _height;

        }
    }
}