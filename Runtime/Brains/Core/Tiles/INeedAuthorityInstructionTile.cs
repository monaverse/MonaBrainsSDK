using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.Body;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface INeedAuthorityInstructionTile
    {
        IMonaBody GetBodyToControl();
    }
}