using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ChangeShaderVector", fileName = "ChangeShaderVector")]
    public class ChangeShaderVectorInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeShaderVectorInstructionTile();
        [SerializeField] protected string _id = ChangeShaderVectorInstructionTile.ID;
        [SerializeField] protected string _name = ChangeShaderVectorInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeShaderVectorInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
