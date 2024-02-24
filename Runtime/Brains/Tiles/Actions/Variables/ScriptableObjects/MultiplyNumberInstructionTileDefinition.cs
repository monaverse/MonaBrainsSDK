using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/MultiplyNumber", fileName = "MultiplyNumber")]
    public class MultiplyNumberInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MultiplyNumberInstructionTile();
        [SerializeField] protected string _id = MultiplyNumberInstructionTile.ID;
        [SerializeField] protected string _name = MultiplyNumberInstructionTile.NAME;
        [SerializeField] protected string _category = MultiplyNumberInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
