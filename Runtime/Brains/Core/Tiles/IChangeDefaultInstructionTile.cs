using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IChangeDefaultInstructionTile : IInstructionTile
    {
        Vector3 GetEndPosition(Vector3 pos);
    }
}