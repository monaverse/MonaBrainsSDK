using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/ChangeBodyArray", fileName = "ChangeBodyArray")]
    public class ChangeBodyArrayInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeBodyArrayInstructionTile();
        [SerializeField] protected string _id = ChangeBodyArrayInstructionTile.ID;
        [SerializeField] protected string _name = ChangeBodyArrayInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeBodyArrayInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
