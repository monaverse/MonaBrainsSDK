using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Input.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Input/EnablePlayerInput", fileName = "EnablePlayerInput")]
    public class EnablePlayerInputInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new EnablePlayerInputInstructionTile();
        [SerializeField] protected string _id = EnablePlayerInputInstructionTile.ID;
        [SerializeField] protected string _name = EnablePlayerInputInstructionTile.NAME;
        [SerializeField] protected string _category = EnablePlayerInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
