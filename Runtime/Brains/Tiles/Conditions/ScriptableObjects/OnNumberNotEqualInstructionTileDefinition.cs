using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnNumberNotEqual", fileName = "OnNumberNotEqual")]
    public class OnNumberNotEqualInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnNumberNotEqualInstructionTile();
        [SerializeField] protected string _id = OnNumberNotEqualInstructionTile.ID;
        [SerializeField] protected string _name = OnNumberNotEqualInstructionTile.NAME;
        [SerializeField] protected string _category = OnNumberNotEqualInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
