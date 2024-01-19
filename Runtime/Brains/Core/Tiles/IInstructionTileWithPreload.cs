using Mona.SDK.Brains.Core.Brain;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IInstructionTileWithPreload : IInstructionTile
    {
        void Preload(IMonaBrain brainInstance);   
    }
}