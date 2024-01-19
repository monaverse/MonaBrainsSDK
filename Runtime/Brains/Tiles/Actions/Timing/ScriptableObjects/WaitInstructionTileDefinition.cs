using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Timing.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Timing/Wait", fileName = "Wait")]
    public class WaitInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new WaitInstructionTile();
        [SerializeField] protected string _id = WaitInstructionTile.ID;
        [SerializeField] protected string _name = WaitInstructionTile.NAME;
        [SerializeField] protected string _category = WaitInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
