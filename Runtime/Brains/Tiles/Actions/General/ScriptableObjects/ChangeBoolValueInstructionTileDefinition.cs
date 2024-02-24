using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/ChangeBoolValue", fileName = "ChangeBoolValue")]
    public class ChangeBoolValueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeBoolValueInstructionTile();
        [SerializeField] protected string _id = ChangeBoolValueInstructionTile.ID;
        [SerializeField] protected string _name = ChangeBoolValueInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeBoolValueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
