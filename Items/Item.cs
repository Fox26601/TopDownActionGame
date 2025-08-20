using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Items
{
    public abstract class Item
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public Texture2D Icon { get; protected set; }
        public bool IsStackable { get; protected set; }
        public int MaxStackSize { get; protected set; } = 1;
        public int Quantity { get; set; } = 1;
        public bool IsUsable { get; protected set; } = false;
        public float ItemScale { get; protected set; }
        public float InteractionRadius { get; protected set; }
        public int SpriteWidth { get; protected set; }
        public int SpriteHeight { get; protected set; }

        protected Item(string name, string description, bool isStackable = false, int maxStackSize = 1)
        {
            Name = name;
            Description = description;
            IsStackable = isStackable;
            MaxStackSize = maxStackSize;
            Quantity = 1;
            ItemScale = GameConstants.SpriteScale.ITEM_SCALE; // Default scale
            InteractionRadius = GameConstants.Interaction.ITEM_DEFAULT_INTERACTION_RADIUS; // Default interaction radius
            // SpriteWidth and SpriteHeight will be set when icon is loaded
        }

        public virtual void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            // Override in derived classes to load specific icons
        }

        public virtual bool Use(Player player)
        {
            // Override in derived classes to implement usage logic
            return false;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE)
        {
            System.Diagnostics.Debug.WriteLine($"Item.Draw called for {Name}. Icon: {Icon != null}, Position: {position}, Scale: {scale}");
            if (Icon != null)
            {
                spriteBatch.Draw(Icon, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, GameConstants.LayerDepth.DEFAULT_DEPTH);
                System.Diagnostics.Debug.WriteLine($"Item.Draw completed for {Name}. Icon drawn successfully.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Item.Draw failed for {Name}. Icon is null!");
            }
        }

        /// <summary>
        /// Set the icon texture for this item
        /// </summary>
        public void SetIcon(Texture2D icon)
        {
            System.Diagnostics.Debug.WriteLine($"Item.SetIcon called for {Name}. Icon: {icon != null}, Icon size: {icon?.Width}x{icon?.Height}");
            Icon = icon;
            SpriteWidth = icon.Width; // Set width from icon
            SpriteHeight = icon.Height; // Set height from icon
            System.Diagnostics.Debug.WriteLine($"Item.SetIcon completed for {Name}. Icon set: {Icon != null}, SpriteWidth: {SpriteWidth}, SpriteHeight: {SpriteHeight}");
        }

        /// <summary>
        /// Apply a scale multiplier to the item's visual scale.
        /// </summary>
        /// <param name="multiplier">The multiplier to apply.</param>
        public void ApplyScaleMultiplier(float multiplier)
        {
            ItemScale *= multiplier;
        }

        /// <summary>
        /// Create a shallow copy of this item with same properties
        /// </summary>
        public virtual Item Clone()
        {
            // This is a base implementation that should be overridden in derived classes
            // For now, return null to indicate that cloning is not supported
            return null;
        }
    }
} 