using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/Numbers/NumberSetRandomSeed", fileName = "NumberSetRandomSeed")]
    public class NumberSetRandomSeedInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new NumberSetRandomSeedInstructionTile();
        [SerializeField] protected string _id = NumberSetRandomSeedInstructionTile.ID;
        [SerializeField] protected string _name = NumberSetRandomSeedInstructionTile.NAME;
        [SerializeField] protected string _category = NumberSetRandomSeedInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
