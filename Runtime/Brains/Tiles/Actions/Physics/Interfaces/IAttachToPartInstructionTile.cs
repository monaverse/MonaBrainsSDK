﻿using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Tiles;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces
{
    public interface IAttachToPartInstructionTile : IInstructionTileWithPreload
    {
        string Part { get; set; }
        Vector3 Offset { get; set; }
        Vector3 Scale { get; set; }
    }
}