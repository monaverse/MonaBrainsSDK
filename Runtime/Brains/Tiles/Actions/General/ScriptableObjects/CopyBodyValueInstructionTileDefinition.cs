using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/CopyBodyValue", fileName = "CopyBodyValue")]
    public class CopyBodyValueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new CopyBodyValueInstructionTile();
        [SerializeField] protected string _id = CopyBodyValueInstructionTile.ID;
        [SerializeField] protected string _name = CopyBodyValueInstructionTile.NAME;
        [SerializeField] protected string _category = CopyBodyValueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}