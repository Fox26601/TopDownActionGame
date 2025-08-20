using Microsoft.Xna.Framework;

namespace IsometricActionGame.Core.Data
{
    /// <summary>
    /// Centralized constants to eliminate hardcoded values throughout the project
    /// </summary>
    public static class GameConstants
    {
        // Animation Constants
        public static class Animation
        {
            public const float IDLE_FRAME_TIME = 0.2f;
            public const float RUN_FRAME_TIME = 0.1f;
            public const float ATTACK_FRAME_TIME = 0.1f;
            public const float DEATH_FRAME_TIME = 0.2f;
            public const float HIT_FRAME_TIME = 0.1f;
            public const float FIREBALL_FRAME_TIME = 0.1f;
            public const float MAYOR_FRAME_TIME = 0.3f;
            public const float PEBBLE_FRAME_TIME = 0.15f;
        }

        // Scale Constants - Renamed to SpriteScale
        public static class SpriteScale
        {
            public const float PLAYER_SCALE = 3.0f;
            public const float BAT_SCALE = 3.0f;
            public const float PEBBLE_SCALE = 3.0f;
            public const float NPC_SCALE = 1.0f;
            public const float ITEM_SCALE = 0.5f; // Increased from 0.05f for better visibility
            public const float HEALTH_POTION_SCALE = 0.05f; // Decreased from 0.1f for smaller health potions on ground
            public const float HEALTH_POTION_INVENTORY_SCALE = 0.05f; // Decreased to match ground scale
            public const float KEY_INVENTORY_SCALE = 2.0f; // Increased to match ground scale
            public const float DOOR_SCALE = 1.0f;
            public const float CHEST_SCALE = 1.0f;
            public const float KEY_SCALE = 3.0f; // Increased to match inventory scale size
            public const float FIREBALL_SCALE = 1.0f; // Larger for testing visibility
        }

        // Layer Depth Constants
        public static class LayerDepth
        {
            public const float NORMALIZATION_FACTOR = 20.0f;
            public const float DEFAULT_DEPTH = 0.5f;
            public const float UI_DEPTH = 0.9f;
            public const float DEBUG_DEPTH = 0.95f;
        }

        // Movement Constants
        public static class Movement
        {
            public const float PLAYER_SPEED = 3.0f;
            public const float BAT_SPEED = 2.0f;
            public const float BAT_CHASE_SPEED = 3.0f;
            public const float FIREBALL_SPEED = 5.0f;
        }

        // Health Constants
        public static class Health
        {
            public const int PLAYER_MAX_HEALTH = 200;
            public const int BAT_MAX_HEALTH = 50;
            public const int PEBBLE_MAX_HEALTH = 100;
            public const int HEALTH_POTION_DEFAULT_HEAL = 50;
            public const int HEALTH_THRESHOLD_HIGH = 50;
            public const int HEALTH_THRESHOLD_MEDIUM = 25;
        }

        // Damage Constants
        public static class Damage
        {
            public const int PLAYER_ATTACK_DAMAGE = 50;
            public const int BAT_ATTACK_DAMAGE = 20;
            public const int FIREBALL_DAMAGE = 25;
            public const int PEBBLE_DAMAGE = 15;
        }

        // Attack Constants
        public static class Attack
        {
            public const float PLAYER_ATTACK_RANGE = 1.0f; // Increased from 0.5f for better gameplay feel
            public const float PLAYER_ATTACK_CONE_ANGLE = 160f; // 160-degree attack cone
            public const float BAT_ATTACK_RANGE = 0.8f; // Increased from 0.5f for better bat attacks
            public const float BAT_CHASE_RANGE = 5.0f;
            public const float PEBBLE_ATTACK_RANGE = 8.0f; // Reduced from unlimited to reasonable range
        }

        // Timing Constants
        public static class Timing
        {
            public const float HIT_ANIMATION_DURATION = 0.5f;
            public const float PEBBLE_SHOOT_COOLDOWN = 3.0f;
            public const float BAT_ATTACK_COOLDOWN = 5.0f;
            public const float CONSOLE_FULL_OPACITY_DURATION = 15.0f;
            public const float CONSOLE_FADE_DURATION = 5.0f;
            public const float DIALOGUE_TEXT_SPEED = 0.02f; // Faster text speed for better responsiveness
            public const float CAMERA_LERP_FACTOR = 0.1f;
        }

        // Spawn Constants
        public static class Spawn
        {
            public const int MAX_BATS_WITH_QUEST = 2; // Maximum bats when quest is active
            public const int MIN_BATS_ALWAYS = 1; // Minimum bats when quest is not active
            public const float BAT_SPAWN_COOLDOWN = 20f; // 20 seconds cooldown between bat spawns
        }

