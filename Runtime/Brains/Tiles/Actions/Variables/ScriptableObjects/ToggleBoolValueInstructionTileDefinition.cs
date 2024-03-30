using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Booleans/ToggleBoolValue", fileName = "ToggleBoolValue")]
    public class ToggleBoolValueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ToggleBoolValueInstructionTile();
        [SerializeField] protected string _id = ToggleBoolValueInstructionTile.ID;
        [SerializeField] protected string _name = ToggleBoolValueInstructionTile.NAME;
        [SerializeField] protected string _category = ToggleBoolValueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
