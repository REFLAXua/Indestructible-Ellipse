using UnityEngine;
using UnityEngine.AI;
using Features.Enemy.Data;

namespace Features.Enemy.Systems
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMover : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private EnemyBlackboard _blackboard;
        private Vector3 _knockbackVelocity;
        private bool _isKnockedBack;

        // Helper to check if we can safely use the NavMeshAgent
        private bool IsAgentValid => _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;

        public void Initialize(EnemyBlackboard blackboard)
        {
            _blackboard = blackboard;
            _agent = GetComponent<NavMeshAgent>();
            
            if (_blackboard?.Config != null)
            {
                _agent.speed = _blackboard.Config.MoveSpeed;
                _agent.angularSpeed = 360f;
                _agent.acceleration = 8f;
            }
        }

        public void OnUpdate()
        {
            if (_isKnockedBack)
            {
                _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
                
                if (_knockbackVelocity.magnitude < 0.1f)
                {
                    _isKnockedBack = false;
                    _knockbackVelocity = Vector3.zero;
                    
                    // Only resume if agent is valid
                    if (IsAgentValid) _agent.isStopped = false;
                }
                else
                {
                    transform.position += _knockbackVelocity * Time.deltaTime;
                }
            }
        }

        public void MoveTo(Vector3 destination, float speed)
        {
            // Check validity before trying to move
            if (_isKnockedBack || !IsAgentValid) return;

            _agent.speed = speed;
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (!IsAgentValid) return;
            _agent.isStopped = true;
        }

        public void ApplyKnockback(Vector3 direction, float force)
        {
            _isKnockedBack = true;
            _knockbackVelocity = direction.normalized * force;
            
            if (IsAgentValid) _agent.isStopped = true;
        }

        public bool IsMoving()
        {
            if (!IsAgentValid) return false;
            return _agent.velocity.magnitude > 0.1f;
        }

        public bool HasReachedDestination()
        {
            if (!IsAgentValid || _agent.pathPending) return false;
            return _agent.remainingDistance <= _agent.stoppingDistance;
        }
    }
}
