using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/HideTarget", fileName = "HideTarget")]
    public class HideTargetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new HideTargetInstructionTile();
        [SerializeField] protected string _id = HideTargetInstructionTile.ID;
        [SerializeField] protected string _name = HideTargetInstructionTile.NAME;
        [SerializeField] protected string _category = HideTargetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
