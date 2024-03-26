using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/PathFinding/PathFindToTag", fileName = "PathFindToTag")]
    public class PathFindToTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PathFindToTagInstructionTile();
        [SerializeField] protected string _id = PathFindToTagInstructionTile.ID;
        [SerializeField] protected string _name = PathFindToTagInstructionTile.NAME;
        [SerializeField] protected string _category = PathFindToTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
