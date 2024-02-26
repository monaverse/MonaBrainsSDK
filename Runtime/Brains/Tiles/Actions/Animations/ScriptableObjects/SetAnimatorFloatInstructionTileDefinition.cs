using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Animations.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Animations/SetAnimatorFloat", fileName = "SetAnimatorFloat")]
    public class SetAnimatorFloatInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetAnimatorFloatInstructionTile();
        [SerializeField] protected string _id = SetAnimatorFloatInstructionTile.ID;
        [SerializeField] protected string _name = SetAnimatorFloatInstructionTile.NAME;
        [SerializeField] protected string _category = SetAnimatorFloatInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
