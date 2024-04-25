using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnSelectToken", fileName = "OnSelectToken")]
    public class OnSelectTokenInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnSelectTokenInstructionTile();
        [SerializeField] protected string _id = OnSelectTokenInstructionTile.ID;
        [SerializeField] protected string _name = OnSelectTokenInstructionTile.NAME;
        [SerializeField] protected string _category = OnSelectTokenInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
