using Mona.Core.Body.Enums;
using Mona.Brains.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnTagInLineOfSightInstructionTile : IInstructionTileWithPreload
    {
        string OriginSource { get; set; }
        MonaPlayerBodyParts OriginPartType { get; set; }
        string OriginPart { get; set; }
        Vector3 Direction { get; set; }
        float MaxDistance { get; set; }
        string TargetMonaTag { get; set; }

    }
}
