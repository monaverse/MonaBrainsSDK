using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/TeleportToStartRotation", fileName = "TeleportToStartRotation")]
    public class TeleportToStartRotationInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new TeleportToStartRotationInstructionTile();
        [SerializeField] protected string _id = TeleportToStartRotationInstructionTile.ID;
        [SerializeField] protected string _name = TeleportToStartRotationInstructionTile.NAME;
        [SerializeField] protected string _category = TeleportToStartRotationInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
