using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.General.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/General/SpawnAsset", fileName = "SpawnAsset")]
    public class SpawnAssetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new SpawnAssetInstructionTile();
        [SerializeField] protected string _id = SpawnAssetInstructionTile.ID;
        [SerializeField] protected string _name = SpawnAssetInstructionTile.NAME;
        [SerializeField] protected string _category = SpawnAssetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
