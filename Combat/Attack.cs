using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    public class Attack
    {
        public Vector2 AttackerPosition { get; set; }
        public Vector2 AttackDirection { get; set; }
        public float AttackRange { get; set; }
        public float ConeAngle { get; set; }
        public int Damage { get; set; }
        public string AttackType { get; set; }
        
        public Attack(Vector2 attackerPosition, Vector2 attackDirection, float attackRange, float coneAngle, int damage, string attackType = "melee")
        {
            AttackerPosition = attackerPosition;
            AttackDirection = attackDirection;
            AttackRange = attackRange;
            ConeAngle = coneAngle;
            Damage = damage;
            AttackType = attackType;
        }
    }
} 