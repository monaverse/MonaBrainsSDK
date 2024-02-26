using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Animations.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Animations/SetAnimatorInteger", fileName = "SetAnimatorInteger")]
    public class SetAnimatorIntegerInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetAnimatorIntegerInstructionTile();
        [SerializeField] protected string _id = SetAnimatorIntegerInstructionTile.ID;
        [SerializeField] protected string _name = SetAnimatorIntegerInstructionTile.NAME;
        [SerializeField] protected string _category = SetAnimatorIntegerInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
