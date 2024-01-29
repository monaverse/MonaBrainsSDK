using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.General;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/AttachToPlayerPart", fileName = "AttachToPlayerPart")]
    public class AttachToPlayerPartInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new AttachToPlayerPartInstructionTile();
        [SerializeField] protected string _id = AttachToPlayerPartInstructionTile.ID;
        [SerializeField] protected string _name = AttachToPlayerPartInstructionTile.NAME;
        [SerializeField] protected string _category = AttachToPlayerPartInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
