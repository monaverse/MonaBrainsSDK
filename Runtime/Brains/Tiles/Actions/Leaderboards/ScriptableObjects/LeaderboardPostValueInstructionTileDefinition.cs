using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Leaderboards/PostToLeaderboard", fileName = "PostToLeaderboard")]
    public class LeaderboardPostValueInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new LeaderboardPostValueInstructionTile();
        [SerializeField] protected string _id = LeaderboardPostValueInstructionTile.ID;
        [SerializeField] protected string _name = LeaderboardPostValueInstructionTile.NAME;
        [SerializeField] protected string _category = LeaderboardPostValueInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
