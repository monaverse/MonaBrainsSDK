using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Broadcasting/BroadcastMessageToTarget", fileName = "BroadcastMessageToTarget")]
    public class BroadcastMessageToTargetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new BroadcastMessageToTargetInstructionTile();
        [SerializeField] protected string _id = BroadcastMessageToTargetInstructionTile.ID;
        [SerializeField] protected string _name = BroadcastMessageToTargetInstructionTile.NAME;
        [SerializeField] protected string _category = BroadcastMessageToTargetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
