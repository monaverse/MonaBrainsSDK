using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Binding/UnBindRotationZ", fileName = "UnBindRotationZ")]
    public class UnBindRotationZInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UnBindRotationZInstructionTile();
        [SerializeField] protected string _id = UnBindRotationZInstructionTile.ID;
        [SerializeField] protected string _name = UnBindRotationZInstructionTile.NAME;
        [SerializeField] protected string _category = UnBindRotationZInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
