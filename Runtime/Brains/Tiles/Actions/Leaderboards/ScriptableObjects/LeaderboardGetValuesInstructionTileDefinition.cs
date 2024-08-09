using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Leaderboards/GetLeaderboardValues", fileName = "GetLeaderboardValues")]
    public class LeaderboardGetValuesInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new LeaderboardGetValuesInstructionTile();
        [SerializeField] protected string _id = LeaderboardGetValuesInstructionTile.ID;
        [SerializeField] protected string _name = LeaderboardGetValuesInstructionTile.NAME;
        [SerializeField] protected string _category = LeaderboardGetValuesInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
