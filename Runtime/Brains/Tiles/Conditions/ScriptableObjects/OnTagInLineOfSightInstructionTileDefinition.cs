using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnTagInLineOfSight", fileName = "OnTagInLineOfSight")]
    public class OnTagInLineOfSightInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnTagInLineOfSightInstructionTile();
        [SerializeField] protected string _id = OnTagInLineOfSightInstructionTile.ID;
        [SerializeField] protected string _name = OnTagInLineOfSightInstructionTile.NAME;
        [SerializeField] protected string _category = OnTagInLineOfSightInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
