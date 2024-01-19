using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/DissolveGlue", fileName = "DissolveGlue")]
    public class DissolveGlueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new DissolveGlueInstructionTile();
        [SerializeField] protected string _id = DissolveGlueInstructionTile.ID;
        [SerializeField] protected string _name = DissolveGlueInstructionTile.NAME;
        [SerializeField] protected string _category = DissolveGlueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
