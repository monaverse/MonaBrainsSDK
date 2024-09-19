using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Audio.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Audio/MuteAudio", fileName = "MuteAudio")]
    public class MuteAudioInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MuteAudioInstructionTile();
        [SerializeField] protected string _id = MuteAudioInstructionTile.ID;
        [SerializeField] protected string _name = MuteAudioInstructionTile.NAME;
        [SerializeField] protected string _category = MuteAudioInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
