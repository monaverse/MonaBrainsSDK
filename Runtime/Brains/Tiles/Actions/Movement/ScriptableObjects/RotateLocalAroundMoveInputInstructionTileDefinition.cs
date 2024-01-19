using Mona.Brains.Core.Tiles;
using Mona.Brains.Core.Tiles.ScriptableObjects;
using Mona.Brains.Tiles.Actions.Movement;
using System;
using UnityEngine;

namespace Mona.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/RotateLocalAroundMoveInput", fileName = "RotateLocalAroundMoveInput")]
    public class RotateLocalAroundMoveInputInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new RotateLocalAroundMoveInputInstructionTile();
        [SerializeField] protected string _id = RotateLocalAroundMoveInputInstructionTile.ID;
        [SerializeField] protected string _name = RotateLocalAroundMoveInputInstructionTile.NAME;
        [SerializeField] protected string _category = RotateLocalAroundMoveInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
