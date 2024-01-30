using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnNearInstructionTile : IInstructionTileWithPreloadAndPage
    {
        string MonaTag { get; set; }
        float Distance { get; set; }
        float FieldOfView { get; set; }
    }
}
