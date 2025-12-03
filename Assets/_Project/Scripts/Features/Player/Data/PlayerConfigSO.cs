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

        [Header("Jump")]
        public float JumpForce = 5f;
        public float JumpTime = 0.2f;

        [Header("Stamina")]
        public float MaxStamina = 100f;
        public float StaminaRegenRate = 3f;
        public float SprintStaminaCost = 10f;
        public float JumpStaminaCost = 15f;

        [Header("Combat")]
        public float AttackDamage = 1f;
        public float AttackCooldown = 1.5f;
        public float AttackRange = 1.5f;
        public float AttackRadius = 1.0f;
        public float StunDuration = 1f;
        public float KnockbackForce = 5f;
        public float attackMoveMultiplier = 0.4f;
        public float slowedSmoothTime = 3f;
    }
}
