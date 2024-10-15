using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.UI.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/UI/SetScreenJoypad", fileName = "SetScreenJoypad")]
    public class SetScreenJoypadInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetScreenJoypadInstructionTile();
        [SerializeField] protected string _id = SetScreenJoypadInstructionTile.ID;
        [SerializeField] protected string _name = SetScreenJoypadInstructionTile.NAME;
        [SerializeField] protected string _category = SetScreenJoypadInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
