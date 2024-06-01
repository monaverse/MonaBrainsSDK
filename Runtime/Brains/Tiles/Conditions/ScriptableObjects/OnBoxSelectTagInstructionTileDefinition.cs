using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnBoxSelectTag", fileName = "OnBoxSelectTag")]
    public class OnBoxSelectTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnBoxSelectTagInstructionTile();
        [SerializeField] protected string _id = OnBoxSelectTagInstructionTile.ID;
        [SerializeField] protected string _name = OnBoxSelectTagInstructionTile.NAME;
        [SerializeField] protected string _category = OnBoxSelectTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
