using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Visuals.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Visuals/ChangeSpace", fileName = "ChangeSpace")]
    public class ChangeSpaceInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new ChangeSpaceInstructionTile();
        [SerializeField] protected string _id = ChangeSpaceInstructionTile.ID;
        [SerializeField] protected string _name = ChangeSpaceInstructionTile.NAME;
        [SerializeField] protected string _category = ChangeSpaceInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
