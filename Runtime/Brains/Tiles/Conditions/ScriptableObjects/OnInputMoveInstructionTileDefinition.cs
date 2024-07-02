using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Conditions.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Conditions/Input/OnInputMove", fileName = "OnInputMove")]
    public class OnInputMoveInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new OnInputMoveInstructionTile();
        [SerializeField] protected string _id = OnInputMoveInstructionTile.ID;
        [SerializeField] protected string _name = OnInputMoveInstructionTile.NAME;
        [SerializeField] protected string _category = OnInputMoveInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
