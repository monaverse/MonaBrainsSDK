using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveAlongXNegative", fileName = "MoveAlongXNegative")]
    public class MoveAlongXNegativeInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveAlongXNegativeInstructionTile();
        [SerializeField] protected string _id = MoveAlongXNegativeInstructionTile.ID;
        [SerializeField] protected string _name = MoveAlongXNegativeInstructionTile.NAME;
        [SerializeField] protected string _category = MoveAlongXNegativeInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
