using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "LetsRoll/Enemy/EnemyConfig")]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("Movement")]
        public float MoveSpeed = 3.5f;
        public float PatrolSpeed = 2f;
        public float ChaseSpeed = 4.5f;
        public float RotationSpeed = 10f;
        public float Acceleration = 8f;

        [Header("Detection")]
        public float DetectionRange = 10f;
        public float ViewDistance = 15f;
        public float ViewAngle = 90f;
        public float DetectionDelay = 0.2f;
        public float MemoryDuration = 5f;

        [Header("Combat")]
        public float AttackRange = 1.5f;
        public float AttackDamage = 10f;
        public float AttackCooldown = 2f;
        public float AttackWindupTime = 0.5f; // Delay before damage

        [Header("Health")]
        public float MaxHealth = 50f;
        public float StunDuration = 1f;
        public float KnockbackResistance = 0f; // 0 to 1

        [Header("Death")]
        public float DeathDuration = 7f;
        public float SinkDelay = 6.5f; // Time before sinking starts
        public float SinkSpeed = 2f;
        public float FinalSinkSpeed = 1.2f;
        public float DeathRotationSpeed = 7f;
    }
}