        // Interaction Constants
        public static class Interaction
        {
            public const float NPC_INTERACTION_RADIUS = 1.0f; // Absolute interaction radius for NPCs
            public const float DOOR_INTERACTION_RADIUS = 1.0f; // Absolute interaction radius for Doors
            public const float CHEST_INTERACTION_RADIUS = 0.5f; // Interaction radius for chests
            public const float ITEM_DEFAULT_INTERACTION_RADIUS = 0.5f; // Adjusted for new calculation logic
            public const float KEY_PICKUP_RADIUS = 3.0f; // Adjusted for new calculation logic
        }

        // Hitbox Constants - Renamed to Combat for better grouping
        public static class Combat
        {
            public const float PLAYER_HITBOX_RADIUS = 0.3f; // Adjusted value
            public const float BAT_HITBOX_RADIUS = 0.3f; // Adjusted value
            public const float PEBBLE_HITBOX_RADIUS = 0.3f;
            public const float DEFAULT_NPC_HITBOX_RADIUS = 0.3f; // New
            public const float DOOR_HITBOX_RADIUS = 0.5f; // Door hitbox radius
            public const float FIREBALL_HITBOX_RADIUS = 0.2f; // Keep this as it was (already reasonable)
            public const float PLAYER_HITBOX_Y_OFFSET = 1f; // Existing
        }

        // UI Constants
        public static class UI
        {
            // Base UI dimensions (will be scaled by UIScalingManager)
            public const int HEALTH_BAR_WIDTH = 150;
            public const int HEALTH_BAR_HEIGHT = 20;
            public const int HEALTH_BAR_X = 10;
            public const int HEALTH_BAR_Y = 10;
            
            // Console dimensions and positions
            public const int CONSOLE_DEFAULT_X = 980;
            public const int CONSOLE_DEFAULT_Y = 10;
            public const int CONSOLE_DEFAULT_WIDTH = 1280;
            public const int CONSOLE_DEFAULT_HEIGHT = 720;

            public const float OVERLAY_OPACITY = 0.8f;
            public const float PANEL_OPACITY = 0.9f;
            public const float PAUSE_OVERLAY_OPACITY = 0.6f;
            
            // Gold display constants
            public const int GOLD_DISPLAY_MARGIN_X = 10;
            public const int GOLD_DISPLAY_MARGIN_Y = 10;
            
            // Button dimensions
            public const int BUTTON_WIDTH = 220;
            public const int BUTTON_HEIGHT = 60;
            public const int BUTTON_SPACING = 20;
            
            // Panel dimensions
            public const int PANEL_WIDTH = 400;
            public const int PANEL_HEIGHT = 300;
            public const int DIALOGUE_PANEL_WIDTH = 1100;
            public const int DIALOGUE_PANEL_HEIGHT = 120;
            
            // Margins and spacing
            public const int MARGIN_SMALL = 10;
            public const int MARGIN_MEDIUM = 20;
            public const int MARGIN_LARGE = 30;
            
            // Font sizes
            public const float FONT_SIZE_SMALL = 12f;
            public const float FONT_SIZE_MEDIUM = 16f;
            public const float FONT_SIZE_LARGE = 24f;
            public const float FONT_SIZE_TITLE = 32f;
        }

        // World Constants
        public static class World
        {
            public static readonly Vector2 PLAYER_START_POSITION = new Vector2(1, 1);
            public static readonly Vector2 BAT_START_POSITION = new Vector2(1, 19);
            public static readonly Vector2 BAT_INITIAL_DIRECTION = new Vector2(-1, -1);
            public static readonly Vector2 PEBBLE_START_POSITION = new Vector2(18, 2);
            public static readonly Vector2 MAYOR_POSITION = new Vector2(10, 10);
            public static readonly Vector2 VENDOR_POSITION = new Vector2(5, 5);
            public static readonly Vector2 DOOR_POSITION = new Vector2(2, 1);
            public static readonly Vector2 KEY_DROP_OFFSET = new Vector2(-1, 0); // Spawn key to the left of chest

        
        }

        // Direction Constants
        public static class Directions
        {
            public static readonly Vector2 UP = new Vector2(0, -1);
            public static readonly Vector2 DOWN = new Vector2(0, 1);
            public static readonly Vector2 LEFT = new Vector2(-1, 0);
            public static readonly Vector2 RIGHT = new Vector2(1, 0);
            public static readonly Vector2 ZERO = new Vector2(0, 0);
        }

