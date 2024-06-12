using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/Torque/ApplyTorquePitchDown", fileName = "ApplyTorquePitchDown")]
    public class ApplyTorquePitchDownInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ApplyTorquePitchDownInstructionTile();
        [SerializeField] protected string _id = ApplyTorquePitchDownInstructionTile.ID;
        [SerializeField] protected string _name = ApplyTorquePitchDownInstructionTile.NAME;
        [SerializeField] protected string _category = ApplyTorquePitchDownInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
