using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/General/GetMultiBodyValue", fileName = "GetMultiBodyValue")]
    public class GetMultiBodyValueValueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GetMultiBodyValueInstructionTile();
        [SerializeField] protected string _id = GetMultiBodyValueInstructionTile.ID;
        [SerializeField] protected string _name = GetMultiBodyValueInstructionTile.NAME;
        [SerializeField] protected string _category = GetMultiBodyValueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
