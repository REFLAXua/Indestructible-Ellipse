using UnityEngine;
using UnityEngine.AI;
using Features.Enemy.Data;
using Features.Enemy.States;
using Features.Enemy.States.Concrete;
using Features.Enemy.Systems;

namespace Features.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyConfigSO _config;
        
        [Header("Visuals")]
        [SerializeField] private GameObject _stunVisuals;

        public EnemyConfigSO Config => _config;
        public GameObject StunVisuals => _stunVisuals;

        public EnemyBlackboard Blackboard { get; private set; }
        public EnemyMover Mover { get; private set; }
        public EnemyPerception Perception { get; private set; }
        public EnemyStateMachine StateMachine { get; private set; }

        public EnemyIdleState IdleState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyStunnedState StunnedState { get; private set; }
        public EnemyDeadState DeadState { get; private set; }

        public Animator Animator { get; private set; }
        public EnemyHealth Health { get; private set; }

        private EnemyAnimationHandler _animationHandler;

        private void Awake()
        {
            CacheComponents();
            InitializeBlackboardAndSystems();
            InitializeStateMachine();
            SubscribeToHealthEvents();
            SetupStunVisuals();
        }

        private void CacheComponents()
        {
            Animator = GetComponent<Animator>();
            Health = GetComponent<EnemyHealth>();
        }

        private void InitializeBlackboardAndSystems()
        {
            Blackboard = new EnemyBlackboard(_config);

            Mover = gameObject.AddComponent<EnemyMover>();
            Perception = gameObject.AddComponent<EnemyPerception>();

            Mover.Initialize(Blackboard);
            Perception.Initialize(Blackboard);

            _animationHandler = new EnemyAnimationHandler(Animator);
        }

        private void InitializeStateMachine()
        {
            StateMachine = new EnemyStateMachine();

            IdleState = new EnemyIdleState(this, StateMachine);
            PatrolState = new EnemyPatrolState(this, StateMachine);
            ChaseState = new EnemyChaseState(this, StateMachine);
            AttackState = new EnemyAttackState(this, StateMachine);
            StunnedState = new EnemyStunnedState(this, StateMachine);
            DeadState = new EnemyDeadState(this, StateMachine);
        }

        private void SubscribeToHealthEvents()
        {
            if (Health != null)
            {
                Health.OnHit += OnHit;
                Health.Initialize(_config.MaxHealth, _config.HitFlashDuration);
            }
        }

        private void SetupStunVisuals()
        {
            if (_stunVisuals != null && _stunVisuals.GetComponent<Core.Utils.BillBoard>() == null)
            {
                _stunVisuals.AddComponent<Core.Utils.BillBoard>();
            }
        }

        private void Start()
        {
            EnsureOnNavMesh();
            StateMachine.Initialize(IdleState);
        }

        private void EnsureOnNavMesh()
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null && !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }

        private void Update()
        {
            Perception.OnUpdate();
            Mover.OnUpdate();
            StateMachine.CurrentState?.LogicUpdate();
        }

        private void OnDestroy()
        {
            if (Health != null)
            {
                Health.OnHit -= OnHit;
            }
        }

        public void OnHit(Vector3? knockbackDir)
        {
            if (StateMachine.CurrentState == DeadState) return;

            if (knockbackDir.HasValue)
            {
                Mover.ApplyKnockback(knockbackDir.Value, _config.KnockbackForce);
            }

            StateMachine.ChangeState(StunnedState);
        }

        public void TakeDamage(float damage, Vector3 knockbackDir)
        {
            Health?.TakeDamage(damage, knockbackDir);
        }

        public void OnDeath()
        {
            StateMachine.ChangeState(DeadState);
        }

        public void SetStunVisuals(bool active)
        {
            if (_stunVisuals != null) _stunVisuals.SetActive(active);
        }

        public void SetWalking(bool isWalking) => _animationHandler.SetWalking(isWalking);
        public void TriggerAttackAnimation() => _animationHandler.TriggerAttack();
        public void TriggerDieAnimation() => _animationHandler.TriggerDie();

        public void OnAttackHit()
        {
            if (StateMachine.CurrentState == DeadState) return;
            AttackState?.TriggerDamage();
        }

        public void OnAttackEnd()
        {
            if (StateMachine.CurrentState == DeadState) return;
            AttackState?.FinishAttack();
        }

        public void DealDamage() => OnAttackHit();
        public void ResetDamage() => OnAttackEnd();
    }

    public sealed class EnemyAnimationHandler
    {
        private readonly Animator _animator;
        private readonly int _walkParamId;
        private readonly int _attackParamId;
        private readonly int _dieParamId;

        public EnemyAnimationHandler(Animator animator)
        {
            _animator = animator;

            if (_animator != null)
            {
                _walkParamId = Animator.StringToHash("IsWalking");
                _attackParamId = Animator.StringToHash("Attack");
                _dieParamId = Animator.StringToHash("isDead");
            }
        }

        public void SetWalking(bool isWalking)
        {
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetBool(_walkParamId, isWalking);
            }
        }

        public void TriggerAttack()
        {
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetTrigger(_attackParamId);
            }
        }

        public void TriggerDie()
        {
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetTrigger(_dieParamId);
            }
        }
    }
}
