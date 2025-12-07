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
        [Header("Configuration")]
        [SerializeField] private PlayerConfigSO _config;

        [Header("Visuals")]
        [SerializeField] private GameObject _stunVisuals;

        public PlayerConfigSO Config => _config;
        public GameObject StunVisuals => _stunVisuals;

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
        public float RotationVelocity;
        public bool IsRotationSlowed { get; set; }

        private float _targetRotation;

        private void Awake()
        {
            CacheComponents();
            SetupCombat();
            CacheMainCamera();
            InitializeStateMachine();
            SetupStunVisuals();
        }

        private void CacheComponents()
        {
            Controller = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();
            Stamina = GetComponent<PlayerStamina>();
            Health = GetComponent<PlayerHealth>();
        }

        private void SetupCombat()
        {
            Combat = gameObject.AddComponent<PlayerCombat>();
            Combat.Initialize(this);
        }

        private void CacheMainCamera()
        {
            if (UnityEngine.Camera.main != null)
            {
                MainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("[PlayerController] Main Camera not found! Player movement depends on camera view.");
            }
        }

        private void InitializeStateMachine()
        {
            StateMachine = new PlayerStateMachine();
            IdleState = new PlayerIdleState(this, StateMachine);
            MoveState = new PlayerMoveState(this, StateMachine);
            JumpState = new PlayerJumpState(this, StateMachine);
            AirState = new PlayerAirState(this, StateMachine);
            StunnedState = new PlayerStunnedState(this, StateMachine);
            AttackState = new PlayerAttackState(this, StateMachine);
        }

        private void SetupStunVisuals()
        {
            if (_stunVisuals != null && _stunVisuals.GetComponent<Core.Utils.BillBoard>() == null)
            {
                _stunVisuals.AddComponent<Core.Utils.BillBoard>();
            }
        }

        public void SetStunVisuals(bool active)
        {
            if (_stunVisuals != null) _stunVisuals.SetActive(active);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out IInputService inputService))
            {
                Debug.LogError("[PlayerController] InputService not found!");
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

            StateMachine.CurrentState?.LogicUpdate();
            ApplyGravity();
            Controller.Move(Velocity * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (Controller.isGrounded && Velocity.y < 0)
            {
                Velocity.y = _config.GroundStickForce;
            }
            Velocity.y += _config.Gravity * Time.deltaTime;
        }

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
            float smoothTime = IsRotationSlowed 
                ? _config.RotationSmoothTime * _config.SlowedRotationMultiplier 
                : _config.RotationSmoothTime;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref RotationVelocity, smoothTime);

            if (IsRotationSlowed)
            {
                RotationVelocity *= _config.SlowedVelocityDamping;
            }

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        public float GetMovementMultiplierWithSlow(float baseMultiplier)
        {
            return IsRotationSlowed ? baseMultiplier * _config.SlowedMoveMultiplier : baseMultiplier;
        }

        public void SensitivitySlow(float slowDuration)
        {
            IsRotationSlowed = true;
        }

        public void SensitivityNormal()
        {
            IsRotationSlowed = false;
        }

        public Vector3 GetMoveDirection(Vector2 input)
        {
            if (MainCameraTransform == null) return Vector3.zero;

            Vector3 inputDir = new Vector3(input.x, 0.0f, input.y).normalized;
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
                    return;
                }
            }

            StunnedState.SetKnockback(knockbackDir);
            StateMachine.ChangeState(StunnedState);
        }

        public void EnableDamage() => Combat?.EnableDamage();
        public void DisableDamage() => Combat?.DisableDamage();
    }
}
