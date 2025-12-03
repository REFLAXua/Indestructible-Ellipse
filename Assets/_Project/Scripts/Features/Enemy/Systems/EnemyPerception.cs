using UnityEngine;
using Features.Enemy.Data;

namespace Features.Enemy.Systems
{
    public class EnemyPerception : MonoBehaviour
    {
        private EnemyBlackboard _blackboard;
        private Transform _cachedPlayerTransform;
        private float _detectionCheckInterval = 0.2f;
        private float _nextCheckTime;

        public void Initialize(EnemyBlackboard blackboard)
        {
            _blackboard = blackboard;
        }

        public void OnUpdate()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + _detectionCheckInterval;

            DetectPlayer();
        }

        private void DetectPlayer()
        {
            if (_blackboard?.Config == null) return;

            if (_cachedPlayerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _cachedPlayerTransform = player.transform;
                }
            }

            if (_cachedPlayerTransform == null)
            {
                _blackboard.Target = null;
                _blackboard.CanAttack = false;
                return;
            }

            float distance = Vector3.Distance(transform.position, _cachedPlayerTransform.position);

            if (distance <= _blackboard.Config.DetectionRange)
            {
                Vector3 directionToPlayer = (_cachedPlayerTransform.position - transform.position).normalized;
                
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, _blackboard.Config.DetectionRange))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        _blackboard.Target = _cachedPlayerTransform;
                        _blackboard.CanAttack = distance <= _blackboard.Config.AttackRange;
                        return;
                    }
                }
            }

            if (_blackboard.Target != null)
            {
                if (distance > _blackboard.Config.DetectionRange * 1.2f)
                {
                    _blackboard.Target = null;
                    _blackboard.CanAttack = false;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_blackboard?.Config == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _blackboard.Config.DetectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _blackboard.Config.AttackRange);
        }
    }
}
