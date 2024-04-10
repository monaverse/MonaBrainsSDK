using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Audio.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Audio/PlayNextAudio", fileName = "PlayNextAudio")]
    public class PlayNextAudioInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PlayNextAudioInstructionTile();
        [SerializeField] protected string _id = PlayNextAudioInstructionTile.ID;
        [SerializeField] protected string _name = PlayNextAudioInstructionTile.NAME;
        [SerializeField] protected string _category = PlayNextAudioInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
