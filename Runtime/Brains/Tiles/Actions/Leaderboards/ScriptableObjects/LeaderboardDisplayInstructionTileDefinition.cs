using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Leaderboards/DisplayLeaderboard", fileName = "DisplayLeaderboard")]
    public class LeaderboardDisplayInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new LeaderboardDisplayInstructionTile();
        [SerializeField] protected string _id = LeaderboardDisplayInstructionTile.ID;
        [SerializeField] protected string _name = LeaderboardDisplayInstructionTile.NAME;
        [SerializeField] protected string _category = LeaderboardDisplayInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
