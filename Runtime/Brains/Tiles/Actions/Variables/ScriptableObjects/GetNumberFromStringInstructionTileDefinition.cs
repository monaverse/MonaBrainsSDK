using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Strings/GetStringNumbers", fileName = "GetStringNumbers")]
    public class GetNumberFromStringInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GetNumberFromStringInstructionTile();
        [SerializeField] protected string _id = GetNumberFromStringInstructionTile.ID;
        [SerializeField] protected string _name = GetNumberFromStringInstructionTile.NAME;
        [SerializeField] protected string _category = GetNumberFromStringInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
