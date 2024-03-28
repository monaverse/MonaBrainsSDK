using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/SpawnNextAsset", fileName = "SpawnNextAsset")]
    public class SpawnNextAssetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SpawnNextAssetInstructionTile();
        [SerializeField] protected string _id = SpawnNextAssetInstructionTile.ID;
        [SerializeField] protected string _name = SpawnNextAssetInstructionTile.NAME;
        [SerializeField] protected string _category = SpawnNextAssetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
