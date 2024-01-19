using Mona.Brains.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnCloseToTagInstructionTile : IInstructionTileWithPreload
    {
        string MonaTag { get; set; }
        float Distance { get; set; }
        float FieldOfView { get; set; }
    }
}
