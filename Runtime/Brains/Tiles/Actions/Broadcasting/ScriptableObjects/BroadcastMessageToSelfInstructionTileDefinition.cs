using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using Mona.Brains.Tiles.Actions.Broadcasting;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Broadcasting.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Broadcasting/BroadcastMessageToSelf", fileName = "BroadcastMessageToSelf")]
    public class BroadcastMessageToSelfInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new BroadcastMessageToSelfInstructionTile();
        [SerializeField] protected string _id = BroadcastMessageToSelfInstructionTile.ID;
        [SerializeField] protected string _name = BroadcastMessageToSelfInstructionTile.NAME;
        [SerializeField] protected string _category = BroadcastMessageToSelfInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
