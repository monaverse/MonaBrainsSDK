using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/ShowTarget", fileName = "ShowTarget")]
    public class ShowTargetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ShowTargetInstructionTile();
        [SerializeField] protected string _id = ShowTargetInstructionTile.ID;
        [SerializeField] protected string _name = ShowTargetInstructionTile.NAME;
        [SerializeField] protected string _category = ShowTargetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
