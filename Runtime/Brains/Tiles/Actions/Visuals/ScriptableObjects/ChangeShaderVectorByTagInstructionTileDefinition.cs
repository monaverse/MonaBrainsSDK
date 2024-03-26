using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ChangeShaderVectorByTag", fileName = "ChangeShaderVectorByTag")]
    public class ChangeShaderVectorByTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeShaderVectorByTagInstructionTile();
        [SerializeField] protected string _id = ChangeShaderVectorByTagInstructionTile.ID;
        [SerializeField] protected string _name = ChangeShaderVectorByTagInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeShaderVectorByTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
