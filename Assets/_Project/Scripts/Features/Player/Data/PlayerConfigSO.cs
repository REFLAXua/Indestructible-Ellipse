using UnityEngine;

namespace Features.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "LetsRoll/Player/PlayerConfig")]
    public class PlayerConfigSO : ScriptableObject
    {
        [Header("Movement")]
        public float BaseSpeed = 5f;
        public float SprintSpeed = 7f;
        public float RotationSmoothTime = 0.1f;
        public float Gravity = -9.81f;
        public float AirControlMultiplier = 5f;

        [Header("Jump")]
        public float JumpForce = 6f;
        public float JumpTime = 0.2f;
        public float GroundStickForce = -2f;

        [Header("Stamina")]
        public float MaxStamina = 100f;
        public float StaminaRegenRate = 3f;
        public float SprintStaminaCost = 10f;
        public float JumpStaminaCost = 15f;
        public float ExhaustionRecoveryThreshold = 15f;

        [Header("Combat")]
        public float AttackDamage = 1f;
        public float AttackCooldown = 1.5f;
        public float AttackRange = 1.5f;
        public float AttackRadius = 1.0f;
        public float StunDuration = 1f;
        public float KnockbackForce = 5f;
        public float AttackMoveMultiplier = 0.4f;
        public float AttackDuration = 0.5f;
        public float SlowedRotationMultiplier = 1.5f;
        public float SlowedVelocityDamping = 0.8f;
        public float SlowedMoveMultiplier = 0.5f;

        [Header("Visual Feedback")]
        public float StunRotationSpeed = 200f;
        public float KnockbackDragSpeed = 5f;
    }
}
