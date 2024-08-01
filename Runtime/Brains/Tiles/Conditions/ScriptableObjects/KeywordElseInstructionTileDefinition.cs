using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/Keywords/Else", fileName = "Else")]
    public class KeywordElseInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new KeywordElseInstructionTile();
        [SerializeField] protected string _id = KeywordElseInstructionTile.ID;
        [SerializeField] protected string _name = KeywordElseInstructionTile.NAME;
        [SerializeField] protected string _category = KeywordElseInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    }
}
