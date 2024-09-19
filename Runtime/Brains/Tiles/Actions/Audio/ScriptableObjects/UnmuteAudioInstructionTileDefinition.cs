using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Audio.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Audio/UnmuteAudio", fileName = "UnmuteAudio")]
    public class UnmuteAudioInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new UnmuteAudioInstructionTile();
        [SerializeField] protected string _id = UnmuteAudioInstructionTile.ID;
        [SerializeField] protected string _name = UnmuteAudioInstructionTile.NAME;
        [SerializeField] protected string _category = UnmuteAudioInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
