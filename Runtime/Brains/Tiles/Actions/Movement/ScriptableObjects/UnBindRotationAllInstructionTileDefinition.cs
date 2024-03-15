using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Binding/UnBindRotationAll", fileName = "UnBindRotationAll")]
    public class UnBindRotationllInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UnBindRotationAllInstructionTile();
        [SerializeField] protected string _id = UnBindRotationAllInstructionTile.ID;
        [SerializeField] protected string _name = UnBindRotationAllInstructionTile.NAME;
        [SerializeField] protected string _category = UnBindRotationAllInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
