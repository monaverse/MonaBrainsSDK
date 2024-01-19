using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/GlueToMonaPlayer", fileName = "GlueToMonaPlayer")]
    public class GlueToMonaPlayerInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GlueToMonaPlayerInstructionTile();
        [SerializeField] protected string _id = GlueToMonaPlayerInstructionTile.ID;
        [SerializeField] protected string _name = GlueToMonaPlayerInstructionTile.NAME;
        [SerializeField] protected string _category = GlueToMonaPlayerInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
