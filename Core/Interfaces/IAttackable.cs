using System;
using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    public interface IAttackable
    {
        Vector2 WorldPosition { get; }
        bool IsAlive { get; }
        int MaxHealth { get; }
        int CurrentHealth { get; }
        
        void TakeDamage(int damage, Vector2? attackerPosition = null);
        void Heal(int amount);
        
        // Events
        event Action<IAttackable, int> OnDamage; // Target, damage
        event Action<IAttackable, int> OnHeal; // Target, amount
        event Action<IAttackable> OnDeath; // Target
    }
} 