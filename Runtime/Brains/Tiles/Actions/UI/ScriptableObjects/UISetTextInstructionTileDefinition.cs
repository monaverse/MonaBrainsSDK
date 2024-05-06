using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.UI.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/UI/UISetText", fileName = "UISetText")]
    public class UISetTextInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UISetTextInstructionTile();
        [SerializeField] protected string _id = UISetTextInstructionTile.ID;
        [SerializeField] protected string _name = UISetTextInstructionTile.NAME;
        [SerializeField] protected string _category = UISetTextInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
