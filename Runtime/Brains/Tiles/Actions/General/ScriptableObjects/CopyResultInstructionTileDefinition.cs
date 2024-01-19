using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/CopyResult", fileName = "CopyResult")]
    public class CopyResultInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new CopyResultInstructionTile();
        [SerializeField] protected string _id = CopyResultInstructionTile.ID;
        [SerializeField] protected string _name = CopyResultInstructionTile.NAME;
        [SerializeField] protected string _category = CopyResultInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
