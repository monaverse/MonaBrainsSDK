using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IInstructionTileWithPreloadAndPage : IInstructionTile
    {
        void Preload(IMonaBrain brainInstance, IMonaBrainPage page);   
    }
}