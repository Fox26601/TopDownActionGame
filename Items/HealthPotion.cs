using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Items
{
    public class HealthPotion : Item
    {
        public int HealAmount { get; private set; } = GameConstants.Health.HEALTH_POTION_DEFAULT_HEAL;

        public HealthPotion() : base("Health Potion", $"Restores {GameConstants.Health.HEALTH_POTION_DEFAULT_HEAL} health points", false, GameConstants.Items.DEFAULT_ITEM_QUANTITY)
        {
            IsUsable = true;
            HealAmount = GameConstants.Health.HEALTH_POTION_DEFAULT_HEAL;
        }

        public HealthPotion(int healAmount) : base("Health Potion", $"Restores {healAmount} health points", false, GameConstants.Items.DEFAULT_ITEM_QUANTITY)
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
            if (player.CurrentHealth < player.MaxHealth)
            {
                player.Heal(HealAmount);
                Quantity--;
                return true;
            }
            return false;
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