using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Pausing/ResumeTarget", fileName = "ResumeTarget")]
    public class ResumeTargetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ResumeTargetInstructionTile();
        [SerializeField] protected string _id = ResumeTargetInstructionTile.ID;
        [SerializeField] protected string _name = ResumeTargetInstructionTile.NAME;
        [SerializeField] protected string _category = ResumeTargetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
