using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Movement/RotateAroundInput", fileName = "RotateAroundInput")]
    public class RotateAroundInputInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new RotateAroundInputInstructionTile();
        [SerializeField] protected string _id = RotateAroundInputInstructionTile.ID;
        [SerializeField] protected string _name = RotateAroundInputInstructionTile.NAME;
        [SerializeField] protected string _category = RotateAroundInputInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
