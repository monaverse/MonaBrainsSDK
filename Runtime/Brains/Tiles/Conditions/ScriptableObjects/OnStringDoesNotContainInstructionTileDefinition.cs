using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnStringDoesNotContain", fileName = "OnStringDoesNotContain")]
    public class OnStringDoesNotContainInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnStringDoesNotContainInstructionTile();
        [SerializeField] protected string _id = OnStringDoesNotContainInstructionTile.ID;
        [SerializeField] protected string _name = OnStringDoesNotContainInstructionTile.NAME;
        [SerializeField] protected string _category = OnStringDoesNotContainInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
