using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/InBodyArray", fileName = "InBodyArray")]
    public class InBodyArrayInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new InBodyArrayInstructionTile();
        [SerializeField] protected string _id = InBodyArrayInstructionTile.ID;
        [SerializeField] protected string _name = InBodyArrayInstructionTile.NAME;
        [SerializeField] protected string _category = InBodyArrayInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
