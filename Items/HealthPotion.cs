using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using System;

namespace IsometricActionGame.Items
{
    public class HealthPotion : Item
    {
        public int HealAmount { get; private set; } = GameConstants.Health.HEALTH_POTION_DEFAULT_HEAL;

        public HealthPotion() : base("Health Potion", "Restores health points", false, GameConstants.Items.DEFAULT_ITEM_QUANTITY)
        {
            IsUsable = true;
            HealAmount = GameConstants.Health.HEALTH_POTION_DEFAULT_HEAL;
        }

        public HealthPotion(int healAmount) : base("Health Potion", "Restores health points", false, GameConstants.Items.DEFAULT_ITEM_QUANTITY)
        {
            IsUsable = true;
            HealAmount = healAmount;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                Icon = content.Load<Texture2D>("health_potion");
            }
            catch
            {
                // Create a simple red texture for health potion as fallback
                var graphicsDevice = content.ServiceProvider.GetService(typeof(GraphicsDevice)) as GraphicsDevice;
                if (graphicsDevice != null)
                {
                    Icon = new Texture2D(graphicsDevice, GameConstants.Items.DEBUG_TEXTURE_SIZE, GameConstants.Items.DEBUG_TEXTURE_SIZE);
                    var colorData = new Color[GameConstants.Items.DEBUG_TEXTURE_SIZE * GameConstants.Items.DEBUG_TEXTURE_SIZE];
                    for (int i = 0; i < colorData.Length; i++)
                    {
                        colorData[i] = Color.Red;
                    }
                    Icon.SetData(colorData);
                }
            }
        }

        public override bool Use(Player player)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"HealthPotion.Use: Called with player health: {player?.CurrentHealth}/{player?.MaxHealth}");
                
                if (player == null)
                {
                    System.Diagnostics.Debug.WriteLine($"HealthPotion.Use: Player is null");
                    return false;
                }
                
                // Always try to heal, even if at full health (for consistency)
                System.Diagnostics.Debug.WriteLine($"HealthPotion.Use: Attempting to heal player by {HealAmount}");
                player.Heal(HealAmount);
                System.Diagnostics.Debug.WriteLine($"HealthPotion.Use: Heal completed, new health: {player.CurrentHealth}/{player.MaxHealth}");
                
                // Note: Quantity is decremented in Inventory.UseQuickAccessItem
                // Do not decrement here to avoid double decrement
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HealthPotion.Use: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"HealthPotion.Use: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE)
        {
            if (Icon != null)
            {
                // Use the proper icon texture with correct scaling for inventory
                float inventoryScale = GameConstants.SpriteScale.HEALTH_POTION_INVENTORY_SCALE;
                spriteBatch.Draw(Icon, position, null, Color.White, 0f, Vector2.Zero, inventoryScale, SpriteEffects.None, GameConstants.LayerDepth.DEFAULT_DEPTH);
            }
            else
            {
                // Fallback: create a simple red rectangle only if texture loading failed
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.Red });
                float inventoryScale = GameConstants.SpriteScale.HEALTH_POTION_INVENTORY_SCALE;
                spriteBatch.Draw(debugTexture, position, null, Color.Red, 0f, Vector2.Zero, new Vector2(GameConstants.Items.DEBUG_TEXTURE_SIZE, GameConstants.Items.DEBUG_TEXTURE_SIZE) * inventoryScale, SpriteEffects.None, GameConstants.Graphics.DEFAULT_SPRITE_SCALE);
            }
        }

        public override Item Clone()
        {
            var clone = new HealthPotion();
            clone.Quantity = this.Quantity;
            clone.SetIcon(this.Icon);
            return clone;
        }
    }
} 