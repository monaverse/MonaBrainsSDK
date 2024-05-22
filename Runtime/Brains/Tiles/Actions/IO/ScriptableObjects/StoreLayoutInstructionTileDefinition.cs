using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.IO.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/IO/StoreLayout", fileName = "StoreLayout")]
    public class StoreLayoutInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new StoreLayoutInstructionTile();
        [SerializeField] protected string _id = StoreLayoutInstructionTile.ID;
        [SerializeField] protected string _name = StoreLayoutInstructionTile.NAME;
        [SerializeField] protected string _category = StoreLayoutInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
