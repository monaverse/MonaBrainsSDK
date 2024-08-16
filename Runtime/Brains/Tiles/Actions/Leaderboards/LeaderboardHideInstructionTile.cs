using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.EasyUI.Leaderboards;

namespace Mona.SDK.Brains.Tiles.Actions.Leaderboards
{
    [Serializable]
    public class LeaderboardHideInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "HideLeaderboard";
        public const string NAME = "Hide Leaderboard";
        public const string CATEGORY = "Leaderboards";
        public override Type TileType => typeof(LeaderboardHideInstructionTile);

        private IMonaBrain _brain;
        private LeaderboardDisplayController _leaderboardDisplay;

        public LeaderboardHideInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _leaderboardDisplay = LeaderboardDisplayController.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _leaderboardDisplay == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (_leaderboardDisplay.Window != null)
                _leaderboardDisplay.Window.CloseWindow();

            return Complete(InstructionTileResult.Success, true);
        }
    }
}