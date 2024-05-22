using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.IO.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/IO/LoadLayout", fileName = "LoadLayout")]
    public class LoadLayoutInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new LoadLayoutInstructionTile();
        [SerializeField] protected string _id = LoadLayoutInstructionTile.ID;
        [SerializeField] protected string _name = LoadLayoutInstructionTile.NAME;
        [SerializeField] protected string _category = LoadLayoutInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
