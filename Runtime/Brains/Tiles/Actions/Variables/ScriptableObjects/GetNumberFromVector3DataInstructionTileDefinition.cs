using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/Numbers/GetNumberFromVector3Data", fileName = "GetNumberFromVector3Data")]
    public class GetNumberFromVector3DataInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GetNumberFromVector3DataInstructionTile();
        [SerializeField] protected string _id = GetNumberFromVector3DataInstructionTile.ID;
        [SerializeField] protected string _name = GetNumberFromVector3DataInstructionTile.NAME;
        [SerializeField] protected string _category = GetNumberFromVector3DataInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
