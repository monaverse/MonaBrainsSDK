using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Broadcasting/BroadcastMessageToSender", fileName = "BroadcastMessageToSender")]
    public class BroadcastMessageToSenderInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new BroadcastMessageToSenderInstructionTile();
        [SerializeField] protected string _id = BroadcastMessageToSenderInstructionTile.ID;
        [SerializeField] protected string _name = BroadcastMessageToSenderInstructionTile.NAME;
        [SerializeField] protected string _category = BroadcastMessageToSenderInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
