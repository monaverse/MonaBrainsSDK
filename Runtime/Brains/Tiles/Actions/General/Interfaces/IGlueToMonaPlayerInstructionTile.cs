using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Tiles;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IGlueToMonaPlayerInstructionTile : IInstructionTileWithPreload
    {
        MonaPlayerBodyParts MonaPart { get; set; }
        Vector3 Offset { get; set; }
        Vector3 Scale { get; set; }
    }
}