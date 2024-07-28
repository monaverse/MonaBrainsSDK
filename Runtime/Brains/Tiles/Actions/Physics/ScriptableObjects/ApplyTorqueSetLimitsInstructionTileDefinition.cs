using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Torque/SetTorqueLimits", fileName = "SetTorqueLimits")]
    public class ApplyTorqueSetLimitsInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyTorqueSetLimitsInstructionTile();
        [SerializeField] protected string _id = ApplyTorqueSetLimitsInstructionTile.ID;
        [SerializeField] protected string _name = ApplyTorqueSetLimitsInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyTorqueSetLimitsInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
