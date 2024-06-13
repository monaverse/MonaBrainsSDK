using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Torque/ApplyTorqueTurnLeft", fileName = "ApplyTorqueTurnLeft")]
    public class ApplyTorqueTurnLeftInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyTorqueTurnLeftInstructionTile();
        [SerializeField] protected string _id = ApplyTorqueTurnLeftInstructionTile.ID;
        [SerializeField] protected string _name = ApplyTorqueTurnLeftInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyTorqueTurnLeftInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
