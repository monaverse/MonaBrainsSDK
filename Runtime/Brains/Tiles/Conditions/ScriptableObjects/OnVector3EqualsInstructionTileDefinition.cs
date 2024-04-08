using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnVector3Equals", fileName = "OnVector3Equals")]
    public class OnVector3EqualsInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnVector3EqualsInstructionTile();
        [SerializeField] protected string _id = OnVector3EqualsInstructionTile.ID;
        [SerializeField] protected string _name = OnVector3EqualsInstructionTile.NAME;
        [SerializeField] protected string _category = OnVector3EqualsInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
