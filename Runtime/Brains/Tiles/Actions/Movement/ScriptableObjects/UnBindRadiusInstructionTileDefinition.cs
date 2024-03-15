using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Binding/UnBindRadius", fileName = "UnBindRadius")]
    public class UnBindRadiusInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UnBindRadiusInstructionTile();
        [SerializeField] protected string _id = UnBindRadiusInstructionTile.ID;
        [SerializeField] protected string _name = UnBindRadiusInstructionTile.NAME;
        [SerializeField] protected string _category = UnBindRadiusInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
