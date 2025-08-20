using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.SaveSystem;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;
using System;

namespace IsometricActionGame
{
    public class Door : IEntity, IInteractable, ISaveable, IUpdateable, IDrawable
    {
        public Vector2 WorldPosition { get; protected set; }
        public Texture2D Texture { get; protected set; }
        public int SpriteWidth { get; protected set; }
        public int SpriteHeight { get; protected set; }
        public Vector2 FacingDirection { get; protected set; } // For rendering consistency
        public float LayerDepth => GridHelper.CalculateLayerDepth(WorldPosition);
        public float Scale { get; protected set; }
        public float BaseHitboxRadius => GameConstants.Combat.DOOR_HITBOX_RADIUS; // Base hitbox radius for doors
        public float InteractionRadius { get; protected set; }
        public bool IsOpen { get; private set; }
        public bool IsOpening { get; private set; }
        public bool IsClosing { get; private set; }

        // Animation properties - simplified to static frames only
        private AnimatedSprite _doorAnimation;
        private AnimatedSprite _currentAnimation;

        public event Action<Door> OnOpened;

        public Door(Vector2 position)
        {
            WorldPosition = position;
            IsOpen = false;
            IsOpening = false;
            IsClosing = false;
            Scale = GameConstants.SpriteScale.DOOR_SCALE;
            InteractionRadius = GameConstants.Interaction.DOOR_INTERACTION_RADIUS;
            FacingDirection = Vector2.Zero;
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                Texture = content.Load<Texture2D>("door");
                
                if (Texture != null)
                {
                    SpriteWidth = Texture.Width;
                    SpriteHeight = Texture.Height / GameConstants.DoorSpriteFrames.DOOR_ROWS;
                    
                    // Create door animation with all frames from all rows
                    _doorAnimation = new AnimatedSprite(
                        Texture, 
                        GameConstants.DoorSpriteFrames.DOOR_COLUMNS, 
                        GameConstants.DoorSpriteFrames.DOOR_ROWS, 
                        GameConstants.Animation.IDLE_FRAME_TIME, // Frame time (not used for static frames)
                        false); // No looping
                    
                    // Set initial animation to closed door
                    _currentAnimation = _doorAnimation;
                    _currentAnimation.SetFrame(GameConstants.DoorSpriteFrames.DOOR_CLOSED_FRAME);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Door texture: {ex.Message}");
            }
        }
        


        public void Update(GameTime gameTime)
        {
            if (_currentAnimation == null) return;

            // Set the correct frame based on door state
            if (IsOpen)
            {
                _currentAnimation.SetFrame(GameConstants.DoorSpriteFrames.DOOR_OPEN_FRAME);
            }
            else
            {
                _currentAnimation.SetFrame(GameConstants.DoorSpriteFrames.DOOR_CLOSED_FRAME);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            if (Texture == null)
            {
                // Fallback: Draw a simple red rectangle
                var rect = new Rectangle((int)screenPos.X - 16, (int)screenPos.Y - 16, 32, 32);
                spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), rect, Color.Red);
                return;
            }
            
            if (_currentAnimation == null)
            {
                // Fallback: Draw texture directly
                spriteBatch.Draw(Texture, screenPos, null, Color.White, 0f, new Vector2(SpriteWidth / 2, SpriteHeight / 2), Scale, SpriteEffects.None, LayerDepth);
                return;
            }
            
            _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, SpriteEffects.None, LayerDepth, Scale);
        }
        
        private Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            Texture2D pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        }

        public void OnInteract(Player player)
        {
            try
            {
                if (player == null) return;

                // Don't allow interaction during animation
                if (IsOpening || IsClosing) return;

                if (!IsOpen && player.HasKey)
                {
                    // Open door immediately
                    IsOpen = true;
                    
                    // Remove key BEFORE triggering event
                    player.RemoveKey(); // Player loses key after opening door
                    
                    // Safe event invocation with null check
                    if (OnOpened != null)
                    {
                        try
                        {
                            OnOpened.Invoke(this);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error invoking OnOpened event: {ex.Message}");
                        }
                    }
                }
                else if (!IsOpen && !player.HasKey)
                {
                    // Door remains locked - no event triggered
                }
                else if (IsOpen)
                {
                    // Door is already open - always allow interaction to show level completion panel
                    if (OnOpened != null)
                    {
                        try
                        {
                            OnOpened.Invoke(this);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error invoking OnOpened event: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Door.OnInteract: {ex.Message}");
            }
        }


        
        /// <summary>
        /// Reset the door to its initial state
        /// </summary>
        public void Reset()
        {
            IsOpen = false;
            IsOpening = false;
            IsClosing = false;
            if (_currentAnimation != null)
            {
                _currentAnimation.SetFrame(GameConstants.DoorSpriteFrames.DOOR_CLOSED_FRAME);
            }
        }
        
        /// <summary>
        /// Set the door position
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            WorldPosition = position;
        }
        
        // ISaveable implementation
        public string SaveId => "Door";
        
        public SaveData Serialize()
        {
            var data = new SaveData(SaveId);
            data.SetValue("Position", WorldPosition);
            data.SetValue("IsOpen", IsOpen);
            data.SetValue("CurrentFrame", _currentAnimation?.CurrentFrame ?? GameConstants.DoorSpriteFrames.DOOR_CLOSED_FRAME);
            return data;
        }
        
        public void Deserialize(SaveData data)
        {
            if (data == null) return;
            
            WorldPosition = data.GetValue<Vector2>("Position", WorldPosition);
            IsOpen = data.GetValue<bool>("IsOpen", IsOpen);
            int currentFrame = data.GetValue<int>("CurrentFrame", GameConstants.DoorSpriteFrames.DOOR_CLOSED_FRAME);
            
            // Ensure frame is within valid range
            currentFrame = MathHelper.Clamp(currentFrame, 0, GameConstants.DoorSpriteFrames.DOOR_COLUMNS - 1);
            
            if (_currentAnimation != null)
            {
                _currentAnimation.SetFrame(currentFrame);
            }
        }
    }
}
