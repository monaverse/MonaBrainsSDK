using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnNumberLessThan", fileName = "OnNumberLessThan")]
    public class OnNumberLessThanInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnNumberLessThanInstructionTile();
        [SerializeField] protected string _id = OnNumberLessThanInstructionTile.ID;
        [SerializeField] protected string _name = OnNumberLessThanInstructionTile.NAME;
        [SerializeField] protected string _category = OnNumberLessThanInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
