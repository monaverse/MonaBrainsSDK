using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.IO.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/IO/DeleteAllData", fileName = "DeleteAllData")]
    public class DeleteAllDataInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new DeleteAllDataInstructionTile();
        [SerializeField] protected string _id = DeleteAllDataInstructionTile.ID;
        [SerializeField] protected string _name = DeleteAllDataInstructionTile.NAME;
        [SerializeField] protected string _category = DeleteAllDataInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
