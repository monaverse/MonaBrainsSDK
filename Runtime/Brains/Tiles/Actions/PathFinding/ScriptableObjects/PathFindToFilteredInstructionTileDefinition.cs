using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/PathFinding/PathFindToFiltered", fileName = "PathFindToFiltered")]
    public class PathFindToFilteredInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PathFindToFilteredInstructionTile();
        [SerializeField] protected string _id = PathFindToFilteredInstructionTile.ID;
        [SerializeField] protected string _name = PathFindToFilteredInstructionTile.NAME;
        [SerializeField] protected string _category = PathFindToFilteredInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
