using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Extensions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Extensions/VisualScripting", fileName = "VisualScripting")]
    public class VisualScriptingInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new VisualScriptingInstructionTile();
        [SerializeField] protected string _id = VisualScriptingInstructionTile.ID;
        [SerializeField] protected string _name = VisualScriptingInstructionTile.NAME;
        [SerializeField] protected string _category = VisualScriptingInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
