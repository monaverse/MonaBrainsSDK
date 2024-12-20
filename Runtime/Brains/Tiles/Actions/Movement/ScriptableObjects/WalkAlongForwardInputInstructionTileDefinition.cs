using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Character/WalkAlongForwardInput", fileName = "WalkAlongForwardInput")]
    public class WalkAlongForwardInputInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new WalkAlongForwardInputInstructionTile();
        [SerializeField] protected string _id = WalkAlongForwardInputInstructionTile.ID;
        [SerializeField] protected string _name = WalkAlongForwardInputInstructionTile.NAME;
        [SerializeField] protected string _category = WalkAlongForwardInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
