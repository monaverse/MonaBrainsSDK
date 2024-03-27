using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/SetVariableOnTag", fileName = "SetVariableOnTag")]
    public class SetVariableOnTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetVariableOnTagInstructionTile();
        [SerializeField] protected string _id = SetVariableOnTagInstructionTile.ID;
        [SerializeField] protected string _name = SetVariableOnTagInstructionTile.NAME;
        [SerializeField] protected string _category = SetVariableOnTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
