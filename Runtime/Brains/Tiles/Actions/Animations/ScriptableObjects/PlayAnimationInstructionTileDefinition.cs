using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Animations.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Animations/PlayAnimation", fileName = "PlayAnimation")]
    public class PlayAnimationInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PlayAnimationInstructionTile();
        [SerializeField] protected string _id = PlayAnimationInstructionTile.ID;
        [SerializeField] protected string _name = PlayAnimationInstructionTile.NAME;
        [SerializeField] protected string _category = PlayAnimationInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
