using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Linear/ApplyForceNadir", fileName = "ApplyForceNadir")]
    public class ApplyForceNadirInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyForceNadirInstructionTile();
        [SerializeField] protected string _id = ApplyForceNadirInstructionTile.ID;
        [SerializeField] protected string _name = ApplyForceNadirInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyForceNadirInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
