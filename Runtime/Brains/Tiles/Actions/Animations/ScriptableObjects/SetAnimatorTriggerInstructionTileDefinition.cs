using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Animations.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Animations/SetAnimatorTrigger", fileName = "SetAnimatorTrigger")]
    public class SetAnimatorTriggerInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetAnimatorTriggerInstructionTile();
        [SerializeField] protected string _id = SetAnimatorTriggerInstructionTile.ID;
        [SerializeField] protected string _name = SetAnimatorTriggerInstructionTile.NAME;
        [SerializeField] protected string _category = SetAnimatorTriggerInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
