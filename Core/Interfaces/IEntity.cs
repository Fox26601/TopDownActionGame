using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsometricActionGame
{
    public interface IEntity
    {
        Vector2 WorldPosition { get; }
        float LayerDepth { get; }
        Vector2 FacingDirection { get; } // Direction the entity is facing for sprite mirroring
        float Scale { get; } // Universal scale for sprite, hitbox, and interaction radius
        float BaseHitboxRadius { get; } // Base hitbox radius in pixels (before scaling)
        float HitboxRadius => BaseHitboxRadius * Scale; // Scaled hitbox radius
        int SpriteWidth { get; } // Sprite width in pixels
        int SpriteHeight { get; } // Sprite height in pixels
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
} 