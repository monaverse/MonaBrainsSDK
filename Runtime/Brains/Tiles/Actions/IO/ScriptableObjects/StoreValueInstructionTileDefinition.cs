using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.IO.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/IO/StoreValue", fileName = "StoreValue")]
    public class StoreValueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new StoreValueInstructionTile();
        [SerializeField] protected string _id = StoreValueInstructionTile.ID;
        [SerializeField] protected string _name = StoreValueInstructionTile.NAME;
        [SerializeField] protected string _category = StoreValueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
