using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Items
{
    public class Gold : Item
    {
        public int Amount { get; set; }

        public Gold() : base("Gold", "Shiny gold coins", true, GameConstants.Items.GOLD_MAX_STACK_SIZE)
        {
            Amount = GameConstants.Items.DEFAULT_ITEM_QUANTITY;
            Quantity = GameConstants.Items.DEFAULT_ITEM_QUANTITY;
            IsUsable = false;
        }

        public Gold(int amount) : base("Gold", "Shiny gold coins", true, GameConstants.Items.GOLD_MAX_STACK_SIZE)
        {
            Amount = amount;
            Quantity = amount;
            IsUsable = false;
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                Icon = content.Load<Texture2D>("Gold");
            }
            catch
            {
                // Use debug rectangle if texture not found
                Icon = null;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE)
        {
            if (Icon != null)
            {
                base.Draw(spriteBatch, position, scale);
            }
            else
            {
                // Debug rectangle for gold
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.Yellow });
                spriteBatch.Draw(debugTexture, position, null, Color.Yellow, 0f, Vector2.Zero, new Vector2(GameConstants.Items.DEBUG_SMALL_RECTANGLE_SIZE, GameConstants.Items.DEBUG_SMALL_RECTANGLE_SIZE) * scale, SpriteEffects.None, GameConstants.Graphics.DEFAULT_SPRITE_SCALE);
            }
        }

        public override Item Clone()
        {
            var clone = new Gold(this.Amount);
            clone.Quantity = this.Quantity;
            clone.SetIcon(this.Icon);
            return clone;
        }
    }
} 