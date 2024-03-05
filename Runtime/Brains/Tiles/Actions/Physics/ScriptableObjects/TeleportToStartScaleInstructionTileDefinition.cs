using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/TeleportToStartScale", fileName = "TeleportToStartScale")]
    public class TeleportToStartScaleInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new TeleportToStartScaleInstructionTile();
        [SerializeField] protected string _id = TeleportToStartScaleInstructionTile.ID;
        [SerializeField] protected string _name = TeleportToStartScaleInstructionTile.NAME;
        [SerializeField] protected string _category = TeleportToStartScaleInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
