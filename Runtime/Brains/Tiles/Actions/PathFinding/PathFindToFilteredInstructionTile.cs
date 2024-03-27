using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using UnityEngine.AI;
using Unity.VisualScripting;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Tiles.Actions.PathFinding.Enums;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding
{
    [Serializable]
    public class PathFindToFilteredInstructionTile : PathFindInstructionTile
    {
        public new const string ID = "PathFindToFilteredInstructionTile";
        public new const string NAME = "Path Find To Filtered";
        public new const string CATEGORY = "Path Finding";
        public override Type TileType => typeof(PathFindToFilteredInstructionTile);

        [SerializeField] private ChooseTagType _chooseTagType;
        [BrainPropertyEnum(true)] public ChooseTagType ChooseTag { get => _chooseTagType; set => _chooseTagType = value; }

        [SerializeField] private bool _followWhileMoving;
        [BrainProperty(false)] public bool FollowWhileMoving { get => _followWhileMoving; set => _followWhileMoving = value; }

        public PathFindToFilteredInstructionTile() { }

        private int _lastIndex = -1;

        public override InstructionTileResult Do()
        {
            _bodies = _instruction.InstructionBodies;

            if (_movingState == MovingStateType.Stopped || _followWhileMoving)
            {
                var pos = Vector3.zero;
                if (_bodies.Count > 0)
                {
                    IMonaBody closest = _bodies[0];

                    for (var i = 1; i < _bodies.Count; i++)
                    {
                        var distanceClosest = Vector3.Distance(_brain.Body.GetPosition(), closest.GetPosition());
                        var distanceBody = Vector3.Distance(_brain.Body.GetPosition(), _bodies[i].GetPosition());
                        switch (_chooseTagType)
                        {
                            case ChooseTagType.Closest:
                                if (distanceBody < distanceClosest)
                                    closest = _bodies[i];
                                break;
                            case ChooseTagType.Furthest:
                                if (distanceBody > distanceClosest)
                                    closest = _bodies[i];
                                break;
                        }
                        pos += _bodies[i].GetPosition();
                    }

                    if (_brain != null && closest != null)
                    {
                        /*if (_chooseTagType == ChooseTagType.Next)
                        {
                            _lastIndex++;
                            if (_lastIndex >= _bodies.Count)
                                _lastIndex = 0;
                            MoveTo(_bodies[_lastIndex].GetPosition());
                        }
                        else*/
                        if (_chooseTagType == ChooseTagType.Average)
                        {
                            MoveTo(pos / _bodies.Count);
                        }
                        else
                        {
                            if (_agent.isOnNavMesh)
                            {
                                MoveTo(closest.GetPosition());
                            }
                        }

                        if(_movingState == MovingStateType.Moving)
                            return Complete(InstructionTileResult.Running);
                    }
                }
            }
            return Complete(InstructionTileResult.Success);
        }

        private void MoveTo(Vector3 pos)
        {
            //Debug.Log($"{nameof(PathFindToTagInstructionTile)} bodies: {_instruction.InstructionBodies.Count} pos: {pos}");
            SetAgentSettings();
            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.SetDestination(pos);
            }
            _movingState = MovingStateType.Moving;
            AddFixedTickDelegate();
        }
    }
}