using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/SetVariableOnSelf", fileName = "SetVariableOnSelf")]
    public class SetVariableOnSelfInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetVariableOnSelfInstructionTile();
        [SerializeField] protected string _id = SetVariableOnSelfInstructionTile.ID;
        [SerializeField] protected string _name = SetVariableOnSelfInstructionTile.NAME;
        [SerializeField] protected string _category = SetVariableOnSelfInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
