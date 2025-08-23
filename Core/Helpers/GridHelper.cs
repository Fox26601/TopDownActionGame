using Microsoft.Xna.Framework;
using System;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Core.Helpers
{
    /// <summary>
    /// Helper class for standard rectangular grid operations
    /// Replaces isometric functionality with simple 2D grid
    /// </summary>
    public static class GridHelper
    {
        // Grid constants
        public const float TILE_SIZE = 64f;
        public const float TILE_X_OFFSET = 0f;
        public const float TILE_Y_OFFSET = 0f;

        /// <summary>
        /// Convert world coordinates to screen coordinates
        /// Simple 1:1 mapping for rectangular grid
        /// </summary>
        public static Vector2 WorldToScreen(Vector2 worldPosition)
        {
            float screenX = worldPosition.X * TILE_SIZE + TILE_X_OFFSET;
            float screenY = worldPosition.Y * TILE_SIZE + TILE_Y_OFFSET;
            return new Vector2(screenX, screenY);
        }

        /// <summary>
        /// Convert screen coordinates to world coordinates
        /// Simple 1:1 mapping for rectangular grid
        /// </summary>
        public static Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            float worldX = (screenPosition.X - TILE_X_OFFSET) / TILE_SIZE;
            float worldY = (screenPosition.Y - TILE_Y_OFFSET) / TILE_SIZE;
            return new Vector2(worldX, worldY);
        }

        /// <summary>
        /// Calculate layer depth for proper rendering order
        /// Y coordinate determines depth (higher Y = further from camera = rendered behind)
        /// </summary>
        public static float CalculateLayerDepth(Vector2 worldPosition)
        {
            // Normalize Y position to 0.0-1.0 range based on map size
            // Higher Y values get LOWER depth (rendered behind) - corrected for proper isometric rendering
            float normalizedY = worldPosition.Y / GameMap.Height;
            // Invert the Y calculation: higher Y = lower depth (behind)
            float invertedY = 1.0f - normalizedY;
            return MathHelper.Clamp(GameConstants.LayerDepth.DEFAULT_DEPTH * 0.2f + invertedY * GameConstants.LayerDepth.DEFAULT_DEPTH * 0.8f, GameConstants.LayerDepth.DEFAULT_DEPTH * 0.2f, GameConstants.LayerDepth.DEFAULT_DEPTH * 0.9f);
        }

        /// <summary>
        /// Calculate distance between two points in world space
        /// </summary>
        public static float CalculateDistance(Vector2 point1, Vector2 point2)
        {
            return Vector2.Distance(point1, point2);
        }

        /// <summary>
        /// Get direction from input flags
        /// Standard WASD movement
        /// </summary>
        public static Vector2 GetMovementDirection(bool isUp, bool isDown, bool isLeft, bool isRight)
        {
            Vector2 direction = Vector2.Zero;
            
            if (isUp && !isDown) 
                direction.Y -= 1;
            if (isDown && !isUp) 
                direction.Y += 1;
            if (isLeft && !isRight) 
                direction.X -= 1;
            if (isRight && !isLeft) 
                direction.X += 1;
            
            return direction;
        }

        /// <summary>
        /// Get movement speed multiplier (always 1.0 for rectangular grid)
        /// </summary>
        public static float GetMovementSpeedMultiplier()
        {
            return GameConstants.Graphics.DEFAULT_SPRITE_SCALE;
        }

        /// <summary>
        /// Calculate NPC layer depth relative to player
        /// </summary>
        public static float CalculateNPCLayerDepth(Vector2 npcPosition, Vector2 playerPosition, float baseNPCDepth = GameConstants.Graphics.NPC_LAYER_DEPTH, float depthOffset = 0.01f)
        {
            return CalculateLayerDepth(npcPosition);
        }
    }
}
