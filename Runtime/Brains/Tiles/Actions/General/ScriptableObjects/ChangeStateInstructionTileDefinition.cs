using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/ChangeState", fileName = "ChangeState")]
    public class ChangeStateInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeStateInstructionTile();
        [SerializeField] protected string _id = ChangeStateInstructionTile.ID;
        [SerializeField] protected string _name = ChangeStateInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeStateInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
