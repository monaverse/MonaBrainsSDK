using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.Body;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface INeedAuthorityInstructionTile
    {
        List<IMonaBody> GetBodiesToControl();
    }
}