using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/BindRotationY", fileName = "BindRotationY")]
    public class BindRotationYInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new BindRotationYInstructionTile();
        [SerializeField] protected string _id = BindRotationYInstructionTile.ID;
        [SerializeField] protected string _name = BindRotationYInstructionTile.NAME;
        [SerializeField] protected string _category = BindRotationYInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
