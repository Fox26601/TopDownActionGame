namespace IsometricActionGame.Events
{
    // Centralized event name constants
    // Provides type safety and prevents string typos
    public static class GameEvents
    {
        // ===== PLAYER EVENTS =====
        public const string PLAYER_ATTACK = "player.attack";
        public const string PLAYER_DAMAGED = "player.damaged";
        public const string PLAYER_HEALED = "player.healed";
        public const string PLAYER_DIED = "player.died";
        public const string PLAYER_LEVEL_UP = "player.levelup";
        public const string PLAYER_MOVED = "player.moved";
        public const string PLAYER_INTERACTION_REQUESTED = "player.interaction_requested";
        public const string PLAYER_ATTACK_REQUESTED = "player.attack_requested";
        public const string PLAYER_MOVEMENT_DIRECTION = "player.movement_direction";
        
        // ===== COMBAT EVENTS =====
        public const string ENEMY_DEFEATED = "combat.enemy_defeated";
        public const string ATTACK_EXECUTED = "combat.attack_executed";
        public const string DAMAGE_DEALT = "combat.damage_dealt";
        public const string PROJECTILE_CREATED = "combat.projectile_created";
        public const string PROJECTILE_DESTROYED = "combat.projectile_destroyed";
        
        // ===== INVENTORY EVENTS =====
        public const string ITEM_PICKED_UP = "inventory.item_picked_up";
        public const string ITEM_DISCARDED = "inventory.item_discarded";
        public const string ITEM_DROPPED = "inventory.item_dropped";
        public const string ITEM_USED = "inventory.item_used";
        public const string GOLD_CHANGED = "inventory.gold_changed";
        public const string INVENTORY_CHANGED = "inventory.changed";
        public const string INVENTORY_OPENED = "inventory.opened";
        public const string INVENTORY_CLOSED = "inventory.closed";
        public const string QUICK_ACCESS_USED = "inventory.quick_access_used";
        
        // ===== QUEST EVENTS =====
        public const string QUEST_STARTED = "quest.started";
        public const string QUEST_COMPLETED = "quest.completed";
        public const string QUEST_TURNED_IN = "quest.turned_in";
        public const string QUEST_REFUSED = "quest.refused";
        public const string QUEST_PROGRESS_UPDATED = "quest.progress_updated";
        
        // ===== DIALOGUE EVENTS =====
        public const string DIALOGUE_STARTED = "dialogue.started";
        public const string DIALOGUE_ENDED = "dialogue.ended";
        public const string DIALOGUE_CHOICE_SELECTED = "dialogue.choice_selected";
        
        // ===== UI EVENTS =====
        public const string MENU_OPENED = "ui.menu_opened";
        public const string MENU_CLOSED = "ui.menu_closed";
        public const string BUTTON_CLICKED = "ui.button_clicked";
        public const string RESOLUTION_CHANGED = "ui.resolution_changed";
        public const string FULLSCREEN_CHANGED = "ui.fullscreen_changed";
        public const string SETTINGS_OPENED = "ui.settings_opened";
        public const string SETTINGS_RESET = "ui.settings_reset";
        public const string ESC_PRESSED = "ui.esc_pressed";
        public const string INVENTORY_TOGGLE_REQUESTED = "ui.inventory_toggle_requested";
        public const string PAUSE_MENU_OPEN_REQUESTED = "ui.pause_menu_open_requested";
        public const string PAUSE_MENU_CLOSE_REQUESTED = "ui.pause_menu_close_requested";
        public const string PAUSE_MENU_CONFIRMATION_CLOSE_REQUESTED = "ui.pause_menu_confirmation_close_requested";
        public const string SAVE_SLOT_MENU_CLOSE_REQUESTED = "ui.save_slot_menu_close_requested";
        
        // ===== GAME STATE EVENTS =====
        public const string GAME_STARTED = "game.started";
        public const string GAME_PAUSED = "game.paused";
        public const string GAME_RESUMED = "game.resumed";
        public const string GAME_STOPPED = "game.stopped";
        public const string GAME_RESTART = "game.restart";
        public const string GAME_EXIT = "game.exit";
        public const string RETURN_TO_MENU = "game.return_to_menu";
        public const string GO_TO_MENU = "game.go_to_menu";
        public const string GAME_COMPLETED = "game.completed";
        public const string GAME_STATE_CHANGED = "game.state_changed";
        public const string SCENE_CLEAR_REQUESTED = "game.scene_clear_requested";
        
        // ===== RESTART SYSTEM EVENTS =====
        public const string RESTART_INITIATED = "restart.initiated";
        public const string RESTART_ENTITIES_CLEARED = "restart.entities_cleared";
        public const string RESTART_PLAYER_RESET = "restart.player_reset";
        public const string RESTART_ENEMIES_RESET = "restart.enemies_reset";
        public const string RESTART_ITEMS_RESET = "restart.items_reset";
        public const string RESTART_UI_RESET = "restart.ui_reset";
        public const string RESTART_SYSTEMS_RESET = "restart.systems_reset";
        public const string RESTART_COMPLETED = "restart.completed";
        public const string RESTART_FAILED = "restart.failed";
        public const string PLAYER_RECREATED = "restart.player_recreated";
        public const string PEBBLE_RECREATED = "restart.pebble_recreated";
        public const string BAT_RECREATED = "restart.bat_recreated";
        
        // ===== RESET SYSTEM EVENTS =====
        public const string RESET_REQUESTED = "reset.requested";
        public const string RESET_COMPLETED = "reset.completed";
        public const string RESET_FAILED = "reset.failed";
        
        // ===== LEVEL SYSTEM EVENTS =====
        public const string LEVEL_STARTED = "level.started";
        public const string LEVEL_COMPLETED = "level.completed";
        public const string LEVEL_LOADING = "level.loading";
        public const string LEVEL_LOADED = "level.loaded";
        public const string LEVEL_FAILED = "level.failed";
        public const string LEVEL_PROGRESS_SAVED = "level.progress_saved";
        public const string LEVEL_PROGRESS_LOADED = "level.progress_loaded";
        public const string NEXT_LEVEL_TRIGGERED = "level.next_triggered";
        public const string LEVEL_TRANSITION_PANEL_SHOWN = "level.transition_panel_shown";
        public const string LEVEL_TRANSITION_PANEL_HIDDEN = "level.transition_panel_hidden";
        public const string LEVEL_TRANSITION_PANEL_CREATED = "level.transition_panel_created";
        
        // ===== ENVIRONMENT EVENTS =====
        public const string DOOR_OPENED = "environment.door_opened";
        public const string CHEST_OPENED = "environment.chest_opened";
        public const string CHEST_CREATED = "environment.chest_created";
        public const string KEY_PICKED_UP = "environment.key_picked_up";
        public const string AREA_ENTERED = "environment.area_entered";
        public const string AREA_EXITED = "environment.area_exited";
        
        // ===== AUDIO EVENTS =====
        public const string SOUND_EFFECT_PLAY = "audio.sound_effect_play";
        public const string MUSIC_PLAY = "audio.music_play";
        public const string MUSIC_STOP = "audio.music_stop";
        public const string VOLUME_CHANGED = "audio.volume_changed";
        
        // ===== NPC EVENTS =====
        public const string NPC_INTERACTION = "npc.interaction";
        public const string NPC_DIALOGUE_STARTED = "npc.dialogue_started";
        public const string NPC_QUEST_OFFERED = "npc.quest_offered";
        
        // ===== SYSTEM EVENTS =====
        public const string SAVE_GAME = "system.save_game";
        public const string LOAD_GAME = "system.load_game";
        public const string LOAD_GAME_REQUESTED = "system.load_game_requested";
        public const string QUICK_SAVE_REQUESTED = "system.quick_save_requested";
        public const string QUICK_LOAD_REQUESTED = "system.quick_load_requested";
        public const string ERROR_OCCURRED = "system.error_occurred";
        public const string PERFORMANCE_WARNING = "system.performance_warning";
        public const string CONSOLE_MESSAGE = "system.console_message";
        public const string GAME_LOADED = "system.game_loaded";
    }
}

