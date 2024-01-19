using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveDown", fileName = "MoveDown")]
    public class MoveDownInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveDownInstructionTile();
        [SerializeField] protected string _id = MoveDownInstructionTile.ID;
        [SerializeField] protected string _name = MoveDownInstructionTile.NAME;
        [SerializeField] protected string _category = MoveDownInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
