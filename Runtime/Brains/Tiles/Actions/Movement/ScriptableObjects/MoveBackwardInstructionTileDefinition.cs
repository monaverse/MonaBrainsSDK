using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using Mona.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveBackward", fileName = "MoveBackward")]
    public class MoveBackwardInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveBackwardInstructionTile();
        [SerializeField] protected string _id = MoveBackwardInstructionTile.ID;
        [SerializeField] protected string _name = MoveBackwardInstructionTile.NAME;
        [SerializeField] protected string _category = MoveBackwardInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
