using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;

namespace IsometricActionGame.Settings
{
    public class InputSettings
    {
        private const string SETTINGS_FILE = "Settings/input_settings.json";
        
        // Movement keys
        public Keys MoveUp { get; set; } = Keys.W;
        public Keys MoveDown { get; set; } = Keys.S;
        public Keys MoveLeft { get; set; } = Keys.A;
        public Keys MoveRight { get; set; } = Keys.D;
        
        // Alternative movement keys
        public Keys MoveUpAlt { get; set; } = Keys.Up;
        public Keys MoveDownAlt { get; set; } = Keys.Down;
        public Keys MoveLeftAlt { get; set; } = Keys.Left;
        public Keys MoveRightAlt { get; set; } = Keys.Right;
        
        // Action keys
        public Keys InventoryToggle { get; set; } = Keys.Q;
        public Keys Interaction { get; set; } = Keys.E;
        public Keys Attack { get; set; } = Keys.Space;
        
        // System keys
        public Keys Pause { get; set; } = Keys.Escape;
        public Keys PauseMenu { get; set; } = Keys.Escape;
        public Keys QuickSave { get; set; } = Keys.F5;
        public Keys QuickLoad { get; set; } = Keys.F9;
        public Keys Exit { get; set; } = Keys.F10;
        public Keys ExitGame { get; set; } = Keys.F10;
        
        // UI keys
        public Keys Confirm { get; set; } = Keys.Enter;
        public Keys Cancel { get; set; } = Keys.Escape;
        
        // Quick access slots
        public Keys[] QuickAccessSlots { get; set; } = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6 };
        
        // Singleton instance
        private static InputSettings _instance;
        public static InputSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadSettings();
                }
                return _instance;
            }
        }
        
        public static InputSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    string json = File.ReadAllText(SETTINGS_FILE);
                    return JsonSerializer.Deserialize<InputSettings>(json) ?? new InputSettings();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with defaults
                System.Diagnostics.Debug.WriteLine($"Failed to load input settings: {ex.Message}");
            }
            
            return new InputSettings();
        }
        
        public void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SETTINGS_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save input settings: {ex.Message}");
            }
        }
        
        public void ResetToDefaults()
        {
            MoveUp = Keys.W;
            MoveDown = Keys.S;
            MoveLeft = Keys.A;
            MoveRight = Keys.D;
            MoveUpAlt = Keys.Up;
            MoveDownAlt = Keys.Down;
            MoveLeftAlt = Keys.Left;
            MoveRightAlt = Keys.Right;
            InventoryToggle = Keys.Q;
            Interaction = Keys.E;
            Attack = Keys.Space;
            Pause = Keys.Escape;
            PauseMenu = Keys.Escape;
            QuickSave = Keys.F5;
            QuickLoad = Keys.F9;
            Exit = Keys.F10;
            ExitGame = Keys.F10;
            Confirm = Keys.Enter;
            Cancel = Keys.Escape;
            QuickAccessSlots = new Keys[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6 };
        }
    }
}
