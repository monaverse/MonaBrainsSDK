using Unity.VisualScripting;
using UnityEngine;

namespace Mona.Brains.Core.Tiles
{
    public interface IEventInstructionTileWithGameObject : IEventInstructionTile
    {
        void SetGameObject(GameObject gameObject);   
    }
}