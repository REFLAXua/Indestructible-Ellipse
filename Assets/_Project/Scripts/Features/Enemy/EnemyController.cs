using UnityEngine;
using UnityEngine.AI;
using Features.Enemy.Data;
using Features.Enemy.States;
using Features.Enemy.States.Concrete;
using Features.Enemy.Systems;
using System.Collections.Generic;

namespace Features.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyConfigSO _config;
        public EnemyConfigSO Config => _config;

        // Systems
        public EnemyBlackboard Blackboard { get; private set; }
        public EnemyMover Mover { get; private set; }
        public EnemyPerception Perception { get; private set; }
        public EnemyStateMachine StateMachine { get; private set; }

        // States
        public EnemyIdleState IdleState { get; private set; }
        public EnemyPatrolState PatrolState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyAttackState AttackState { get; private set; }
        public EnemyStunnedState StunnedState { get; private set; }
        public EnemyDeadState DeadState { get; private set; }

        // Components
        public Animator Animator { get; private set; }
        public EnemyHealth Health { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject _stunVisuals;

        // Animation Parameters
        private int _walkParamId;
        private int _attackParamId;
        // private int _hitParamId;
        private int _dieParamId;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            Health = GetComponent<EnemyHealth>();
            
            InitializeAnimatorParameters();

            // 1. Create Blackboard
            Blackboard = new EnemyBlackboard(_config);

            // 2. Initialize Systems
            Mover = gameObject.AddComponent<EnemyMover>();
            Perception = gameObject.AddComponent<EnemyPerception>();
            
            Mover.Initialize(Blackboard);
            Perception.Initialize(Blackboard);

            // 3. Initialize State Machine
            InitializeStateMachine();

            // 4. Subscribe to Health Events
            if (Health != null)
            {
                Health.OnHit += OnHit;
                Health.Initialize(_config.MaxHealth);
            }

            // 5. Ensure Stun Visuals have Billboard
            if (_stunVisuals != null)
            {
                if (_stunVisuals.GetComponent<Core.Utils.BillBoard>() == null)
                {
                    _stunVisuals.AddComponent<Core.Utils.BillBoard>();
                }
            }
        }

        private void Start()
        {
            // Initial Placement Logic (if needed)
            if (!GetComponent<NavMeshAgent>().isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                {
                    GetComponent<NavMeshAgent>().Warp(hit.position);
                }
            }

            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            // Update Systems
            Perception.OnUpdate();
            Mover.OnUpdate();

            // Update State
            StateMachine.CurrentState.LogicUpdate();
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

        // ========================================================================================
        // Public API (Called by Health, Animation Events, etc.)
        // ========================================================================================

        public void OnHit(Vector3? knockbackDir)
        {
            if (StateMachine.CurrentState == DeadState) return;

            if (knockbackDir.HasValue)
            {
                Mover.ApplyKnockback(knockbackDir.Value, 5f);
            }
            
            StateMachine.ChangeState(StunnedState);
        }

        public void TakeDamage(float damage, Vector3 knockbackDir)
        {
            if (Health != null)
            {
                Health.TakeDamage(damage, knockbackDir);
            }
        }

        public void OnDeath()
        {
            StateMachine.ChangeState(DeadState);
        }

        public void SetStunVisuals(bool active)
        {
            if (_stunVisuals != null) _stunVisuals.SetActive(active);
        }

        public GameObject StunVisuals => _stunVisuals;

        // ========================================================================================
        // Animation Handling
        // ========================================================================================

        private void InitializeAnimatorParameters()
        {
            _walkParamId = Animator.StringToHash("IsWalking");
            _attackParamId = Animator.StringToHash("Attack");
            // _hitParamId = Animator.StringToHash("GetHit"); // Removed
            _dieParamId = Animator.StringToHash("isDead");
        }

        public void SetWalking(bool isWalking) => Animator.SetBool(_walkParamId, isWalking);
        public void TriggerAttackAnimation() => Animator.SetTrigger(_attackParamId);
        // public void TriggerHitAnimation() => Animator.SetTrigger(_hitParamId); // Removed
        public void TriggerDieAnimation() => Animator.SetTrigger(_dieParamId);

        // Animation Events
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

        // Legacy Support
        public void DealDamage() => OnAttackHit();
        public void ResetDamage() => OnAttackEnd();
    }
}
