namespace Mona.SDK.Brains.Core
{
    public class MonaBrainConstants
    {
        public const string SPEED_FACTOR = "__SpeedFactor";

        public const string RESULT_SENDER = "__Sender";
        public const string RESULT_TARGET = "__Target";
        public const string RESULT_LAST_SPAWNED = "__LastSpawned";
        public const string RESULT_LAST_SKIN = "__LastSkin";
        public const string RESULT_INDEX = "Index";

        public const string RESULT_MOVE_DIRECTION = "__MoveDirection";
        public const string RESULT_MOUSE_DIRECTION = "__MouseDirection";
        public const string RESULT_HIT_TARGET = "__HitTarget";
        public const string RESULT_HIT_POINT = "__HitPoint";
        public const string RESULT_HIT_NORMAL = "__HitNormal";
        public const string RESULT_STATE = "__State";
        public const string RESULT_DIRECTION_TO_TARGET = "__DirectionToTarget";

        public const string LAST_MOVE_DIRECTION = "__LastMoveDirection";

        public const string ON_STARTING = "__OnStarting";

        public const string TRIGGER = "__TriggerAnimation";
        public const string TRIGGER_1 = "__TriggerAnimation1";
        public const string ANIMATION_SPEED = "__AnimationSpeed";

        public const string BRAIN_SPAWNED_EVENT = "OnMonaBrainSpawnedEvent";
        public const string BRAIN_DESTROYED_EVENT = "OnMonaBrainDestroyedEvent";
        public const string BRAIN_RELOAD_EVENT = "OnMonaBrainReloadEvent";
        public const string BRAIN_ADD_UI = "OnMonaBrainAddUIEvent";
        public const string BRAIN_REMOVE_UI = "OnBrainRemoveUIEvent";

        public const string CORE_PAGE_TICK_EVENT = "OnMonaCorePageTickEvent";
        public const string STATE_PAGE_TICK_EVENT = "OnMonaStatePageTickEvent";

        public const string MONA_BRAINS_DO_EVENT = "OnMonaBrainsDoEvent";
        public const string MONA_BRAINS_EVENT = "OnMonaBrainsEvent";
        public const string MONA_BRAINS_THEN_EVENT = "OnMonaBrainsThenEvent";

        public const string WALLET_CONNECTED_EVENT = "OnMonaWalletConnected";
        public const string WALLET_DISCONNECTED_EVENT = "OnMonaWalletDisconnected";
        public const string WALLET_TOKEN_SELECTED_EVENT = "OnMonaWalletTokenSelected";

        public const string STATE_CHANGED_EVENT = "OnMonaStateChangedEvent";
        public const string STATE_AUTHORITY_CHANGED_EVENT = "OnMonaStateAuthorityChangedEvent";
        public const string BODY_ANIMATION_CONTROLLER_CHANGE_EVENT = "OnMonaBodyAnimationControllerChangeEvent";
        public const string BODY_ANIMATION_CONTROLLER_CHANGED_EVENT = "OnMonaBodyAnimationControllerChangedEvent";

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
        public const string TAG_TEAM_A = "TeamA";
        public const string TAG_TEAM_B = "TeamB";
        public const string TAG_TEAM_C = "TeamC";
        public const string TAG_TEAM_D = "TeamD";
        public const string TAG_GAMECONTROLLER = "GameController";
        public const string TAG_CAMERA = "Camera";
        public const string TAG_FRIENDLY = "Friendly";
        public const string TAG_NEUTRAL = "Neutral";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_MINION = "Minion";
        public const string TAG_BOSS = "Boss";
        public const string TAG_CARNIVORE = "Carnivore";
        public const string TAG_HERBIVORE = "Herbivore";
        public const string TAG_OMNIVORE = "Omnivore";
        public const string TAG_VEGITATION = "Vegitation";
        public const string TAG_CORPSE = "Corpse";
        public const string TAG_VEHICLE = "Vehicle";

        public const string TAG_WEAPON = "Weapon";
        public const string TAG_DOOR = "Door";
        public const string TAG_WINDOW = "Window";
        public const string TAG_LOCK = "Lock";
        public const string TAG_STAIRS = "Stairs";
        public const string TAG_TABLE = "Table";
        public const string TAG_CHAIR = "Chair";
        public const string TAG_PICTURE = "Picture";

        public const string TAG_SPAWNPOINT = "SpawnPoint";
        public const string TAG_SPAWNER = "Spawner";
        public const string TAG_SPAWNED_OBJECT = "SpawnedObject";
        public const string TAG_COLLECTIBLE = "Collectible";
        public const string TAG_DESTRUCTIBLE = "Destructible";
        public const string TAG_BUTTON = "Button";
        public const string TAG_SWITCH = "Switch";
        public const string TAG_KEYITEM = "KeyItem";
        public const string TAG_COIN = "Coin";
        public const string TAG_KEY = "Key";
        public const string TAG_HEART = "Heart";
        public const string TAG_POWERUP = "PowerUp";
        public const string TAG_POWERDOWN = "PowerDown";
        public const string TAG_TREASURE = "Treasure";
        public const string TAG_INVENTORYITEM = "InventoryItem";
        public const string TAG_FOOD = "Food";
        public const string TAG_BALL = "Ball";
        public const string TAG_FLAG = "Flag";

        public const string TAG_TRIGGER_VOLUME = "TriggerVolume";
        public const string TAG_GROUND = "Ground";
        public const string TAG_WATER = "Water";
        public const string TAG_HAZARD = "Hazard";
        public const string TAG_BARRIER = "Barrier";
        public const string TAG_KILLZONE = "Killzone";
        public const string TAG_LAVA = "Lava";
        public const string TAG_FIRE = "Fire";
        public const string TAG_ICE = "Ice";
        public const string TAG_WIND = "Wind";
        public const string TAG_POISON = "Poison";
        public const string TAG_SPIKES = "Spikes";
        public const string TAG_TRAP = "Trap";
        public const string TAG_DEATHPLANE = "DeathPlane";

        public const string TAG_START = "Start";
        public const string TAG_GOAL = "Goal";
        public const string TAG_CHECKPOINT = "Checkpoint";
        public const string TAG_WAYPOINT = "Waypoint";

        public const string TAG_EDITABLE = "Editable";
        public const string TAG_LAYOUT = "Layout";
        public const string TAG_SCENE_SPACE = "SceneSpace";

        public const string TAG_PAUSABLE = "Pausable";
        public const string TAG_UNPAUSABLE = "Unpausable";

        public static readonly int AVATAR_MAXIMUM_FILESIZE_MB = 16;

        public const string SCENE_SPACE = "Space";
    }
}