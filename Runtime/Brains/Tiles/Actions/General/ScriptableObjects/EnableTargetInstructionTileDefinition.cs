using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/EnableTarget", fileName = "EnableTarget")]
    public class EnableTargetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new EnableTargetInstructionTile();
        [SerializeField] protected string _id = EnableTargetInstructionTile.ID;
        [SerializeField] protected string _name = EnableTargetInstructionTile.NAME;
        [SerializeField] protected string _category = EnableTargetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
