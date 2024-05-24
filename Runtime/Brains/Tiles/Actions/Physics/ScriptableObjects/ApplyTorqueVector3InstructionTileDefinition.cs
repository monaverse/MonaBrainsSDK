using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/ApplyTorqueVector3", fileName = "ApplyTorqueVector3")]
    public class ApplyTorqueVector3InstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyTorqueVector3InstructionTile();
        [SerializeField] protected string _id = ApplyTorqueVector3InstructionTile.ID;
        [SerializeField] protected string _name = ApplyTorqueVector3InstructionTile.NAME;
        [SerializeField] protected string _category = ApplyTorqueVector3InstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
