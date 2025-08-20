using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    // Interface for entities that can be paused when inventory is open
    public interface IPausable
    {
        // Whether the entity should be paused
        bool IsPaused { get; set; }
        
        // Update method that respects pause state
        void Update(GameTime gameTime, bool isPaused = false);
    }
}
