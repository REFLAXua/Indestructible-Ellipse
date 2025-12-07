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
        public float AttackWindupTime = 0.5f;
        public float AttackRangeTolerance = 0.5f;

        [Header("Health")]
        public float MaxHealth = 50f;
        public float StunDuration = 1f;
        public float KnockbackResistance = 0f;
        public float KnockbackForce = 5f;

        [Header("Death")]
        public float DeathDuration = 7f;
        public float SinkDelay = 6.5f;
        public float SinkSpeed = 2f;
        public float FinalSinkSpeed = 1.2f;
        public float DeathRotationSpeed = 7f;
        public float FinalSinkPhaseTime = 2f;

        [Header("Behavior")]
        public float IdleToPatrolDelay = 2f;
        public float PatrolRadius = 10f;
        public float StunRotationSpeed = 200f;
        public float DetectionLossMultiplier = 1.2f;

        [Header("Visual Feedback")]
        public float HitFlashDuration = 0.15f;
    }
}
