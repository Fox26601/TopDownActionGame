using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using IsometricActionGame.Events;

namespace IsometricActionGame.UI
{
    /// <summary>
    /// UI panel for level transitions showing battle statistics and navigation options
    /// </summary>
    public class LevelTransitionPanel : IUIElement
    {
        private SpriteFont _font;
        private Button _menuButton;
        private Button _nextLevelButton;
        private bool _isVisible;
        private int _screenWidth;
        private int _screenHeight;
        
        // Level data
        private int _completedLevel;
        private string _nextLevelName;
        private string _nextLevelDescription;
        
        // Battle statistics
        private int _enemiesKilled;
        private int _questsCompleted;
        private int _itemsCollected;
        private TimeSpan _levelTime;
        
        // Save system data
        private Dictionary<string, int> _enemyKillCounts;
        private TimeSpan _totalPlayTime;
        
        // Event system reference
        private GameEventSystem _eventSystem;
        
        // UI Layout constants (will be scaled by UIScalingManager)
        private const int PANEL_WIDTH = 900;
        private const int PANEL_HEIGHT = 700;
        private const int TITLE_TOP_MARGIN = 20;
        private const int STATS_TOP_MARGIN = 80;
        private const int BUTTON_LIFT_OFFSET = 5;
        
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => _isVisible = value; 
        }
        public bool IsEnabled { get; set; } = true;
        public event Action OnMenu;
        public event Action OnNextLevel;
        public event Action OnReturnToGame;

        public LevelTransitionPanel(SpriteFont font, int screenWidth, int screenHeight)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel constructor: font={font != null}, screen={screenWidth}x{screenHeight}");
                
                _font = font;
                _isVisible = false;
                _screenWidth = screenWidth;
                _screenHeight = screenHeight;
                
                // Initialize UI scaling
                UIScalingManager.Initialize(screenWidth, screenHeight);
                
