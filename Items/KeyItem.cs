using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using System;
using IsometricActionGame.Items;

namespace IsometricActionGame
{
    // Represents a key item that can unlock doors
    public class KeyItem : Item
    {
        private Texture2D _texture;
        
        public bool IsPickedUp { get; private set; }

        // Creates a new KeyItem
        public KeyItem() : base("Key", "A mysterious key that can unlock doors", false, 1)
        {
            IsPickedUp = false;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                _texture = content.Load<Texture2D>("key");
                SetIcon(_texture);
            }
            catch (Exception)
            {
                // Create a fallback texture for debugging
                var graphicsDevice = content.ServiceProvider.GetService(typeof(GraphicsDevice)) as GraphicsDevice;
                if (graphicsDevice != null)
                {
                    _texture = new Texture2D(graphicsDevice, GameConstants.Items.DEBUG_TEXTURE_SIZE, GameConstants.Items.DEBUG_TEXTURE_SIZE);
                    var colorData = new Color[GameConstants.Items.DEBUG_TEXTURE_SIZE * GameConstants.Items.DEBUG_TEXTURE_SIZE];
                    for (int i = 0; i < colorData.Length; i++)
                    {
                        colorData[i] = Color.Gold;
                    }
                    _texture.SetData(colorData);
                    SetIcon(_texture);
                }
            }
        }

        public override Item Clone()
        {
            var clone = new KeyItem();
            // Copy the icon if it's already loaded
            if (Icon != null)
            {
                clone.SetIcon(Icon);
            }
            return clone;
        }

        public override bool Use(Player player)
        {
            // Keys cannot be used directly - they are only used to unlock doors
            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE)
        {
            if (Icon != null)
            {
                // Use the proper icon texture with correct scaling for inventory
                float inventoryScale = GameConstants.SpriteScale.KEY_INVENTORY_SCALE;
                spriteBatch.Draw(Icon, position, null, Color.White, 0f, Vector2.Zero, inventoryScale, SpriteEffects.None, GameConstants.LayerDepth.DEFAULT_DEPTH);
            }
            else
            {
                // Fallback: create a simple gold rectangle only if texture loading failed
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.Gold });
                float inventoryScale = GameConstants.SpriteScale.KEY_INVENTORY_SCALE;
                spriteBatch.Draw(debugTexture, position, null, Color.Gold, 0f, Vector2.Zero, new Vector2(GameConstants.Items.DEBUG_TEXTURE_SIZE, GameConstants.Items.DEBUG_TEXTURE_SIZE) * inventoryScale, SpriteEffects.None, GameConstants.Graphics.DEFAULT_SPRITE_SCALE);
            }
        }
    }
} 