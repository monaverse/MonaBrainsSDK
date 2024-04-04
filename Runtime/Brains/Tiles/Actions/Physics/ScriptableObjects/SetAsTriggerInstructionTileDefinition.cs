using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Physics.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Physics/SetAsTrigger", fileName = "SetAsTrigger")]
    public class SetAsTriggerInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SetAsTriggerInstructionTile();
        [SerializeField] protected string _id = SetAsTriggerInstructionTile.ID;
        [SerializeField] protected string _name = SetAsTriggerInstructionTile.NAME;
        [SerializeField] protected string _category = SetAsTriggerInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
