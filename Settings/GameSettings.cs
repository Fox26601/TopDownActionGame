using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Settings
{
    /// <summary>
    /// Manages game settings with persistence and validation
    /// </summary>
    public class GameSettings
    {
        private const string SETTINGS_FILE = "Settings/gamesettings.json";
        
        public Resolution CurrentResolution { get; private set; }
        public bool IsFullscreen { get; private set; }
        public float MasterVolume { get; private set; } = GameConstants.Settings.DEFAULT_MASTER_VOLUME;
        public float MusicVolume { get; private set; } = GameConstants.Settings.DEFAULT_MUSIC_VOLUME;
        public float SFXVolume { get; private set; } = GameConstants.Settings.DEFAULT_SFX_VOLUME;
        
        // Track pending changes to prevent unnecessary events
        private Resolution _pendingResolution;
        private bool _pendingFullscreen;
        private bool _hasPendingChanges = false;
        
        public event Action<Resolution> OnResolutionChanged;
        public event Action<bool> OnFullscreenChanged;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action OnSettingsApplied;
        
        private static readonly List<Resolution> SupportedResolutions = new List<Resolution>
        {
            new Resolution(800, 600),
            new Resolution(1024, 768),
            new Resolution(GameConstants.Settings.DEFAULT_RESOLUTION_WIDTH, GameConstants.Settings.DEFAULT_RESOLUTION_HEIGHT),
            new Resolution(1366, 768),
            new Resolution(1440, 900),
            new Resolution(1600, 900),
            new Resolution(1920, 1080),
            new Resolution(2560, 1440),
            new Resolution(3840, 2160)
        };
        
        public IReadOnlyList<Resolution> AvailableResolutions => SupportedResolutions;
        
        public GameSettings()
        {
            // Default settings
            CurrentResolution = new Resolution(GameConstants.Settings.DEFAULT_RESOLUTION_WIDTH, GameConstants.Settings.DEFAULT_RESOLUTION_HEIGHT);
            IsFullscreen = false;
            _pendingResolution = CurrentResolution;
            _pendingFullscreen = IsFullscreen;
        }
        
        /// <summary>
        /// Load settings from file or use defaults
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    string json = File.ReadAllText(SETTINGS_FILE);
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    
                    if (settings != null)
                    {
                        CurrentResolution = settings.Resolution ?? new Resolution(GameConstants.Settings.DEFAULT_RESOLUTION_WIDTH, GameConstants.Settings.DEFAULT_RESOLUTION_HEIGHT);
                        IsFullscreen = settings.IsFullscreen;
                        MasterVolume = MathHelper.Clamp(settings.MasterVolume, GameConstants.Settings.MIN_VOLUME, GameConstants.Settings.MAX_VOLUME);
                        MusicVolume = MathHelper.Clamp(settings.MusicVolume, GameConstants.Settings.MIN_VOLUME, GameConstants.Settings.MAX_VOLUME);
                        SFXVolume = MathHelper.Clamp(settings.SFXVolume, GameConstants.Settings.MIN_VOLUME, GameConstants.Settings.MAX_VOLUME);
                        
                        // Initialize pending values to current values
                        _pendingResolution = CurrentResolution;
                        _pendingFullscreen = IsFullscreen;
                        _hasPendingChanges = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error and use defaults
                Console.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Save current settings to file
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var settings = new SettingsData
                {
                    Resolution = CurrentResolution,
                    IsFullscreen = IsFullscreen,
                    MasterVolume = MasterVolume,
                    MusicVolume = MusicVolume,
                    SFXVolume = SFXVolume
                };
                
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SETTINGS_FILE, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set pending resolution (doesn't apply immediately)
        /// </summary>
        public void SetPendingResolution(Resolution resolution)
        {
            if (resolution != _pendingResolution)
            {
                _pendingResolution = resolution;
                _hasPendingChanges = true;
            }
        }
        
        /// <summary>
        /// Set pending fullscreen state (doesn't apply immediately)
        /// </summary>
        public void SetPendingFullscreen(bool isFullscreen)
        {
            if (isFullscreen != _pendingFullscreen)
            {
                _pendingFullscreen = isFullscreen;
                _hasPendingChanges = true;
            }
        }
        
        /// <summary>
        /// Apply pending settings and notify listeners
        /// </summary>
        public void ApplyPendingSettings()
        {
            if (!_hasPendingChanges) return;
            
            bool resolutionChanged = false;
            bool fullscreenChanged = false;
            
            // Apply resolution if changed
            if (_pendingResolution != CurrentResolution)
            {
                CurrentResolution = _pendingResolution;
                resolutionChanged = true;
            }
            
            // Apply fullscreen if changed
            if (_pendingFullscreen != IsFullscreen)
            {
                IsFullscreen = _pendingFullscreen;
                fullscreenChanged = true;
            }
            
            // Notify listeners only for actual changes
            if (resolutionChanged)
            {
                OnResolutionChanged?.Invoke(CurrentResolution);
            }
            
            if (fullscreenChanged)
            {
                OnFullscreenChanged?.Invoke(IsFullscreen);
            }
            
            // Save settings and notify that settings were applied
            SaveSettings();
            OnSettingsApplied?.Invoke();
            
            _hasPendingChanges = false;
        }
        
        /// <summary>
        /// Cancel pending changes and reset to current values
        /// </summary>
        public void CancelPendingChanges()
        {
            _pendingResolution = CurrentResolution;
            _pendingFullscreen = IsFullscreen;
            _hasPendingChanges = false;
        }
        
        /// <summary>
        /// Check if there are pending changes
        /// </summary>
        public bool HasPendingChanges => _hasPendingChanges;
        
        /// <summary>
        /// Get pending resolution
        /// </summary>
        public Resolution PendingResolution => _pendingResolution;
        
        /// <summary>
        /// Get pending fullscreen state
        /// </summary>
        public bool PendingFullscreen => _pendingFullscreen;
        
        /// <summary>
        /// Set resolution and notify listeners (legacy method for backward compatibility)
        /// </summary>
        public void SetResolution(Resolution resolution)
        {
            SetPendingResolution(resolution);
            ApplyPendingSettings();
        }
        
        /// <summary>
        /// Set fullscreen mode and notify listeners (legacy method for backward compatibility)
        /// </summary>
        public void SetFullscreen(bool isFullscreen)
        {
            SetPendingFullscreen(isFullscreen);
            ApplyPendingSettings();
        }
        
        /// <summary>
        /// Update resolution without triggering events (for internal sync)
        /// </summary>
        public void UpdateResolution(Resolution resolution)
        {
            CurrentResolution = resolution;
            _pendingResolution = resolution;
        }
        
        /// <summary>
        /// Update fullscreen without triggering events (for internal sync)
        /// </summary>
        public void UpdateFullscreen(bool isFullscreen)
        {
            IsFullscreen = isFullscreen;
            _pendingFullscreen = isFullscreen;
        }
        
        /// <summary>
        /// Set master volume and notify listeners
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            float clampedVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
            if (clampedVolume != MasterVolume)
            {
                MasterVolume = clampedVolume;
                OnMasterVolumeChanged?.Invoke(clampedVolume);
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Set music volume and notify listeners
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            float clampedVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
            if (clampedVolume != MusicVolume)
            {
                MusicVolume = clampedVolume;
                OnMusicVolumeChanged?.Invoke(clampedVolume);
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Set SFX volume and notify listeners
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            float clampedVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
            if (clampedVolume != SFXVolume)
            {
                SFXVolume = clampedVolume;
                OnSFXVolumeChanged?.Invoke(clampedVolume);
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            var defaultResolution = new Resolution(1280, 720);
            var defaultFullscreen = false;
            
            SetPendingResolution(defaultResolution);
            SetPendingFullscreen(defaultFullscreen);
            SetMasterVolume(1.0f);
            SetMusicVolume(0.8f);
            SetSFXVolume(0.9f);
            
            ApplyPendingSettings();
        }
        
        /// <summary>
        /// Check if resolution is supported
        /// </summary>
        private bool IsResolutionSupported(Resolution resolution)
        {
            return SupportedResolutions.Contains(resolution);
        }
        
        /// <summary>
        /// Get next resolution in the list
        /// </summary>
        public Resolution GetNextResolution()
        {
            int currentIndex = SupportedResolutions.IndexOf(CurrentResolution);
            int nextIndex = (currentIndex + 1) % SupportedResolutions.Count;
            return SupportedResolutions[nextIndex];
        }
        
        /// <summary>
        /// Get previous resolution in the list
        /// </summary>
        public Resolution GetPreviousResolution()
        {
            int currentIndex = SupportedResolutions.IndexOf(CurrentResolution);
            int prevIndex = (currentIndex - 1 + SupportedResolutions.Count) % SupportedResolutions.Count;
            return SupportedResolutions[prevIndex];
        }
        
        /// <summary>
        /// Data structure for JSON serialization
        /// </summary>
        private class SettingsData
        {
            public Resolution? Resolution { get; set; }
            public bool IsFullscreen { get; set; }
            public float MasterVolume { get; set; }
            public float MusicVolume { get; set; }
            public float SFXVolume { get; set; }
        }
    }
    
    /// <summary>
    /// Represents a screen resolution
    /// </summary>
    public struct Resolution : IEquatable<Resolution>
    {
        public int Width { get; }
        public int Height { get; }
        
        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        public override string ToString()
        {
            return $"{Width} x {Height}";
        }
        
        public bool Equals(Resolution other)
        {
            return Width == other.Width && Height == other.Height;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Resolution other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }
        
        public static bool operator ==(Resolution left, Resolution right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(Resolution left, Resolution right)
        {
            return !left.Equals(right);
        }
    }
    

}

