using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/MoveRelativeToCamera", fileName = "MoveRelativeToCamera")]
    public class MoveRelativeToCameraInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new MoveRelativeToCameraInstructionTile();
        [SerializeField] protected string _id = MoveRelativeToCameraInstructionTile.ID;
        [SerializeField] protected string _name = MoveRelativeToCameraInstructionTile.NAME;
        [SerializeField] protected string _category = MoveRelativeToCameraInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
