using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/CopyUrl", fileName = "CopyUrl")]
    public class CopyUrlInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new CopyUrlInstructionTile();
        [SerializeField] protected string _id = CopyUrlInstructionTile.ID;
        [SerializeField] protected string _name = CopyUrlInstructionTile.NAME;
        [SerializeField] protected string _category = CopyUrlInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
