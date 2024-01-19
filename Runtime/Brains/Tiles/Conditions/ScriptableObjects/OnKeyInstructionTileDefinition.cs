using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnKey", fileName = "OnKey")]
    public class OnKeyInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnKeyInstructionTile();
        [SerializeField] protected string _id = OnKeyInstructionTile.ID;
        [SerializeField] protected string _name = OnKeyInstructionTile.NAME;
        [SerializeField] protected string _category = OnKeyInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
