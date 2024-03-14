using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/UnBindPositionY", fileName = "UnBindPositionY")]
    public class UnBindPositionYInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UnBindPositionYInstructionTile();
        [SerializeField] protected string _id = UnBindPositionYInstructionTile.ID;
        [SerializeField] protected string _name = UnBindPositionYInstructionTile.NAME;
        [SerializeField] protected string _category = UnBindPositionYInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
