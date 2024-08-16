using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Tiles/Leaderboards/HideLeaderboard", fileName = "HideLeaderboard")]
    public class LeaderboardHideInstructionTileDefinition : ScriptableObject, IInstructionTileDefinition
    {
        [SerializeReference] protected IInstructionTile _tile = new LeaderboardHideInstructionTile();
        [SerializeField] protected string _id = LeaderboardHideInstructionTile.ID;
        [SerializeField] protected string _name = LeaderboardHideInstructionTile.NAME;
        [SerializeField] protected string _category = LeaderboardHideInstructionTile.CATEGORY;

        public IInstructionTile Tile { get => _tile; }
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Category { get => _category; set => _category = value; }
    } 
}
