using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Events;
using IsometricActionGame.Graphics;
using IsometricActionGame.Core.Data;
using System;
using System.Collections.Generic;

namespace IsometricActionGame
{
    /// <summary>
    /// Simplified game restart manager that orchestrates reset operations via events
    /// Uses event-driven architecture for modularity and extensibility
    /// </summary>
    public class GameRestartManager
    {
        private readonly GameEventSystem _eventSystem;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly IResolutionManager _resolutionManager;
        
        // Only keep essential references for coordination
        private ConsoleDisplay _console;
        private LevelSystem _levelSystem;
        
        // Restart state tracking
        private bool _isRestarting = false;
        private RestartPhase _currentPhase = RestartPhase.None;
        private readonly Queue<RestartPhase> _restartPhases;
        
        public bool IsRestarting => _isRestarting;
        public RestartPhase CurrentPhase => _currentPhase;
        
        public event Action<RestartPhase> OnRestartPhaseChanged;
        public event Action OnRestartCompleted;
        public event Action<string> OnRestartFailed;
        
        public GameRestartManager(
            ContentManager content, 
            GraphicsDevice graphicsDevice, 
            IResolutionManager resolutionManager)
        {
            _eventSystem = GameEventSystem.Instance;
            _content = content;
            _graphicsDevice = graphicsDevice;
            _resolutionManager = resolutionManager;
            
            _restartPhases = new Queue<RestartPhase>();
            InitializeRestartPhases();
            
            // Subscribe to restart events
            SubscribeToEvents();
        }
        
        /// <summary>
        /// Initialize the sequence of restart phases
        /// </summary>
        private void InitializeRestartPhases()
        {
            _restartPhases.Clear();
            _restartPhases.Enqueue(RestartPhase.Initiate);
            _restartPhases.Enqueue(RestartPhase.ClearEntities);
            _restartPhases.Enqueue(RestartPhase.ResetSystems);
            _restartPhases.Enqueue(RestartPhase.ResetPlayer);
            _restartPhases.Enqueue(RestartPhase.ResetEnemies);
            _restartPhases.Enqueue(RestartPhase.ResetItems);
            _restartPhases.Enqueue(RestartPhase.ResetUI);
            _restartPhases.Enqueue(RestartPhase.Complete);
        }
        
        /// <summary>
        /// Subscribe to restart-related events
        /// </summary>
        private void SubscribeToEvents()
        {
            _eventSystem.Subscribe<object>(GameEvents.GAME_RESTART, _ => 
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: GAME_RESTART event received in GameRestartManager");
                InitiateRestart();
            });
            _eventSystem.Subscribe<object>(GameEvents.RETURN_TO_MENU, _ => InitiateMenuReturn());
        }
        
        /// <summary>
        /// Set essential references for coordination
        /// </summary>
        public void SetGameReferences(ConsoleDisplay console, LevelSystem levelSystem = null)
        {
            _console = console;
            _levelSystem = levelSystem;
        }
        
