using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Tiles;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.Interfaces
{
    public interface IDissolveGlueInstructionTile : IInstructionTileWithPreload
    {
        MonaPlayerBodyParts Part { get; set; }
        string Target { get; set; }
        Vector3 Offset { get; set; }
        Vector3 Scale { get; set; }
        bool LetFall { get; set; }
    }
}