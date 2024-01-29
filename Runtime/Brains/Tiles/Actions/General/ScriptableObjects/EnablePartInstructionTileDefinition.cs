using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/EnablePart", fileName = "EnablePart")]
    public class EnablePartInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new EnablePartInstructionTile();
        [SerializeField] protected string _id = EnablePartInstructionTile.ID;
        [SerializeField] protected string _name = EnablePartInstructionTile.NAME;
        [SerializeField] protected string _category = EnablePartInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
