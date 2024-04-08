using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveAlongVector3", fileName = "MoveAlongVector3")]
    public class MoveAlongVector3InstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveAlongVector3InstructionTile();
        [SerializeField] protected string _id = MoveAlongVector3InstructionTile.ID;
        [SerializeField] protected string _name = MoveAlongVector3InstructionTile.NAME;
        [SerializeField] protected string _category = MoveAlongVector3InstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
