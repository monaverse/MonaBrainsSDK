using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnInput", fileName = "OnInput")]
    public class OnInputInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnInputInstructionTile();
        [SerializeField] protected string _id = OnInputInstructionTile.ID;
        [SerializeField] protected string _name = OnInputInstructionTile.NAME;
        [SerializeField] protected string _category = OnInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
