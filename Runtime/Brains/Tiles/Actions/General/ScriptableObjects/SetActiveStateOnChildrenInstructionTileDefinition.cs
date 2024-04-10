using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Activation/SetActiveStateOnChildren", fileName = "SetActiveStateOnChildren")]
    public class SetActiveStateOnChildrenInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetActiveStateOnChildrenInstructionTile();
        [SerializeField] protected string _id = SetActiveStateOnChildrenInstructionTile.ID;
        [SerializeField] protected string _name = SetActiveStateOnChildrenInstructionTile.NAME;
        [SerializeField] protected string _category = SetActiveStateOnChildrenInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
