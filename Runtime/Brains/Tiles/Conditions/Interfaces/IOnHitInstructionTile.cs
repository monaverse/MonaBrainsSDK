using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnHitInstructionTile : IInstructionTileWithPreloadAndPage
    {
        string MonaTag { get; set; }
    }
}
