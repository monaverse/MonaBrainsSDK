using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface IOnKeyInstructionTile : IInstructionTile
    {
        KeyCode KeyCode { get; set; }
    }
}
