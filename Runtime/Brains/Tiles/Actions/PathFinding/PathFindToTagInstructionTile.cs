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
    public class PathFindToTagInstructionTile : PathFindInstructionTile
    {
        public new const string ID = "PathFindToTagInstructionTile";
        public new const string NAME = "Path Find To Tag";
        public new const string CATEGORY = "Path Finding";
        public override Type TileType => typeof(PathFindToTagInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private ChooseTagType _chooseTagType;
        [BrainPropertyEnum(true)] public ChooseTagType ChooseTag { get => _chooseTagType; set => _chooseTagType = value; }

        [SerializeField] private bool _followWhileMoving;
        [BrainProperty(false)] public bool FollowWhileMoving { get => _followWhileMoving; set => _followWhileMoving = value; }

        public PathFindToTagInstructionTile() { }

        public override InstructionTileResult Do()
        {
            if (_movingState == MovingStateType.Stopped || _followWhileMoving)
            {
                var bodies = MonaBodyFactory.FindByTag(_tag);

                Debug.Log($"{nameof(PathFindToTagInstructionTile)} bodies {bodies.Count}");

                var pos = Vector3.zero;
                if (bodies.Count > 0)
                {
                    IMonaBody closest = bodies[0];

                    if (_chooseTagType == ChooseTagType.Random)
                    {
                        var active = bodies.FindAll(x => x.GetActive());
                        if (active.Count > 0)
                            closest = active[UnityEngine.Random.Range(0, active.Count)];
                    }
                    else
                    {
                        for (var i = 1; i < bodies.Count; i++)
                        {
                            if (!bodies[i].GetActive()) continue;
                            var distanceClosest = Vector3.Distance(_brain.Body.GetPosition(), closest.GetPosition());
                            var distanceBody = Vector3.Distance(_brain.Body.GetPosition(), bodies[i].GetPosition());
                            switch (_chooseTagType)
                            {
                                case ChooseTagType.Closest:
                                    if (distanceBody < distanceClosest)
                                        closest = bodies[i];
                                    break;
                                case ChooseTagType.Furthest:
                                    if (distanceBody > distanceClosest)
                                        closest = bodies[i];
                                    break;
                            }
                            pos += bodies[i].GetPosition();
                        }
                    }

                    if (_brain != null && closest != null)
                    {
                        /*if (_chooseTagType == ChooseTagType.Next)
                        {
                            _lastIndex++;
                            if (_lastIndex >= bodies.Count)
                                _lastIndex = 0;
                            MoveTo(bodies[_lastIndex].GetPosition());
                        }
                        else
                        */
                        if (_chooseTagType == ChooseTagType.Average)
                        {
                            MoveTo(pos / bodies.Count);
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
            Debug.Log($"{nameof(PathFindToTagInstructionTile)} bodies: {_instruction.InstructionBodies.Count} pos: {pos}");
            SetAgentSettings();
            _agent.isStopped = false;
            _agent.SetDestination(pos);
            _movingState = MovingStateType.Moving;
            AddFixedTickDelegate();
        }
    }
}