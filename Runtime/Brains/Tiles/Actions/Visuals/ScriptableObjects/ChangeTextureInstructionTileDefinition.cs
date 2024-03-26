using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ChangeTexture", fileName = "ChangeTexture")]
    public class ChangeTextureInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeTextureInstructionTile();
        [SerializeField] protected string _id = ChangeTextureInstructionTile.ID;
        [SerializeField] protected string _name = ChangeTextureInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeTextureInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
