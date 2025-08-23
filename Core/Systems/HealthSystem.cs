using System;
using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    public class HealthSystem
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        
        // Events
        public event Action<HealthSystem, int> OnDamage; // HealthSystem, damage
        public event Action<HealthSystem, int> OnHeal; // HealthSystem, amount
        public event Action<HealthSystem> OnDeath; // HealthSystem
        public event Action<HealthSystem> OnHealthChanged; // HealthSystem
        
        public HealthSystem(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }
        
        public void TakeDamage(int damage, Vector2? attackerPosition = null)
        {
            if (!IsAlive) return;
            
            int oldHealth = CurrentHealth;
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            int actualDamage = oldHealth - CurrentHealth;
            
            if (actualDamage > 0)
            {
                OnDamage?.Invoke(this, actualDamage);
                OnHealthChanged?.Invoke(this);
                
                if (!IsAlive)
                {
                    OnDeath?.Invoke(this);
                }
            }
        }
        
        public void Heal(int amount)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: Called with amount={amount}, current health={CurrentHealth}/{MaxHealth}");
                
                if (!IsAlive) 
                {
                    System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: Not alive, cannot heal");
                    return;
                }
                
                int oldHealth = CurrentHealth;
                CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
                int actualHeal = CurrentHealth - oldHealth;
                
                System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: oldHealth={oldHealth}, newHealth={CurrentHealth}, actualHeal={actualHeal}");
                
                if (actualHeal > 0)
                {
                    OnHeal?.Invoke(this, actualHeal);
                    OnHealthChanged?.Invoke(this);
                    System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: Events invoked");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: No actual healing occurred");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"HealthSystem.Heal: Stack trace: {ex.StackTrace}");
            }
        }
        
        public void RestoreFullHealth()
        {
            int oldHealth = CurrentHealth;
            CurrentHealth = MaxHealth;
            int actualHeal = CurrentHealth - oldHealth;
            
            if (actualHeal > 0)
            {
                OnHeal?.Invoke(this, actualHeal);
                OnHealthChanged?.Invoke(this);
            }
        }
        
        public float GetHealthPercentage()
        {
            return (float)CurrentHealth / MaxHealth;
        }
        
        /// <summary>
        /// Reset health to maximum
        /// </summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
        }
        
        /// <summary>
        /// Set health to a specific value
        /// </summary>
        public void SetHealth(int health)
        {
            int oldHealth = CurrentHealth;
            CurrentHealth = MathHelper.Clamp(health, 0, MaxHealth);
            
            if (CurrentHealth != oldHealth)
            {
                OnHealthChanged?.Invoke(this);
                
                if (CurrentHealth == 0 && oldHealth > 0)
                {
                    OnDeath?.Invoke(this);
                }
            }
        }
    }
} 