        // Sprite Frame Constants
        public static class SpriteFrames
        {
            public const int PLAYER_COLUMNS = 6;
            public const int PLAYER_ROWS = 10;
            public const int PLAYER_IDLE_FRAMES = 6;
            public const int PLAYER_RUN_FRAMES = 6;
            public const int PLAYER_ATTACK_FRAMES = 4;
            public const int PLAYER_DEATH_FRAMES = 4;
            
            public const int BAT_COLUMNS = 4;
            public const int BAT_FLY_FRAMES = 4;
            public const int BAT_ATTACK_FRAMES = 7;
            public const int BAT_HIT_FRAMES = 5;
            public const int BAT_DEATH_FRAMES = 11;
            
            public const int PEBBLE_COLUMNS = 4;
            public const int PEBBLE_IDLE_FRAMES = 4;
            public const int PEBBLE_HIT_FRAMES = 5;
            public const int PEBBLE_DEATH_FRAMES = 7;
            
            public const int BONES_COLUMNS = 4;
            public const int BONES_FLY_FRAMES = 8;
            public const int BONES_HIT_FRAMES = 4;
            
            public const int NPC_IDLE_FRAMES = 6;
        }

        // New Chest Sprite Frame Constants
        public static class ChestSpriteFrames
        {
            public const int CHEST_COLUMNS = 5;
            public const int CHEST_ROWS = 8;
            public const int CHEST_CLOSED_ROW = 0; // Row 1 (0-indexed)
            public const int CHEST_CLOSED_FRAMES = 5; // All 5 frames of row 1
            public const int CHEST_OPENING_ROW = 1; // Row 2 (0-indexed)
            public const int CHEST_OPENING_FRAMES = 5; // All 5 frames of row 2
            public const int CHEST_OPEN_ROW = 1; // Row 2 (0-indexed) - same row as opening animation
            public const int CHEST_OPEN_FRAMES = 3; // Last 3 frames of row 2 (frames 2,3,4)
            public const int CHEST_OPEN_START_FRAME = 2; // Start from frame 2 (third frame) of opening row
        }

        // New Door Sprite Frame Constants
        public static class DoorSpriteFrames
        {
            public const int DOOR_COLUMNS = 1;
            public const int DOOR_ROWS = 10;
            public const int DOOR_CLOSED_FRAME = 0; // Frame 1 (0-indexed) - fully closed
            public const int DOOR_OPEN_FRAME = 4;   // Frame 5 (0-indexed) - fully open
            public const int DOOR_OPENING_START_FRAME = 0; // Start from frame 1
            public const int DOOR_OPENING_END_FRAME = 4;   // End at frame 5
            public const int DOOR_OPENING_FRAMES = 5; // Total frames for opening animation
        }

        // Graphics Constants
        public static class Graphics
        {
            public const float BACKGROUND_SCALE = 1.0f;
            public const float BACKGROUND_LAYER_DEPTH = 0.0f;
            public const float ITEM_DEFAULT_LAYER_DEPTH = 0.5f;
            public const float DOOR_LAYER_DEPTH = 0.6f; // New
            public const float ENEMY_LAYER_DEPTH = 0.55f; // New
            public const float OBJECT_LAYER_DEPTH = 0.45f; // New
            public const float NPC_LAYER_DEPTH = 0.52f; // New
            public const float DEFAULT_SPRITE_SCALE = 1.0f;
            public const float MAX_SPRITE_SCALE = 2.0f; // New - for UI scaling
        }

        // New Tile Constants
        public static class Tile
        {
            public const int TILE_SIZE = 64; // Default tile size for isometric grid
        }

        // Probability Constants
        public static class Probability
        {
            public const double GOLD_DROP_CHANCE = 0.5;
            public const double HEALTH_POTION_DROP_CHANCE = 0.3;
        }

        // Item Constants
        public static class Items
        {
            public const int GOLD_MAX_STACK_SIZE = 999;
            public const int DEFAULT_ITEM_QUANTITY = 1;
            public const int DEBUG_TEXTURE_SIZE = 32;
            public const int DEBUG_RECTANGLE_SIZE = 160;
            public const int DEBUG_SMALL_RECTANGLE_SIZE = 32;
            public const int DEBUG_RECTANGLE_OFFSET = 80;
            public const int DEBUG_SMALL_RECTANGLE_OFFSET = 16;
        }

        // Projectile Constants
        public static class Projectiles
        {
            public const int MAX_POOL_SIZE = 50;
        }

