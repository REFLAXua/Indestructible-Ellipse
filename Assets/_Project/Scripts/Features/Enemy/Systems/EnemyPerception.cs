using UnityEngine;
using Features.Enemy.Data;

namespace Features.Enemy.Systems
{
    public class EnemyPerception : MonoBehaviour
    {
        private EnemyBlackboard _blackboard;
        private Transform _cachedPlayerTransform;
        private float _nextCheckTime;
        private bool _initialized;

        public void Initialize(EnemyBlackboard blackboard)
        {
            _blackboard = blackboard;
            _initialized = true;
            CachePlayerTransform();
        }

        private void CachePlayerTransform()
        {
            if (_cachedPlayerTransform != null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _cachedPlayerTransform = player.transform;
            }
        }

        public void OnUpdate()
        {
            if (!_initialized || _blackboard?.Config == null) return;

            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + _blackboard.Config.DetectionDelay;

            DetectPlayer();
        }

        private void DetectPlayer()
        {
            if (_cachedPlayerTransform == null)
            {
                CachePlayerTransform();
                if (_cachedPlayerTransform == null)
                {
                    _blackboard.Target = null;
                    _blackboard.CanAttack = false;
                    return;
                }
            }

            float distance = Vector3.Distance(transform.position, _cachedPlayerTransform.position);
            float detectionRange = _blackboard.Config.DetectionRange;
            float attackRange = _blackboard.Config.AttackRange;
            float lossMultiplier = _blackboard.Config.DetectionLossMultiplier;

            if (distance <= detectionRange)
            {
                Vector3 directionToPlayer = (_cachedPlayerTransform.position - transform.position).normalized;

                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRange))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        _blackboard.Target = _cachedPlayerTransform;
                        _blackboard.CanAttack = distance <= attackRange;
                        return;
                    }
                }
            }

            if (_blackboard.Target != null)
            {
                if (distance > detectionRange * lossMultiplier)
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
