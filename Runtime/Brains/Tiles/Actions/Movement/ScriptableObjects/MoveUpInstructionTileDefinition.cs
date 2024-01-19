using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using Mona.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveUp", fileName = "MoveUp")]
    public class MoveUpInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveUpInstructionTile();
        [SerializeField] protected string _id = MoveUpInstructionTile.ID;
        [SerializeField] protected string _name = MoveUpInstructionTile.NAME;
        [SerializeField] protected string _category = MoveUpInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
