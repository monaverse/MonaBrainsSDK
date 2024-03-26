using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ChangeShaderFloat", fileName = "ChangeShaderFloat")]
    public class ChangeShaderFloatInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeShaderFloatInstructionTile();
        [SerializeField] protected string _id = ChangeShaderFloatInstructionTile.ID;
        [SerializeField] protected string _name = ChangeShaderFloatInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeShaderFloatInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
