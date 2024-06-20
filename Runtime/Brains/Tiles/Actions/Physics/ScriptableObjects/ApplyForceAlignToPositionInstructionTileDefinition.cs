using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Linear/ApplyForceAlignToPosition", fileName = "ApplyForceAlignToPosition")]
    public class ApplyForceAlignToPositionInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyForceAlignToPositionInstructionTile();
        [SerializeField] protected string _id = ApplyForceAlignToPositionInstructionTile.ID;
        [SerializeField] protected string _name = ApplyForceAlignToPositionInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyForceAlignToPositionInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
