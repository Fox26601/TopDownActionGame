using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using IsometricActionGame.Items;
using IsometricActionGame.SaveSystem;

namespace IsometricActionGame
{
    public enum ChestState
    {
        Closed,
        Opening,
        Open
    }

    public class Chest : IInteractable, ISaveable, IUpdateable, IDrawable
    {
        public Vector2 WorldPosition { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 0;
        public int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 0;
        public Vector2 FacingDirection { get; protected set; }
        public float LayerDepth => GridHelper.CalculateLayerDepth(WorldPosition);
        public float Scale { get; protected set; }
        public float InteractionRadius { get; protected set; }

        private ChestState _currentState;
        private bool _isContentLoaded = false;
        
        private AnimatedSprite _closedAnimation;
        private AnimatedSprite _openingAnimation;
        private AnimatedSprite _openedAnimation;
        private AnimatedSprite _currentAnimation;

        public event Action<Chest, bool> OnOpened;

        public Chest(Vector2 position)
        {
            WorldPosition = position;
            _currentState = ChestState.Closed;
            Scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE;
            InteractionRadius = GameConstants.Interaction.CHEST_INTERACTION_RADIUS; // Interaction radius for chests
            FacingDirection = Vector2.Zero; // Default, as chests don't have a facing direction
        }

        public void LoadContent(ContentManager content)
        {
            // Prevent duplicate loading
            if (_isContentLoaded && Texture != null)
            {

                return;
            }

            try
            {
                Texture = content.Load<Texture2D>("Chest");
                
                // Initialize animations with correct frame configurations
                _closedAnimation = new AnimatedSprite(
                    Texture, 
                    GameConstants.ChestSpriteFrames.CHEST_COLUMNS, 
                    GameConstants.ChestSpriteFrames.CHEST_ROWS, 
                    GameConstants.ChestSpriteFrames.CHEST_CLOSED_FRAMES, 
                    GameConstants.ChestSpriteFrames.CHEST_CLOSED_ROW, 
                    GameConstants.Animation.IDLE_FRAME_TIME, true);

                _openingAnimation = new AnimatedSprite(
                    Texture, 
                    GameConstants.ChestSpriteFrames.CHEST_COLUMNS, 
                    GameConstants.ChestSpriteFrames.CHEST_ROWS, 
                    GameConstants.ChestSpriteFrames.CHEST_OPENING_FRAMES, 
                    GameConstants.ChestSpriteFrames.CHEST_OPENING_ROW, 
                    GameConstants.Animation.ATTACK_FRAME_TIME, false);

                _openedAnimation = new AnimatedSprite(
                    Texture,
                    GameConstants.ChestSpriteFrames.CHEST_COLUMNS, 
                    GameConstants.ChestSpriteFrames.CHEST_ROWS, 
                    GameConstants.ChestSpriteFrames.CHEST_OPEN_FRAMES, 
                    GameConstants.ChestSpriteFrames.CHEST_OPEN_START_FRAME, // startColumn - начинаем с кадра 2
                    GameConstants.ChestSpriteFrames.CHEST_OPEN_ROW, // startRow - 1 indsex
                    GameConstants.Animation.IDLE_FRAME_TIME, true);

                _currentAnimation = _closedAnimation;
                _isContentLoaded = true;
                
                System.Diagnostics.Debug.WriteLine($"Chest animations loaded successfully:");
                System.Diagnostics.Debug.WriteLine($"  - Closed animation: {_closedAnimation?.FrameCount ?? 0} frames");
                System.Diagnostics.Debug.WriteLine($"  - Opening animation: {_openingAnimation?.FrameCount ?? 0} frames");
                System.Diagnostics.Debug.WriteLine($"  - Opened animation: {_openedAnimation?.FrameCount ?? 0} frames");
                System.Diagnostics.Debug.WriteLine($"  - Current animation: {_currentAnimation?.FrameCount ?? 0} frames");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Chest texture/animations: {ex.Message}");
                Texture = null;
                _isContentLoaded = false;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!_isContentLoaded || _currentAnimation == null) return;

            // Debug animation state
            if (gameTime.TotalGameTime.TotalSeconds % GameConstants.Timing.CONSOLE_FADE_DURATION < GameConstants.Animation.IDLE_FRAME_TIME) // Log every few seconds
            {
                System.Diagnostics.Debug.WriteLine($"Chest Update: State={_currentState}, CurrentFrame={_currentAnimation.CurrentFrame}, IsFinished={_currentAnimation.IsFinished}, FrameCount={_currentAnimation.FrameCount}");
            }

            _currentAnimation.Update(gameTime);

            // Handle state transitions
            if (_currentState == ChestState.Opening && _currentAnimation.IsFinished)
            {
                _currentState = ChestState.Open;
                _currentAnimation = _openedAnimation;
                _currentAnimation?.Reset();
                
                System.Diagnostics.Debug.WriteLine($"Chest transitioned to Open state. New animation: {_currentAnimation?.FrameCount ?? 0} frames");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isContentLoaded || _currentAnimation == null || Texture == null) return;

            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, SpriteEffects.None, LayerDepth, Scale);
        }

        public void OnInteract(Player player)
        {
            if (!_isContentLoaded) return;

            if (_currentState == ChestState.Closed)
            {
                _currentState = ChestState.Opening;
                _currentAnimation = _openingAnimation;
                _currentAnimation?.Reset();
                
                System.Diagnostics.Debug.WriteLine("Chest interaction: transitioning to Opening state");
                OnOpened?.Invoke(this, true);
            }
        }
        
        // ISaveable implementation
        public string SaveId => "Chest";
        
        public SaveData Serialize()
        {
            var data = new SaveData(SaveId);
            data.SetValue("Position", WorldPosition);
            data.SetValue("State", _currentState.ToString());
            return data;
        }
        
        public void Deserialize(SaveData data)
        {
            if (data == null || !_isContentLoaded) return;
            
            WorldPosition = data.GetValue<Vector2>("Position", WorldPosition);
            
            if (data.HasValue("State"))
            {
                var stateString = data.GetValue<string>("State", "Closed");
                if (Enum.TryParse<ChestState>(stateString, out var state))
                {
                    _currentState = state;
                    
                    // Set appropriate animation based on state
                    switch (state)
                    {
                        case ChestState.Closed:
                            _currentAnimation = _closedAnimation;
                            break;
                        case ChestState.Opening:
                            _currentAnimation = _openingAnimation;
                            break;
                        case ChestState.Open:
                            _currentAnimation = _openedAnimation;
                            break;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Chest deserialized: State = {state}");
                }
            }
        }
    }
}
