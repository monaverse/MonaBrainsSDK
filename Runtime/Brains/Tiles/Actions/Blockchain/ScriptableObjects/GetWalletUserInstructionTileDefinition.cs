using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Blockchain.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Blockchain/GetWalletUser", fileName = "GetWalletUser")]
    public class GetWalletUserInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GetWalletUserInstructionTile();
        [SerializeField] protected string _id = GetWalletUserInstructionTile.ID;
        [SerializeField] protected string _name = GetWalletUserInstructionTile.NAME;
        [SerializeField] protected string _category = GetWalletUserInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
