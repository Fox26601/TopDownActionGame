using Microsoft.Xna.Framework;
using IsometricActionGame.Events.EventData;

namespace IsometricActionGame.Events.EventData
{
    /// <summary>
    /// Event data for player attack events
    /// </summary>
    public class PlayerAttackEventData : BaseEventData
    {
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        
        public PlayerAttackEventData(Vector2 position, Vector2 direction) : base("Player")
        {
            Position = position;
            Direction = direction;
        }
    }
    
    /// <summary>
    /// Event data for player damage events
    /// </summary>
    public class PlayerDamagedEventData : BaseEventData
    {
        public int Damage { get; }
        public Vector2 Position { get; }
        public string AttackerType { get; }
        
        public PlayerDamagedEventData(int damage, Vector2 position, string attackerType = null) : base("Player")
        {
            Damage = damage;
            Position = position;
            AttackerType = attackerType;
        }
    }
    
    /// <summary>
    /// Event data for player heal events
    /// </summary>
    public class PlayerHealedEventData : BaseEventData
    {
        public int Amount { get; }
        public string HealSource { get; }
        
        public PlayerHealedEventData(int amount, string healSource = null) : base("Player")
        {
            Amount = amount;
            HealSource = healSource;
        }
    }
    
    /// <summary>
    /// Event data for player death events
    /// </summary>
    public class PlayerDeathEventData : BaseEventData
    {
        public Vector2 DeathPosition { get; }
        public string DeathCause { get; }
        
        public PlayerDeathEventData(Vector2 deathPosition, string deathCause = null) : base("Player")
        {
            DeathPosition = deathPosition;
            DeathCause = deathCause;
        }
    }
    
    /// <summary>
    /// Event data for player movement events
    /// </summary>
    public class PlayerMovedEventData : BaseEventData
    {
        public Vector2 OldPosition { get; }
        public Vector2 NewPosition { get; }
        public Vector2 Direction { get; }
        
        public PlayerMovedEventData(Vector2 oldPosition, Vector2 newPosition, Vector2 direction) : base("Player")
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
            Direction = direction;
        }
    }
}


