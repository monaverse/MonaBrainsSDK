using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveForward", fileName = "MoveForward")]
    public class MoveForwardInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveForwardInstructionTile();
        [SerializeField] protected string _id = MoveForwardInstructionTile.ID;
        [SerializeField] protected string _name = MoveForwardInstructionTile.NAME;
        [SerializeField] protected string _category = MoveForwardInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
