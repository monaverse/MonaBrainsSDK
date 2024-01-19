using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using Mona.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveLocalTowardsMoveInput", fileName = "MoveLocalTowardsMoveInput")]
    public class MoveLocalTowardsMoveInputInstructionTileInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveLocalTowardsMoveInputInstructionTile();
        [SerializeField] protected string _id = MoveLocalTowardsMoveInputInstructionTile.ID;
        [SerializeField] protected string _name = MoveLocalTowardsMoveInputInstructionTile.NAME;
        [SerializeField] protected string _category = MoveLocalTowardsMoveInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
