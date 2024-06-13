using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Torque/ApplyTorqueRollLeft", fileName = "ApplyTorqueRollLeft")]
    public class ApplyTorqueRollLeftInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyTorqueRollLeftInstructionTile();
        [SerializeField] protected string _id = ApplyTorqueRollLeftInstructionTile.ID;
        [SerializeField] protected string _name = ApplyTorqueRollLeftInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyTorqueRollLeftInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
