using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Audio.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Audio/GetVolume", fileName = "GetVolume")]
    public class GetVolumeInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new GetVolumeInstructionTile();
        [SerializeField] protected string _id = GetVolumeInstructionTile.ID;
        [SerializeField] protected string _name = GetVolumeInstructionTile.NAME;
        [SerializeField] protected string _category = GetVolumeInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
