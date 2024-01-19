using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IEventInstructionTileWithGameObject : IEventInstructionTile
    {
        void SetGameObject(GameObject gameObject);   
    }
}