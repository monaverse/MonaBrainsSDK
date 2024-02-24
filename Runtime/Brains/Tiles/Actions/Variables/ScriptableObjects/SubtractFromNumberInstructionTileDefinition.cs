using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/SubtractFromNumber", fileName = "SubtractFromNumber")]
    public class SubtractFromNumberInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SubtractFromNumberInstructionTile();
        [SerializeField] protected string _id = SubtractFromNumberInstructionTile.ID;
        [SerializeField] protected string _name = SubtractFromNumberInstructionTile.NAME;
        [SerializeField] protected string _category = SubtractFromNumberInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
