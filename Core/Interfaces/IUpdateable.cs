using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    // Interface for objects that need to be updated each frame
    public interface IUpdateable
    {
        // Update the object with the current game time
        void Update(GameTime gameTime);
    }
}
