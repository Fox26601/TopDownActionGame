using Microsoft.Xna.Framework;
using IsometricActionGame.Events.EventData;

namespace IsometricActionGame.Events.EventData
{
    /// <summary>
    /// Event data for enemy defeat events
    /// </summary>
    public class EnemyDefeatedEventData : BaseEventData
    {
        public string EnemyType { get; }
        public Vector2 Position { get; }
        public string DefeatedBy { get; }
        public int ExperienceGained { get; }
        
        public EnemyDefeatedEventData(string enemyType, Vector2 position, string defeatedBy = null, int experienceGained = 0) : base("Combat")
        {
            EnemyType = enemyType;
            Position = position;
            DefeatedBy = defeatedBy;
            ExperienceGained = experienceGained;
        }
    }
    
    /// <summary>
    /// Event data for attack execution events
    /// </summary>
    public class AttackExecutedEventData : BaseEventData
    {
        public string AttackerType { get; }
        public Vector2 AttackerPosition { get; }
        public Vector2 TargetPosition { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
        
        public AttackExecutedEventData(string attackerType, Vector2 attackerPosition, Vector2 targetPosition, int damage, bool isCritical = false) : base("Combat")
        {
            AttackerType = attackerType;
            AttackerPosition = attackerPosition;
            TargetPosition = targetPosition;
            Damage = damage;
            IsCritical = isCritical;
        }
    }
    
    /// <summary>
    /// Event data for damage dealt events
    /// </summary>
    public class DamageDealtEventData : BaseEventData
    {
        public string AttackerType { get; }
        public string TargetType { get; }
        public Vector2 TargetPosition { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
        
        public DamageDealtEventData(string attackerType, string targetType, Vector2 targetPosition, int damage, bool isCritical = false) : base("Combat")
        {
            AttackerType = attackerType;
            TargetType = targetType;
            TargetPosition = targetPosition;
            Damage = damage;
            IsCritical = isCritical;
        }
    }
    
    /// <summary>
    /// Event data for projectile creation events
    /// </summary>
    public class ProjectileCreatedEventData : BaseEventData
    {
        public string ProjectileType { get; }
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public float Speed { get; }
        
        public ProjectileCreatedEventData(string projectileType, Vector2 position, Vector2 direction, float speed) : base("Combat")
        {
            ProjectileType = projectileType;
            Position = position;
            Direction = direction;
            Speed = speed;
        }
    }
    
    /// <summary>
    /// Event data for projectile destruction events
    /// </summary>
    public class ProjectileDestroyedEventData : BaseEventData
    {
        public string ProjectileType { get; }
        public Vector2 Position { get; }
        public string DestroyReason { get; }
        
        public ProjectileDestroyedEventData(string projectileType, Vector2 position, string destroyReason = null) : base("Combat")
        {
            ProjectileType = projectileType;
            Position = position;
            DestroyReason = destroyReason;
        }
    }
    
}
