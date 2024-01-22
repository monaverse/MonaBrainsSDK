using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnFarInstructionTile : IInstructionTileWithPreload
    {
        string MonaTag { get; set; }
        float Distance { get; set; }
    }
}
