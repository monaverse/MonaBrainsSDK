using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/BindRotationZ", fileName = "BindRotationZ")]
    public class BindRotationZInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new BindRotationZInstructionTile();
        [SerializeField] protected string _id = BindRotationZInstructionTile.ID;
        [SerializeField] protected string _name = BindRotationZInstructionTile.NAME;
        [SerializeField] protected string _category = BindRotationZInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