                // Use scaled button dimensions
                int buttonWidth = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_WIDTH);
                int buttonHeight = UIScalingManager.ScaleValue(GameConstants.UI.BUTTON_HEIGHT);
                int spacing = UIScalingManager.ScaleValue(GameConstants.UI.MARGIN_LARGE);
                int liftOffset = UIScalingManager.ScaleValue(BUTTON_LIFT_OFFSET);
                
                // Get centered positions for buttons
                var centerPos = UIScalingManager.GetCenteredPosition(buttonWidth, buttonHeight);
                int centerX = (int)centerPos.X;
                int centerY = (int)centerPos.Y + UIScalingManager.ScaleValue(120); // Position below stats
                
                // Create buttons with scaled positions and lift offset
                _menuButton = new Button(
                    new Rectangle(centerX - buttonWidth - spacing/2, centerY - liftOffset, buttonWidth, buttonHeight),
                    "Menu"
                );
                
                _nextLevelButton = new Button(
                    new Rectangle(centerX + spacing/2, centerY - liftOffset, buttonWidth, buttonHeight),
                    "Next Level"
                );
                
                // Subscribe to button events
                _menuButton.OnClick += () => OnMenu?.Invoke();
                _nextLevelButton.OnClick += () => OnNextLevel?.Invoke();
                
                System.Diagnostics.Debug.WriteLine("LevelTransitionPanel constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LevelTransitionPanel constructor: {ex}");
                // Fallback to simple buttons if scaling fails
                _menuButton = new Button(
                    new Rectangle(screenWidth / 2 - 250, screenHeight / 2 + 100, 200, 50),
                    "Menu"
                );
                _nextLevelButton = new Button(
                    new Rectangle(screenWidth / 2 + 50, screenHeight / 2 + 100, 200, 50),
                    "Next Level"
                );
                _menuButton.OnClick += () => OnMenu?.Invoke();
                _nextLevelButton.OnClick += () => OnNextLevel?.Invoke();
            }
        }

        public void Show(int completedLevel, string nextLevelName, string nextLevelDescription, 
                        int enemiesKilled, int questsCompleted, int itemsCollected, TimeSpan levelTime)
        {
            System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel.Show called with: level={completedLevel}, enemies={enemiesKilled}, quests={questsCompleted}, items={itemsCollected}, time={levelTime}");
            
            _completedLevel = completedLevel;
            _nextLevelName = nextLevelName;
            _nextLevelDescription = nextLevelDescription;
            _enemiesKilled = enemiesKilled;
            _questsCompleted = questsCompleted;
            _itemsCollected = itemsCollected;
            _levelTime = levelTime;
            _isVisible = true;
            
            // Publish event that transition panel is shown
            if (_eventSystem != null)
            {
                _eventSystem.Publish(GameEvents.LEVEL_TRANSITION_PANEL_SHOWN, new { });
            }
            
            System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel.Show completed: _isVisible={_isVisible}");
        }

        public void SetEventSystem(GameEventSystem eventSystem)
        {
            System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel.SetEventSystem called - eventSystem: {eventSystem != null}");
            _eventSystem = eventSystem;
        }
        
        public void Hide()
        {
            System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel.Hide called - _isVisible: {_isVisible}, _eventSystem: {_eventSystem != null}");
            
            _isVisible = false;
            
            // Trigger return to game event
            OnReturnToGame?.Invoke();
            
            // Publish event that transition panel is hidden
            if (_eventSystem != null)
            {
                System.Diagnostics.Debug.WriteLine("LevelTransitionPanel.Hide: Publishing LEVEL_TRANSITION_PANEL_HIDDEN event");
                _eventSystem.Publish(GameEvents.LEVEL_TRANSITION_PANEL_HIDDEN, new { });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("LevelTransitionPanel.Hide: _eventSystem is null, cannot publish event!");
            }
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            _menuButton.LoadContent(content);
            _nextLevelButton.LoadContent(content);
        }

        public void Update(GameTime gameTime)
        {
            if (!_isVisible || !IsEnabled) return;
            
            // ESC handling is done in InputHandler to avoid double processing
            // Update buttons
            _menuButton.Update(gameTime);
            _nextLevelButton.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                if (!_isVisible) 
                {
                    System.Diagnostics.Debug.WriteLine("LevelTransitionPanel.Draw: not visible, returning");
                    return;
                }
                
                if (_font == null)
                {
                    System.Diagnostics.Debug.WriteLine("LevelTransitionPanel.Draw: _font is null!");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel.Draw: drawing panel with data - level={_completedLevel}, enemies={_enemiesKilled}, quests={_questsCompleted}, items={_itemsCollected}");
                
                // Semi-transparent background covering full screen - unified with other UI
                var screenRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), screenRect, Color.Black * GameConstants.UI.OVERLAY_OPACITY);
                
                // Panel background with scaled dimensions - unified with other UI
                int panelWidth = UIScalingManager.ScaleValue(PANEL_WIDTH);
                int panelHeight = UIScalingManager.ScaleValue(PANEL_HEIGHT);
                var panelCenterPos = UIScalingManager.GetCenteredPosition(panelWidth, panelHeight);
                var panelRect = new Rectangle(
                    (int)panelCenterPos.X, 
                    (int)panelCenterPos.Y, 
                    panelWidth, 
                    panelHeight
                );
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), panelRect, Color.DarkGray * GameConstants.UI.PANEL_OPACITY);
                
                // Panel border - unified with other UI
                var borderThickness = 3;
                var borderColor = Color.LightGray;
                
                // Top border
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                    new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, borderThickness), borderColor);
                // Bottom border
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                    new Rectangle(panelRect.X, panelRect.Bottom - borderThickness, panelRect.Width, borderThickness), borderColor);
                // Left border
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                    new Rectangle(panelRect.X, panelRect.Y, borderThickness, panelRect.Height), borderColor);
                // Right border
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), 
                    new Rectangle(panelRect.Right - borderThickness, panelRect.Y, borderThickness, panelRect.Height), borderColor);
                
                // Title
                string title = $"LEVEL {_completedLevel} COMPLETED!";
                var titleSize = _font.MeasureString(title);
                var titlePos = new Vector2(
                    (_screenWidth - titleSize.X) / 2,
                    panelRect.Y + UIScalingManager.ScaleValue(TITLE_TOP_MARGIN)
                );
                spriteBatch.DrawString(_font, title, titlePos, Color.Gold);
                
                // Battle Statistics
                var statsY = panelRect.Y + UIScalingManager.ScaleValue(STATS_TOP_MARGIN);
                var statsSpacing = UIScalingManager.ScaleValue(25);
                
                // Enemies Killed
                string enemiesText = $"Enemies Killed: {_enemiesKilled}";
                var enemiesSize = _font.MeasureString(enemiesText);
                var enemiesPos = new Vector2((_screenWidth - enemiesSize.X) / 2, statsY);
                spriteBatch.DrawString(_font, enemiesText, enemiesPos, Color.White);
                
                // Quests Completed
                string questsText = $"Quests Completed: {_questsCompleted}";
                var questsSize = _font.MeasureString(questsText);
                var questsPos = new Vector2((_screenWidth - questsSize.X) / 2, statsY + statsSpacing);
                spriteBatch.DrawString(_font, questsText, questsPos, Color.White);
                
                // Items Collected
                string itemsText = $"Items Collected: {_itemsCollected}";
                var itemsSize = _font.MeasureString(itemsText);
                var itemsPos = new Vector2((_screenWidth - itemsSize.X) / 2, statsY + statsSpacing * 2);
                spriteBatch.DrawString(_font, itemsText, itemsPos, Color.White);
                
                // Level Time
                string timeText = $"Level Time: {_levelTime:mm\\:ss}";
                var timeSize = _font.MeasureString(timeText);
                var timePos = new Vector2((_screenWidth - timeSize.X) / 2, statsY + statsSpacing * 3);
                spriteBatch.DrawString(_font, timeText, timePos, Color.White);
                
                // Next Level Info
                if (!string.IsNullOrEmpty(_nextLevelName))
                {
                    string nextLevelTitle = $"Next Level: {_nextLevelName}";
                    var nextLevelTitleSize = _font.MeasureString(nextLevelTitle);
                    var nextLevelTitlePos = new Vector2(
                        (_screenWidth - nextLevelTitleSize.X) / 2,
                        statsY + statsSpacing * 4 + UIScalingManager.ScaleValue(10)
                    );
                    spriteBatch.DrawString(_font, nextLevelTitle, nextLevelTitlePos, Color.Cyan);
                    
                                    // Description with text wrapping
                if (!string.IsNullOrEmpty(_nextLevelDescription))
                {
                    var maxDescriptionWidth = panelWidth - UIScalingManager.ScaleValue(40); // Leave margins
                    var wrappedDescription = WrapText(_nextLevelDescription, maxDescriptionWidth);
                    var descriptionY = nextLevelTitlePos.Y + nextLevelTitleSize.Y + UIScalingManager.ScaleValue(5);
                    
                    foreach (var line in wrappedDescription)
                    {
                        var lineSize = _font.MeasureString(line);
                        var linePos = new Vector2(
                            (_screenWidth - lineSize.X) / 2,
                            descriptionY
                        );
                        spriteBatch.DrawString(_font, line, linePos, Color.LightGray);
                        descriptionY += lineSize.Y + UIScalingManager.ScaleValue(2);
                    }
                }
                }
                
                // Instructions - improved text
                string instruction = "Press ENTER to Continue to Next Level | ESC to Close Panel";
                var instructionSize = _font.MeasureString(instruction);
                var instructionPos = new Vector2(
                    (_screenWidth - instructionSize.X) / 2,
                    panelRect.Bottom - instructionSize.Y - UIScalingManager.ScaleValue(20)
                );
                spriteBatch.DrawString(_font, instruction, instructionPos, Color.Yellow);
                
                // Buttons
                System.Diagnostics.Debug.WriteLine($"LevelTransitionPanel.Draw: Drawing buttons - _menuButton={_menuButton != null}, _nextLevelButton={_nextLevelButton != null}");
                _menuButton?.Draw(spriteBatch);
                _nextLevelButton?.Draw(spriteBatch);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LevelTransitionPanel.Draw: {ex}");
                // Fallback to simple text display if drawing fails
                if (_isVisible && _font != null)
                {
                    string fallbackText = $"Level {_completedLevel} Completed! Press ENTER for Next Level or ESC for Pause Menu.";
                    var textSize = _font.MeasureString(fallbackText);
                    var textPos = new Vector2((_screenWidth - textSize.X) / 2, _screenHeight / 2);
                    spriteBatch.DrawString(_font, fallbackText, textPos, Color.White);
                }
            }
        }

        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }
        
        /// <summary>
        /// Update save system data for display
        /// </summary>
        public void UpdateSaveData(Dictionary<string, int> enemyKillCounts, TimeSpan totalPlayTime)
        {
            _enemyKillCounts = enemyKillCounts ?? new Dictionary<string, int>();
            _totalPlayTime = totalPlayTime;
        }
        
        /// <summary>
        /// Get enemy kill counts for save system
        /// </summary>
        public Dictionary<string, int> GetEnemyKillCounts()
        {
            return _enemyKillCounts ?? new Dictionary<string, int>();
        }
        
        /// <summary>
        /// Get total play time
        /// </summary>
        public TimeSpan GetTotalPlayTime()
        {
            return _totalPlayTime;
        }
        
        /// <summary>
        /// Wrap text to fit within specified width
        /// </summary>
        private List<string> WrapText(string text, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";
            
            foreach (var word in words)
            {
                var testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
                var testSize = _font.MeasureString(testLine);
                
                if (testSize.X <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        // Single word is too long, add it anyway
                        lines.Add(word);
                    }
                }
            }
            
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine);
            }
            
            return lines;
        }
    }
}
