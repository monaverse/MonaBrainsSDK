using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/PathFinding/PathFindToPosition", fileName = "PathFindToPosition")]
    public class PathFindToPositionInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PathFindToPositionInstructionTile();
        [SerializeField] protected string _id = PathFindToPositionInstructionTile.ID;
        [SerializeField] protected string _name = PathFindToPositionInstructionTile.NAME;
        [SerializeField] protected string _category = PathFindToPositionInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
