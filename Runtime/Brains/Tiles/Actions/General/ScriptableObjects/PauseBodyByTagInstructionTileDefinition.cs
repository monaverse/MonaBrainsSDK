using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/PauseBodyByTag", fileName = "PauseBodyByTag")]
    public class PauseBodyByTagInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new PauseBodyByTagInstructionTile();
        [SerializeField] protected string _id = PauseBodyByTagInstructionTile.ID;
        [SerializeField] protected string _name = PauseBodyByTagInstructionTile.NAME;
        [SerializeField] protected string _category = PauseBodyByTagInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
