using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.IO.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/IO/SaveStoredData", fileName = "SaveStoredData")]
    public class SaveStoredDataInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SaveStoredDataInstructionTile();
        [SerializeField] protected string _id = SaveStoredDataInstructionTile.ID;
        [SerializeField] protected string _name = SaveStoredDataInstructionTile.NAME;
        [SerializeField] protected string _category = SaveStoredDataInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
