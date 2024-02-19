using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Character.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Character/WearAsset", fileName = "WearAsset")]
    public class WearAssetInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new WearAssetInstructionTile();
        [SerializeField] protected string _id = WearAssetInstructionTile.ID;
        [SerializeField] protected string _name = WearAssetInstructionTile.NAME;
        [SerializeField] protected string _category = WearAssetInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
