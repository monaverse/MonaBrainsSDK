using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using UnityEngine.AI;
using Unity.VisualScripting;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding
{
    [Serializable]
    public class PathFindInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "PathFindInstructionTile";
        public const string NAME = "Path Find To Position";
        public const string CATEGORY = "Path Finding";
        public override Type TileType => typeof(PathFindInstructionTile);

        [SerializeField] private float _baseOffset = 1f;
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

        public PathFindInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            _agent = _brain.Body.ActiveTransform.GetComponent<NavMeshAgent>();
            if (_agent == null)
                _agent = _brain.Body.ActiveTransform.AddComponent<NavMeshAgent>();

            _agent.baseOffset = _baseOffset;
            _agent.speed = _speed;
            _agent.angularSpeed = _angularSpeed;
            _agent.acceleration = _acceleration;
            _agent.stoppingDistance = _stoppingDistance;
            _agent.autoBraking = _autoBraking;
            _agent.radius = _avoidRadius;
            _agent.height = _height;
        }

        public override InstructionTileResult Do()
        {
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}