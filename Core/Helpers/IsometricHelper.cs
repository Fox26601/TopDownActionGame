using Microsoft.Xna.Framework;
using System;
using IsometricActionGame.Core.Data;

namespace IsometricActionGame.Core.Helpers
{
    public static class IsometricHelper
    {
        // Isometric projection constants
        public const float TILE_X_SCALE = 64f;
        public const float TILE_Y_SCALE = 32f;
        public const float TILE_X_OFFSET = 0f;
        public const float TILE_Y_OFFSET = 0f;

        // Convert world coordinates to screen coordinates
        public static Vector2 WorldToScreen(Vector2 worldPosition)
        {
            float screenX = (worldPosition.X - worldPosition.Y) * TILE_X_SCALE / 2f + TILE_X_OFFSET;
            float screenY = (worldPosition.X + worldPosition.Y) * TILE_Y_SCALE / 2f + TILE_Y_OFFSET;
            return new Vector2(screenX, screenY);
        }

        // Convert screen coordinates to world coordinates
        public static Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            float worldX = (screenPosition.X - TILE_X_OFFSET) / TILE_X_SCALE + (screenPosition.Y - TILE_Y_OFFSET) / TILE_Y_SCALE;
            float worldY = (screenPosition.Y - TILE_Y_OFFSET) / TILE_Y_SCALE - (screenPosition.X - TILE_X_OFFSET) / TILE_X_SCALE;
            return new Vector2(worldX, worldY);
        }

        // Convert cartesian direction to isometric direction
        public static Vector2 ConvertDirection(Vector2 cartesianDirection)
        {
            return new Vector2(
                cartesianDirection.X - cartesianDirection.Y,
                (cartesianDirection.X + cartesianDirection.Y) / 2f
            );
        }

        // Calculate distance between two points in isometric space
        public static float CalculateDistance(Vector2 point1, Vector2 point2)
        {
            return Vector2.Distance(point1, point2);
        }

        // Calculate isometric distance (for combat and interactions)
        public static float CalculateIsometricDistance(Vector2 point1, Vector2 point2)
        {
            Vector2 screen1 = WorldToScreen(point1);
            Vector2 screen2 = WorldToScreen(point2);
            return Vector2.Distance(screen1, screen2);
        }

        // Calculate layer depth for proper rendering order
        public static float CalculateLayerDepth(Vector2 worldPosition)
        {
            return GameConstants.LayerDepth.DEFAULT_DEPTH + (worldPosition.X + worldPosition.Y) * 0.001f;
        }

        // Get isometric direction from input
        public static Vector2 GetIsometricDirection(bool isUp, bool isDown, bool isLeft, bool isRight)
        {
            Vector2 direction = Vector2.Zero;
            
            // Correct isometric mapping:
            // W (Up): (-1, -1) in world coordinates
            // S (Down): (1, 1) in world coordinates  
            // A (Left): (-1, 1) in world coordinates
            // D (Right): (1, -1) in world coordinates
            
            if (isUp && !isDown) 
            {
                direction.X -= 1;
                direction.Y -= 1;
            }
            if (isDown && !isUp) 
            {
                direction.X += 1;
                direction.Y += 1;
            }
            if (isLeft && !isRight) 
            {
                direction.X -= 1;
                direction.Y += 1;
            }
            if (isRight && !isLeft) 
            {
                direction.X += 1;
                direction.Y -= 1;
            }
            
            return direction;
        }

        // Calculate angle between two vectors
        public static float CalculateIsometricAngle(Vector2 vector1, Vector2 vector2)
        {
            float dotProduct = Vector2.Dot(vector1, vector2);
            float angleRadians = (float)Math.Acos(MathHelper.Clamp(dotProduct, -1f, 1f));
            return angleRadians * 180f / (float)Math.PI;
        }

        // Get isometric axes
        public static (Vector2 XAxis, Vector2 YAxis) GetIsometricAxes()
        {
            Vector2 xAxis = ConvertDirection(new Vector2(1, 0));
            Vector2 yAxis = ConvertDirection(new Vector2(0, 1));
            return (xAxis, yAxis);
        }

        // Get vertical movement speed multiplier
        public static float GetVerticalMovementSpeedMultiplier()
        {
            return GameConstants.Graphics.DEFAULT_SPRITE_SCALE / TILE_Y_SCALE;
        }

        // Calculate NPC layer depth
        public static float CalculateNPCLayerDepth(Vector2 npcPosition, Vector2 playerPosition, float baseNPCDepth = GameConstants.Graphics.NPC_LAYER_DEPTH, float depthOffset = 0.01f)
        {
            return CalculateLayerDepth(npcPosition);
        }

        // Print isometric constants (for debugging)
        public static void PrintIsometricConstants()
        {
            var axes = GetIsometricAxes();
            // Debug output removed as per cleanup requirements
        }
    }
}
