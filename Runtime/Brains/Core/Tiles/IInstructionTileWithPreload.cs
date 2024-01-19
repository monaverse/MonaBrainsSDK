using Mona.Brains.Core.Brain;
using UnityEngine;

namespace Mona.Brains.Core.Tiles
{
    public interface IInstructionTileWithPreload : IInstructionTile
    {
        void Preload(IMonaBrain brainInstance);   
    }
}