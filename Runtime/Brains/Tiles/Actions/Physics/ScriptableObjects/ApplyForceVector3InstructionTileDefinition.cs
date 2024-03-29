using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/ApplyForceVector3", fileName = "ApplyForceVector3")]
    public class ApplyForceVector3InstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyForceVector3InstructionTile();
        [SerializeField] protected string _id = ApplyForceVector3InstructionTile.ID;
        [SerializeField] protected string _name = ApplyForceVector3InstructionTile.NAME;
        [SerializeField] protected string _category = ApplyForceVector3InstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
