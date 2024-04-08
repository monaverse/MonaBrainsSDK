using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Variables.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Variables/Vector3/Vector3Modify", fileName = "Vector3Modify")]
    public class Vector3ModifyInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new Vector3ModifyInstructionTile();
        [SerializeField] protected string _id = Vector3ModifyInstructionTile.ID;
        [SerializeField] protected string _name = Vector3ModifyInstructionTile.NAME;
        [SerializeField] protected string _category = Vector3ModifyInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
