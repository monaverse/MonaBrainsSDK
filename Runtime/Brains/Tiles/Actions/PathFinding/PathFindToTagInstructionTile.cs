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

        public PathFindToTagInstructionTile() { }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_tag);
            if (bodies.Count > 0)
            {
                IMonaBody closest = bodies[0];

                for (var i = 1; i < bodies.Count; i++)
                {
                    var distanceClosest = Vector3.Distance(_brain.Body.GetPosition(), closest.GetPosition());
                    var distanceBody = Vector3.Distance(_brain.Body.GetPosition(), bodies[i].GetPosition());
                    if (distanceBody < distanceClosest)
                        closest = bodies[i];
                }

                //Debug.Log($"{nameof(PathFindToPositionInstructionTile)} {_value} {_valueValueName}");
                if (_brain != null && closest != null)
                {
                    _agent.SetDestination(closest.GetPosition());
                    return Complete(InstructionTileResult.Success);
                }
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }
    }
}