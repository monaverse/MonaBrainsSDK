using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/AddToNumber", fileName = "AddToNumber")]
    public class AddToNumberInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new AddToNumberInstructionTile();
        [SerializeField] protected string _id = AddToNumberInstructionTile.ID;
        [SerializeField] protected string _name = AddToNumberInstructionTile.NAME;
        [SerializeField] protected string _category = AddToNumberInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
