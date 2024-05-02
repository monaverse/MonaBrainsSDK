using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/GetVariableOnType", fileName = "GetVariableOnType")]
    public class GetVariableOnTypeInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GetVariableOnTypeInstructionTile();
        [SerializeField] protected string _id = GetVariableOnTypeInstructionTile.ID;
        [SerializeField] protected string _name = GetVariableOnTypeInstructionTile.NAME;
        [SerializeField] protected string _category = GetVariableOnTypeInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
