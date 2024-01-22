using Mona.SDK.Brains.Core.Enums;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Conditions.Interfaces
{
    public interface ITriggerInstructionTile
    {
        List<MonaTriggerType> TriggerTypes { get; }
    }
}