        /// <summary>
        /// Initiate a complete game restart
        /// </summary>
        public void InitiateRestart()
        {
            System.Diagnostics.Debug.WriteLine("DEBUG: GameRestartManager.InitiateRestart() called");
            
            if (_isRestarting)
            {
                _console?.AddMessage("Restart already in progress...", GameConstants.Colors.CONSOLE_YELLOW);
                return;
            }
            
            _isRestarting = true;
            _currentPhase = RestartPhase.None;
            InitializeRestartPhases();
            
            _eventSystem.Publish(GameEvents.RESTART_INITIATED, new { Timestamp = DateTime.UtcNow });
            _console?.AddMessage("Initiating game restart...", GameConstants.Colors.CONSOLE_CYAN);
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Initiate return to menu (different from restart)
        /// </summary>
        public void InitiateMenuReturn()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: GameRestartManager.InitiateMenuReturn() called");
                
                if (_isRestarting)
                {
                    _console?.AddMessage("Cannot return to menu during restart...", GameConstants.Colors.CONSOLE_YELLOW);
                    return;
                }
                
                _isRestarting = true;
                _currentPhase = RestartPhase.None;
                InitializeRestartPhases();
                
                _eventSystem.Publish(GameEvents.RETURN_TO_MENU, new { Timestamp = DateTime.UtcNow });
                _console?.AddMessage("Returning to main menu...", GameConstants.Colors.CONSOLE_CYAN);
                
                ProcessNextRestartPhase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Exception in InitiateMenuReturn: {ex.Message}");
                _console?.AddMessage($"Error returning to menu: {ex.Message}", GameConstants.Colors.CONSOLE_RED);
                _isRestarting = false;
            }
        }
        
        /// <summary>
        /// Process the next phase in the restart sequence
        /// </summary>
        private void ProcessNextRestartPhase()
        {
            if (_restartPhases.Count == 0)
            {
                CompleteRestart();
                return;
            }
            
            _currentPhase = _restartPhases.Dequeue();
            OnRestartPhaseChanged?.Invoke(_currentPhase);
            
            try
            {
                switch (_currentPhase)
                {
                    case RestartPhase.Initiate:
                        HandleInitiatePhase();
                        break;
                    case RestartPhase.ClearEntities:
                        HandleClearEntitiesPhase();
                        break;
                    case RestartPhase.ResetSystems:
                        HandleResetSystemsPhase();
                        break;
                    case RestartPhase.ResetPlayer:
                        HandleResetPlayerPhase();
                        break;
                    case RestartPhase.ResetEnemies:
                        HandleResetEnemiesPhase();
                        break;
                    case RestartPhase.ResetItems:
                        HandleResetItemsPhase();
                        break;
                    case RestartPhase.ResetUI:
                        HandleResetUIPhase();
                        break;
                    case RestartPhase.Complete:
                        HandleCompletePhase();
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleRestartError($"Error in phase {_currentPhase}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle the initiate phase
        /// </summary>
        private void HandleInitiatePhase()
        {
            _console?.AddMessage("Phase 1: Initializing restart...", GameConstants.Colors.CONSOLE_CYAN);
            
            // Request UI reset via event
            _eventSystem.Publish(GameEvents.RESET_REQUESTED, new { ResetType = "UI", IsFromSave = false });
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the clear entities phase
        /// </summary>
        private void HandleClearEntitiesPhase()
            {
                _console?.AddMessage("Phase 2: Clearing entities...", GameConstants.Colors.CONSOLE_CYAN);
                
                // Check if this is a restart from save
            bool isFromSave = _levelSystem?.IsLoadingFromSave ?? false;
            
            // In our current architecture, entity clearing is part of specific handlers:
            // - UnifiedResetEventHandler handles all reset operations through event system
            // So we don't need a dedicated "Entities" reset. Proceed to next phases.
            
                ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the reset systems phase
        /// </summary>
        private void HandleResetSystemsPhase()
        {
            _console?.AddMessage("Phase 3: Resetting systems...", GameConstants.Colors.CONSOLE_CYAN);
            
            // Clear projectiles
            try
            {
                ProjectileManager.Instance?.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error clearing projectiles: {ex.Message}");
            }
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the reset player phase
        /// </summary>
        private void HandleResetPlayerPhase()
        {
            _console?.AddMessage("Phase 4: Resetting player...", GameConstants.Colors.CONSOLE_CYAN);
            
            bool isFromSave = _levelSystem?.IsLoadingFromSave ?? false;
            _eventSystem.Publish(GameEvents.RESET_REQUESTED, new { ResetType = "Player", IsFromSave = isFromSave });
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the reset enemies phase
        /// </summary>
        private void HandleResetEnemiesPhase()
        {
            _console?.AddMessage("Phase 5: Resetting enemies...", GameConstants.Colors.CONSOLE_CYAN);
            
            bool isFromSave = _levelSystem?.IsLoadingFromSave ?? false;
            _eventSystem.Publish(GameEvents.RESET_REQUESTED, new { ResetType = "Enemies", IsFromSave = isFromSave });
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the reset items phase
        /// </summary>
        private void HandleResetItemsPhase()
        {
            _console?.AddMessage("Phase 6: Resetting items...", GameConstants.Colors.CONSOLE_CYAN);
            
            bool isFromSave = _levelSystem?.IsLoadingFromSave ?? false;
            _eventSystem.Publish(GameEvents.RESET_REQUESTED, new { ResetType = "Items", IsFromSave = isFromSave });
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the reset UI phase
        /// </summary>
        private void HandleResetUIPhase()
        {
            _console?.AddMessage("Phase 7: Resetting UI...", GameConstants.Colors.CONSOLE_CYAN);
            
            bool isFromSave = _levelSystem?.IsLoadingFromSave ?? false;
            _eventSystem.Publish(GameEvents.RESET_REQUESTED, new { ResetType = "UI", IsFromSave = isFromSave });
            
            ProcessNextRestartPhase();
        }
        
        /// <summary>
        /// Handle the complete phase
        /// </summary>
        private void HandleCompletePhase()
        {
            _console?.AddMessage("Phase 8: Completing restart...", GameConstants.Colors.CONSOLE_CYAN);
            
            CompleteRestart();
        }
        
        /// <summary>
        /// Complete the restart process
        /// </summary>
        private void CompleteRestart()
        {
            _isRestarting = false;
            _currentPhase = RestartPhase.None;
            
            _eventSystem.Publish(GameEvents.RESTART_COMPLETED, new { Timestamp = DateTime.UtcNow });
            _console?.AddMessage("Game restart completed!", GameConstants.Colors.CONSOLE_GREEN);
            
            OnRestartCompleted?.Invoke();
        }
        
        /// <summary>
        /// Handle restart errors
        /// </summary>
        private void HandleRestartError(string errorMessage)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Restart error: {errorMessage}");
            _console?.AddMessage($"Restart error: {errorMessage}", GameConstants.Colors.CONSOLE_RED);
            
            _isRestarting = false;
            _currentPhase = RestartPhase.None;
            
            _eventSystem.Publish(GameEvents.RESTART_FAILED, errorMessage);
            OnRestartFailed?.Invoke(errorMessage);
        }
        
        /// <summary>
        /// Get current restart progress as percentage
        /// </summary>
        public float GetRestartProgress()
        {
            if (!_isRestarting) return 0f;
            
            int totalPhases = 8; // Total number of restart phases
            int completedPhases = totalPhases - _restartPhases.Count;
            
            return (float)completedPhases / totalPhases;
        }
        
        /// <summary>
        /// Update the restart manager
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Currently no continuous updates needed
            // This method is called for consistency with other game systems
        }
    }
    
    /// <summary>
    /// Enumeration of restart phases for tracking progress
    /// </summary>
    public enum RestartPhase
    {
        None,
        Initiate,
        ClearEntities,
        ResetSystems,
        ResetPlayer,
        ResetEnemies,
        ResetItems,
        ResetUI,
        Complete
    }
}
