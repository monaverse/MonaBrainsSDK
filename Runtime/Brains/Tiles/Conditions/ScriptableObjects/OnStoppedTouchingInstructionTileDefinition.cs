using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/OnStoppedTouching", fileName = "OnStoppedTouching")]
    public class OnStoppedTouchingInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnStoppedTouchingInstructionTile();
        [SerializeField] protected string _id = OnStoppedTouchingInstructionTile.ID;
        [SerializeField] protected string _name = OnStoppedTouchingInstructionTile.NAME;
        [SerializeField] protected string _category = OnStoppedTouchingInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
