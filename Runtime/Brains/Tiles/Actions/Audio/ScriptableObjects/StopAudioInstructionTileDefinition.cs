using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Audio.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Audio/StopAudio", fileName = "StopAudio")]
    public class StopAudioInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new StopAudioInstructionTile();
        [SerializeField] protected string _id = StopAudioInstructionTile.ID;
        [SerializeField] protected string _name = StopAudioInstructionTile.NAME;
        [SerializeField] protected string _category = StopAudioInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
