using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/ToggleVariableUIDisplay", fileName = "ToggleVariableUIDisplay")]
    public class ToggleVariableUIDisplayInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ToggleVariableUIDisplayInstructionTile();
        [SerializeField] protected string _id = ToggleVariableUIDisplayInstructionTile.ID;
        [SerializeField] protected string _name = ToggleVariableUIDisplayInstructionTile.NAME;
        [SerializeField] protected string _category = ToggleVariableUIDisplayInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
