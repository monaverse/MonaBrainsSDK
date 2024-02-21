namespace Mona.SDK.Brains.Core
{
    public class MonaBrainConstants
    {
        public const string SPEED_FACTOR = "__SpeedFactor";

        public const string RESULT_SENDER = "__Sender";
        public const string RESULT_TARGET = "__Target";

        public const string RESULT_MOVE_DIRECTION = "__MoveDirection";
        public const string RESULT_MOUSE_DIRECTION = "__MouseDirection";
        public const string RESULT_HIT_TARGET = "__HitTarget";
        public const string RESULT_HIT_POINT = "__HitPoint";
        public const string RESULT_HIT_NORMAL = "__HitNormal";
        public const string RESULT_STATE = "__State";

        public const string ON_STARTING = "__OnStarting";

        public const string TRIGGER = "__TriggerAnimation";
        public const string TRIGGER_1 = "__TriggerAnimation1";
        public const string ANIMATION_SPEED = "__AnimationSpeed";

        public const string BRAIN_SPAWNED_EVENT = "OnMonaBrainSpawnedEvent";
        public const string BRAIN_DESTROYED_EVENT = "OnMonaBrainDestroyedEvent";
        public const string BRAIN_RELOAD_EVENT = "OnMonaBrainReloadEvent";

        public const string CORE_PAGE_TICK_EVENT = "OnMonaCorePageTickEvent";
        public const string STATE_PAGE_TICK_EVENT = "OnMonaStatePageTickEvent";

        public const string MONA_BRAINS_DO_EVENT = "OnMonaBrainsDoEvent";
        public const string MONA_BRAINS_THEN_EVENT = "OnMonaBrainsThenEvent";

        public const string STATE_CHANGED_EVENT = "OnMonaStateChangedEvent";
        public const string STATE_AUTHORITY_CHANGED_EVENT = "OnMonaStateAuthorityChangedEvent";

        public const string TRIGGER_EVENT = "OnMonaTriggerEvent";
        public const string BROADCAST_MESSAGE_EVENT = "OnMonaBroadcastMessageEvent";
        public const string PLAYER_INPUT_EVENT = "OnPlayerInputEvent";
        public const string BRAIN_TICK_EVENT = "OnBrainTickEvent";

        public const string ERROR_MISSING_PLAYER = "Cannot find Player";
        public const string ERROR_MISSING_ORIGIN = "Cannot find Origin";
        public const string ERROR_MISSING_PART = "Cannot find Part";
        public const string ERROR_MISSING_TARGET = "Cannot find Target";
        public const string NOTHING_IN_SIGHT = "Nothing in Sight";
        public const string NOTHING_CLOSE_BY = "Nothing Close By";
        public const string NO_HIT = "No Hit";
        public const string NO_INPUT = "No Input";
        public const string NO_MESSAGE = "No Message";
        public const string INVALID_VALUE = "Value Invalid";
        public const string NO_VALUE = "No Value by that Name";
        public const string NOT_STARTED = "Not Started";

        public const string TILE_MENU_LABEL = "...";
        public const string MENU_DELETE_TILE = "Delete Tile";
        public const string MENU_MOVE_RIGHT = "Move Tile Right";
        public const string MENU_MOVE_LEFT = "Move Tile Left";
        public const string MENU_SHOW = "Show More Properties";
        public const string MENU_HIDE = "Hide More Properties";

        public const string TAG_DEFAULT = "Default";
        public const string TAG_PLAYER = "Player";
        public const string TAG_REMOTE_PLAYER = "RemotePlayer";
        public const string TAG_CAMERA = "Camera";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_FRIENDLY = "Friendly";
        public const string TAG_COLLECTIBLE = "Collectible";

        public static readonly int AVATAR_MAXIMUM_FILESIZE_MB = 16;
    }
}