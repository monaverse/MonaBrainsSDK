using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/UnBindPositionZ", fileName = "UnBindPositionZ")]
    public class UnBindPositionZInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UnBindPositionZInstructionTile();
        [SerializeField] protected string _id = UnBindPositionZInstructionTile.ID;
        [SerializeField] protected string _name = UnBindPositionZInstructionTile.NAME;
        [SerializeField] protected string _category = UnBindPositionZInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
