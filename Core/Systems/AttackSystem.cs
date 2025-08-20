using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;

namespace IsometricActionGame
{
    public class AttackSystem
    {
        private List<IAttackable> _attackableEntities;
        
        public AttackSystem()
        {
            _attackableEntities = new List<IAttackable>();
        }
        
        public void RegisterEntity(IAttackable entity)
        {
            if (!_attackableEntities.Contains(entity))
            {
                _attackableEntities.Add(entity);
            }
        }
        
        public void UnregisterEntity(IAttackable entity)
        {
            _attackableEntities.Remove(entity);
        }
        
        public List<IAttackable> ExecuteAttack(Attack attack)
        {
            var hitTargets = new List<IAttackable>();
            
            foreach (var target in _attackableEntities)
            {
                if (!target.IsAlive) 
                {
                    continue;
                }
                
                if (IsInAttackCone(attack.AttackerPosition, attack.AttackDirection, target.WorldPosition, attack.AttackRange, attack.ConeAngle))
                {
                    target.TakeDamage(attack.Damage, attack.AttackerPosition);
                    hitTargets.Add(target);
                }
            }
            
            return hitTargets;
        }
        
        private bool IsInAttackCone(Vector2 attackerPos, Vector2 attackDirection, Vector2 targetPos, float attackRange, float coneAngle)
        {
            // Calculate distance using isometric projection for accurate range
            float distance = GridHelper.CalculateDistance(attackerPos, targetPos);
            
            // Find the target entity to get its hitbox radius
            float targetHitboxRadius = 0f;
            foreach (var entity in _attackableEntities)
            {
                if (entity.WorldPosition == targetPos)
                {
                    // Cast to IEntity to access HitboxRadius
                    if (entity is IEntity entityWithHitbox)
                    {
                        // Convert hitbox radius from pixels to world units
                        // Use average of X and Y tile scales for consistent conversion
                        float tileScale = GridHelper.TILE_SIZE;
                        targetHitboxRadius = entityWithHitbox.HitboxRadius / tileScale;
                    }
                    break;
                }
            }
            
            // Account for target's hitbox radius - attack can hit the edges of the sprite
            float effectiveAttackRange = attackRange + targetHitboxRadius;
            
            if (distance > effectiveAttackRange) 
            {
                return false;
            }
            
            // Calculate direction to target using simple coordinates for angle calculation
            Vector2 directionToTarget = Vector2.Normalize(targetPos - attackerPos);
            
            // Calculate angle between attack direction and direction to target using simple vectors
            float dotProduct = Vector2.Dot(attackDirection, directionToTarget);
            float angleRadians = MathF.Acos(MathHelper.Clamp(dotProduct, -1f, 1f));
            float angleDegrees = angleRadians * 180f / MathF.PI;
            
            // Check if within cone angle
            bool inCone = angleDegrees <= coneAngle / 2f;
            
            return inCone;
        }
        
        public void ClearEntities()
        {
            _attackableEntities.Clear();
        }
        

    }
} 