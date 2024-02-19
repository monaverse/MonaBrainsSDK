using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Character.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Character/EquipAsset", fileName = "EquipAsset")]
    public class EquipAssetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new EquipAssetInstructionTile();
        [SerializeField] protected string _id = EquipAssetInstructionTile.ID;
        [SerializeField] protected string _name = EquipAssetInstructionTile.NAME;
        [SerializeField] protected string _category = EquipAssetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
