using Mona.Core.Body.Enums;
using Mona.Brains.Core.Tiles;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.General.Interfaces
{
    public interface IGlueToMonaPlayerInstructionTile : IInstructionTileWithPreload
    {
        MonaPlayerBodyParts MonaPart { get; set; }
        Vector3 Offset { get; set; }
        Vector3 Scale { get; set; }
    }
}