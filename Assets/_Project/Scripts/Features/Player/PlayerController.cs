using UnityEngine;
using Core;
using Core.Input;
using Features.Player.Data;
using Features.Player.States;

namespace Features.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerConfigSO _config;
        public PlayerConfigSO Config => _config;

        public CharacterController Controller { get; private set; }
        public Animator Animator { get; private set; }
        public IInputService InputService { get; private set; }
        public PlayerStamina Stamina { get; private set; }
        public PlayerHealth Health { get; private set; }
        public Transform MainCameraTransform { get; private set; }

        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerAirState AirState { get; private set; }
        public PlayerStunnedState StunnedState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }

        public PlayerCombat Combat { get; private set; }
        public float AttackCooldownTimer;

        public Vector3 Velocity; 
        public float SpeedMultiplier { get; private set; } = 1f;
        
        // Rotation State
        public float RotationVelocity; // Used by SmoothDamp
        private float _targetRotation;

        private void Awake()
        {
            Controller = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();
            Stamina = GetComponent<PlayerStamina>();
            Health = GetComponent<PlayerHealth>();
            Combat = gameObject.AddComponent<PlayerCombat>();
            Combat.Initialize(this);
            
            // Cache Camera to avoid FindTag overhead
            if (UnityEngine.Camera.main != null)
                MainCameraTransform = UnityEngine.Camera.main.transform;
            else
                Debug.LogError("Main Camera not found! Player movement depends on camera view.");

            StateMachine = new PlayerStateMachine();
            IdleState = new PlayerIdleState(this, StateMachine);
            MoveState = new PlayerMoveState(this, StateMachine);
            JumpState = new PlayerJumpState(this, StateMachine);
            AirState = new PlayerAirState(this, StateMachine);
            StunnedState = new PlayerStunnedState(this, StateMachine);
            AttackState = new PlayerAttackState(this, StateMachine);

            if (_stunVisuals != null)
            {
                if (_stunVisuals.GetComponent<Core.Utils.BillBoard>() == null)
                {
                    _stunVisuals.AddComponent<Core.Utils.BillBoard>();
                }
            }
        }

        [Header("Visuals")]
        [SerializeField] private GameObject _stunVisuals;

        public void SetStunVisuals(bool active)
        {
            if (_stunVisuals != null) _stunVisuals.SetActive(active);
        }

        public GameObject StunVisuals => _stunVisuals;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out IInputService inputService))
            {
                Debug.LogError("InputService not found!");
                return;
            }
            InputService = inputService;

            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            if (AttackCooldownTimer > 0)
            {
                AttackCooldownTimer -= Time.deltaTime;
            }

            StateMachine.CurrentState.LogicUpdate();
            
            ApplyGravity();
            
            // Final Move Execution
            Controller.Move(Velocity * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (Controller.isGrounded && Velocity.y < 0)
            {
                Velocity.y = -2f; // Stick to ground
            }
            Velocity.y += _config.Gravity * Time.deltaTime;
        }

        public bool IsRotationSlowed { get; set; }

        public void RotateTowardsMoveDirection(Vector3 direction)
        {
            if (direction.magnitude < 0.01f) return;

            _targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + MainCameraTransform.eulerAngles.y;
            ApplyRotationWithSlow(_targetRotation);
        }

        public void RotateTowardsCamera()
        {
            if (MainCameraTransform == null) return;

            _targetRotation = MainCameraTransform.eulerAngles.y;
            ApplyRotationWithSlow(_targetRotation);
        }

        private void ApplyRotationWithSlow(float targetRotation)
        {
            float smoothTime = IsRotationSlowed ? _config.RotationSmoothTime * 1.5f : _config.RotationSmoothTime;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref RotationVelocity, smoothTime);
            
            if (IsRotationSlowed)
            {
                RotationVelocity *= 0.8f;
            }
            
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        public float GetMovementMultiplierWithSlow(float baseMultiplier)
        {
            return IsRotationSlowed ? baseMultiplier * 0.5f : baseMultiplier;
        }

        // Animation event callback for legacy SensitivitySlow naming
        public void SensitivitySlow(float slowDuration)
        {
            IsRotationSlowed = true;
        }

        // Animation event callback to restore normal rotation
        public void SensitivityNormal()
        {
            IsRotationSlowed = false;
        }

        public Vector3 GetMoveDirection(Vector2 input)
        {
            if (MainCameraTransform == null) return Vector3.zero;

            // Normalize input direction
            Vector3 inputDir = new Vector3(input.x, 0.0f, input.y).normalized;

            // Rotate input direction by camera rotation
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + MainCameraTransform.eulerAngles.y;
            return Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        }

        public bool CheckGround() => Controller.isGrounded;

        public void Stun() => StateMachine.ChangeState(StunnedState);

        public void TakeDamage(float amount, Vector3 knockbackDir)
        {
            if (StateMachine.CurrentState == StunnedState) return;

            if (Health != null)
            {
                Health.TakeDamage(amount);
                if (Health.CurrentHealth <= 0)
                {
                    // PlayerHealth handles death (e.g. Destroy or Event)
                    return;
                }
            }

            StunnedState.SetKnockback(knockbackDir);
            StateMachine.ChangeState(StunnedState);
        }

         // Animation Events for Combat
         public void EnableDamage() => Combat.EnableDamage();
         public void DisableDamage() => Combat.DisableDamage();
    }
}
