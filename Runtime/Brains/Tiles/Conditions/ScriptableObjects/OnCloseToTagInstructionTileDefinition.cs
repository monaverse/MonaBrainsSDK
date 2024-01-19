using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnCloseToTag", fileName = "OnCloseToTag")]
    public class OnCloseToTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnCloseToTagInstructionTile();
        [SerializeField] protected string _id = OnCloseToTagInstructionTile.ID;
        [SerializeField] protected string _name = OnCloseToTagInstructionTile.NAME;
        [SerializeField] protected string _category = OnCloseToTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
