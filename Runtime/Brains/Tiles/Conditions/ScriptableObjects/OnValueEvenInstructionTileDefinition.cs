using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnValueEven", fileName = "OnValueEven")]
    public class OnValueEvenInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnValueEvenInstructionTile();
        [SerializeField] protected string _id = OnValueEvenInstructionTile.ID;
        [SerializeField] protected string _name = OnValueEvenInstructionTile.NAME;
        [SerializeField] protected string _category = OnValueEvenInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
