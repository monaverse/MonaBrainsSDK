using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Strings/StringEquals", fileName = "StringEquals")]
    public class StringEqualsInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new StringEqualsInstructionTile();
        [SerializeField] protected string _id = StringEqualsInstructionTile.ID;
        [SerializeField] protected string _name = StringEqualsInstructionTile.NAME;
        [SerializeField] protected string _category = StringEqualsInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
