using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Linear/ApplyForceAlignHover", fileName = "ApplyForceAlignHover")]
    public class ApplyForceAlignHoverInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyForceAlignHoverInstructionTile();
        [SerializeField] protected string _id = ApplyForceAlignHoverInstructionTile.ID;
        [SerializeField] protected string _name = ApplyForceAlignHoverInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyForceAlignHoverInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
