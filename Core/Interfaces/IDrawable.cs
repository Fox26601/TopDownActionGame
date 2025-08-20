using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsometricActionGame
{
    // Interface for objects that can be drawn
    public interface IDrawable
    {
        // Draw the object
        void Draw(SpriteBatch spriteBatch);
    }
}
