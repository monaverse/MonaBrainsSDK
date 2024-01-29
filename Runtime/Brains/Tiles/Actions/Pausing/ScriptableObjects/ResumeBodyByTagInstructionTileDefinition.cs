using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Pausing/ResumeBodyByTag", fileName = "ResumeBodyByTag")]
    public class ResumeBodyByTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ResumeBodyByTagInstructionTile();
        [SerializeField] protected string _id = ResumeBodyByTagInstructionTile.ID;
        [SerializeField] protected string _name = ResumeBodyByTagInstructionTile.NAME;
        [SerializeField] protected string _category = ResumeBodyByTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
