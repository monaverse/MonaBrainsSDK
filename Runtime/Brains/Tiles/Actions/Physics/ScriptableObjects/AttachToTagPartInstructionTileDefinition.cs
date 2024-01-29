using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/AttachToTagPart", fileName = "AttachToTagPart")]
    public class AttachToTagPartInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new AttachToTagPartInstructionTile();
        [SerializeField] protected string _id = AttachToTagPartInstructionTile.ID;
        [SerializeField] protected string _name = AttachToTagPartInstructionTile.NAME;
        [SerializeField] protected string _category = AttachToTagPartInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
