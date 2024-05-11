using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ChangeMesh", fileName = "ChangeMesh")]
    public class ChangeMeshInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeMeshInstructionTile();
        [SerializeField] protected string _id = ChangeMeshInstructionTile.ID;
        [SerializeField] protected string _name = ChangeMeshInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeMeshInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