        // UI Layout Constants
        public static class UILayout
        {
            public const int SETTINGS_PANEL_WIDTH = 500;
            public const int SETTINGS_PANEL_ELEMENT_OFFSET = 50;
            public const int SETTINGS_BUTTON_Y_OFFSET = 150;
            public const int CONFIRMATION_DIALOG_WIDTH = 500;
            public const int CONFIRMATION_DIALOG_HEIGHT = 250;
            public const int DIALOGUE_POSITION_X = 100;
            public const int DIALOGUE_POSITION_Y = 500;
            
            // Dialogue positioning
            public const int DIALOGUE_CONTINUE_Y_OFFSET = 80;
            public const int DIALOGUE_ESC_Y_OFFSET = 100;
            public const int DIALOGUE_CHOICES_Y_OFFSET = 140;
            public const int DIALOGUE_CHOICE_SPACING = 30;
            public const int DIALOGUE_INSTRUCTION_OFFSET = 20;
            
            // Inventory positioning
            public const int INVENTORY_GOLD_Y_OFFSET = 10;
            public const int INVENTORY_HOTKEY_OFFSET = 2;
            
            // Message display
            public const int MESSAGE_DISPLAY_X = 400;
            public const int MESSAGE_DISPLAY_Y = 100;
            public const int MESSAGE_PADDING_X = 20;
            public const int MESSAGE_PADDING_Y = 10;
            
            // Enhanced Chat System Constants
            public const int CHAT_MAX_MESSAGES = 10;
            public const int CHAT_MESSAGE_SPACING = 2;
            public const int CHAT_PADDING = 8;
            public const int CHAT_WIDTH = 350;
            public const int CHAT_HEIGHT = 300;
            
            // UI positioning vectors
            public static readonly Vector2 VERTICAL_OFFSET = new Vector2(0, 1);
            public static readonly Vector2 HORIZONTAL_OFFSET = new Vector2(1, 0);
            public static readonly Vector2 ZERO_OFFSET = new Vector2(0, 0);
        }

        // Color Constants
        public static class Colors
        {
            public static readonly Color HEALTH_HIGH = Color.Green;
            public static readonly Color HEALTH_MEDIUM = Color.Yellow;
            public static readonly Color HEALTH_LOW = Color.Red;
            public static readonly Color CONSOLE_GOLD = Color.Yellow;
            public static readonly Color CONSOLE_LIME = Color.Lime;
            public static readonly Color CONSOLE_WHITE = Color.White;
            public static readonly Color CONSOLE_BLACK = Color.Black;
            public static readonly Color CONSOLE_RED = Color.Red;
            public static readonly Color CONSOLE_GRAY = Color.Gray;
            public static readonly Color CONSOLE_ORANGE = Color.Orange;
            public static readonly Color CONSOLE_GREEN = Color.Green;
            public static readonly Color CONSOLE_YELLOW = Color.Yellow;
            public static readonly Color CONSOLE_CYAN = Color.Cyan;

            // Tile Colors
            public static readonly Color TILE_GRASS = Color.ForestGreen;
            public static readonly Color TILE_WALL = Color.DarkSlateGray;
            public static readonly Color TILE_WATER = Color.CornflowerBlue;
            public static readonly Color TILE_DOOR = Color.Brown;
            public static readonly Color TILE_CHEST = Color.Gold;
            public static readonly Color TILE_ENEMY_SPAWN = Color.Red;
            public static readonly Color TILE_KEY_SPAWN = Color.Yellow;
            public static readonly Color TILE_DARK_GRAY = Color.DarkGray; // For health bar background
        }

        // Event System Constants
        public static class Events
        {
            public const int MAX_QUEUED_EVENTS = 1000;
            public const int DEFAULT_HANDLER_PRIORITY = 0;
            public const int CRITICAL_HANDLER_PRIORITY = 100;
            public const int UI_HANDLER_PRIORITY = 50;
            public const int AUDIO_HANDLER_PRIORITY = 25;
            public const int GAMEPLAY_HANDLER_PRIORITY = 75;
            
            // Event processing timing
            public const float EVENT_PROCESSING_TIME_BUDGET_MS = 16.0f; // ~1 frame at 60fps
        }

        // Settings Constants
        public static class Settings
        {
            public const float DEFAULT_MASTER_VOLUME = 1.0f;
            public const float DEFAULT_MUSIC_VOLUME = 0.8f;
            public const float DEFAULT_SFX_VOLUME = 0.9f;
            public const float MIN_VOLUME = 0.0f;
            public const float MAX_VOLUME = 1.0f;
            public const int DEFAULT_RESOLUTION_WIDTH = 1280;
            public const int DEFAULT_RESOLUTION_HEIGHT = 720;
            public const string GAME_VERSION = "1.0.0";
        }
    }
}