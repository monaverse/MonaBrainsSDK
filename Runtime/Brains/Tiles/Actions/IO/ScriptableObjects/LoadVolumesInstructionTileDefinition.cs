using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.IO.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/IO/LoadVolumes", fileName = "LoadVolumes")]
    public class LoadVolumesInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new LoadVolumesInstructionTile();
        [SerializeField] protected string _id = LoadVolumesInstructionTile.ID;
        [SerializeField] protected string _name = LoadVolumesInstructionTile.NAME;
        [SerializeField] protected string _category = LoadVolumesInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
