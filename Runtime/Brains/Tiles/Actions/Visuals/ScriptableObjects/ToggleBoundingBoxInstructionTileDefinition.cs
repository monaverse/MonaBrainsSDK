using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ToggleBoundingBox", fileName = "ToggleBoundingBox")]
    public class ToggleBoundingBoxInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ToggleBoundingBoxInstructionTile();
        [SerializeField] protected string _id = ToggleBoundingBoxInstructionTile.ID;
        [SerializeField] protected string _name = ToggleBoundingBoxInstructionTile.NAME;
        [SerializeField] protected string _category = ToggleBoundingBoxInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
