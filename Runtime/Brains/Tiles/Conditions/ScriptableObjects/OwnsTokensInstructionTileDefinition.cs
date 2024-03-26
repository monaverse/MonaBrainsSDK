using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OwnsTokens", fileName = "OwnsTokens")]
    public class OwnsTokensInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OwnsTokensInstructionTile();
        [SerializeField] protected string _id = OwnsTokensInstructionTile.ID;
        [SerializeField] protected string _name = OwnsTokensInstructionTile.NAME;
        [SerializeField] protected string _category = OwnsTokensInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
