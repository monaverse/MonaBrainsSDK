using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/NotGrounded", fileName = "NotGrounded")]
    public class NotGroundedInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new NotGroundedInstructionTile();
        [SerializeField] protected string _id = NotGroundedInstructionTile.ID;
        [SerializeField] protected string _name = NotGroundedInstructionTile.NAME;
        [SerializeField] protected string _category = NotGroundedInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
