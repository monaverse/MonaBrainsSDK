using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Input.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Input/DisablePlayerInput", fileName = "DisablePlayerInput")]
    public class DisablePlayerInputInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new DisablePlayerInputInstructionTile();
        [SerializeField] protected string _id = DisablePlayerInputInstructionTile.ID;
        [SerializeField] protected string _name = DisablePlayerInputInstructionTile.NAME;
        [SerializeField] protected string _category = DisablePlayerInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
