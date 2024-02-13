using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Audio.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Audio/PlayAudio", fileName = "PlayAudio")]
    public class PlayAudioInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PlayAudioInstructionTile();
        [SerializeField] protected string _id = PlayAudioInstructionTile.ID;
        [SerializeField] protected string _name = PlayAudioInstructionTile.NAME;
        [SerializeField] protected string _category = PlayAudioInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
