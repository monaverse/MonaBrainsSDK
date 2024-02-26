using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Animations.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Animations/SetAnimatorBool", fileName = "SetAnimatorBool")]
    public class SetAnimatorBoolInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetAnimatorBoolInstructionTile();
        [SerializeField] protected string _id = SetAnimatorBoolInstructionTile.ID;
        [SerializeField] protected string _name = SetAnimatorBoolInstructionTile.NAME;
        [SerializeField] protected string _category = SetAnimatorBoolInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
