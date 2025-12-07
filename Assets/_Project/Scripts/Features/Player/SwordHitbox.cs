using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Features.Enemy;

namespace Features.Player
{
    [RequireComponent(typeof(Collider))]
    public class SwordHitbox : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private GameObject _woundPrefab;
        [SerializeField] private Transform _swordTip;

        [Header("Wound Settings")]
        [SerializeField] private float _surfaceOffset = 0.03f;
        [SerializeField] private bool _followSurface = true;

        [Header("Pooling")]
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private float _woundLifetime = 10f;

        private MeshRenderer _renderer;
        private bool _canDealDamage;
        private float _damage;
        private HashSet<GameObject> _hitEnemies = new HashSet<GameObject>();
        private Transform _owner;
        private Color _originalColor = Color.white;

        private Vector3 _lastTipPosition;
        private Vector3 _swordVelocity;

        private Queue<GameObject> _woundPool = new Queue<GameObject>();
        private Transform _poolContainer;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>() ?? GetComponentInChildren<MeshRenderer>();
            if (_renderer != null && _renderer.material != null)
            {
                _originalColor = _renderer.material.color;
            }

            if (_swordTip == null) _swordTip = transform;
            _lastTipPosition = _swordTip.position;

            InitializePool();
        }

        private void InitializePool()
        {
            if (_woundPrefab == null) return;

            _poolContainer = new GameObject("WoundPool").transform;
            _poolContainer.SetParent(null);
            DontDestroyOnLoad(_poolContainer.gameObject);

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreatePooledWound();
            }
        }

        private GameObject CreatePooledWound()
        {
            var wound = Instantiate(_woundPrefab, _poolContainer);
            wound.SetActive(false);
            _woundPool.Enqueue(wound);
            return wound;
        }

        private GameObject GetWoundFromPool()
        {
            if (_woundPool.Count == 0)
            {
                return CreatePooledWound();
            }

            var wound = _woundPool.Dequeue();
            wound.SetActive(true);
            return wound;
        }

        public void ReturnWoundToPool(GameObject wound)
        {
            if (wound == null) return;

            wound.transform.SetParent(_poolContainer);
            wound.SetActive(false);
            _woundPool.Enqueue(wound);
        }

        private void Update()
        {
            if (Time.deltaTime > 0 && _swordTip != null)
            {
                _swordVelocity = (_swordTip.position - _lastTipPosition) / Time.deltaTime;
                _lastTipPosition = _swordTip.position;
            }
        }

        public void Initialize(Transform owner)
        {
            _owner = owner;
        }

        public void EnableDamage(float damage)
        {
            _canDealDamage = true;
            _damage = damage;
            _hitEnemies.Clear();
            SetRendererColor(Color.red);

            if (_swordTip != null) _lastTipPosition = _swordTip.position;
        }

        public void DisableDamage()
        {
            _canDealDamage = false;
            SetRendererColor(_originalColor);
        }

        private void SetRendererColor(Color color)
        {
            if (_renderer != null && _renderer.material != null)
            {
                _renderer.material.color = color;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_canDealDamage) return;
            if (!other.CompareTag("Enemy")) return;

            // Розріз створюється на кожному колайдері окремо
            CreateWoundEffect(other);

            // Знаходимо кореневий об'єкт ворога для відстеження шкоди (один раз на ворога)
            var enemyController = other.GetComponentInParent<EnemyController>();
            GameObject enemyRoot = enemyController != null ? enemyController.gameObject : other.gameObject;

            if (_hitEnemies.Contains(enemyRoot)) return;

            _hitEnemies.Add(enemyRoot);
            DealDamageToEnemy(other);
        }

        private void DealDamageToEnemy(Collider other)
        {
            var enemyController = other.GetComponentInParent<EnemyController>();
            if (enemyController != null)
            {
                Vector3 knockbackDir = _owner != null
                    ? (enemyController.transform.position - _owner.position).normalized
                    : transform.forward;

                enemyController.TakeDamage(_damage, knockbackDir);
                return;
            }

            var health = other.GetComponentInParent<EnemyHealth>();
            health?.TakeDamage(_damage);
        }

        private void CreateWoundEffect(Collider other)
        {
            if (_woundPrefab == null) return;

            Vector3 hitPoint;
            Vector3 normal;

            if (!FindBestHitPoint(other, out hitPoint, out normal))
            {
                return;
            }

            Vector3 slashDirection = CalculateSlashDirection(normal);
            Quaternion rotation = Quaternion.LookRotation(-normal, slashDirection);

            Vector3 spawnPos = hitPoint + normal * _surfaceOffset;

            var wound = GetWoundFromPool();
            wound.transform.position = spawnPos;
            wound.transform.rotation = rotation;
            wound.transform.SetParent(other.transform);

            AdjustWoundScale(wound, other, hitPoint, normal);

            if (_followSurface)
            {
                var follower = wound.GetComponent<WoundFollower>();
                if (follower == null)
                {
                    follower = wound.AddComponent<WoundFollower>();
                }
                follower.Initialize(other, normal, _surfaceOffset);
            }

            StartCoroutine(ReturnWoundAfterDelay(wound, _woundLifetime));
        }

        private bool FindBestHitPoint(Collider other, out Vector3 hitPoint, out Vector3 normal)
        {
            hitPoint = Vector3.zero;
            normal = Vector3.up;

            // Визначаємо позицію атакуючого (гравця)
            Vector3 attackerPos = _owner != null ? _owner.position : transform.position;
            attackerPos.y += 1f; // Трохи вище для кращого raycast
            
            Vector3 targetCenter = other.bounds.center;
            Vector3 directionToTarget = (targetCenter - attackerPos).normalized;

            // Raycast від гравця до ворога
            float rayDistance = 10f;
            
            if (Physics.Raycast(attackerPos, directionToTarget, out RaycastHit hit, rayDistance))
            {
                if (hit.collider == other)
                {
                    // Перевіряємо, що нормаль дивиться на нас (кут < 90 градусів)
                    float dotProduct = Vector3.Dot(hit.normal, -directionToTarget);
                    if (dotProduct > 0.1f)
                    {
                        hitPoint = hit.point;
                        normal = hit.normal;
                        return true;
                    }
                }
            }

            // Fallback: найближча точка на колайдері
            Vector3 closestPoint = other.ClosestPoint(attackerPos);
            Vector3 dirFromAttacker = (closestPoint - attackerPos).normalized;
            
            // Нормаль = напрямок від точки до гравця
            normal = -dirFromAttacker;
            
            // Перевіряємо, що точка на видимій стороні
            float checkDot = Vector3.Dot(normal, -dirFromAttacker);
            if (checkDot < 0)
            {
                // Точка на протилежному боці - не створюємо розріз
                return false;
            }
            
            hitPoint = closestPoint;
            return true;
        }

        private Vector3 CalculateSlashDirection(Vector3 normal)
        {
            Vector3 slashDirection = Vector3.ProjectOnPlane(_swordVelocity, normal).normalized;

            if (slashDirection.sqrMagnitude < 0.01f)
            {
                slashDirection = Vector3.ProjectOnPlane(transform.up, normal).normalized;
            }

            if (slashDirection.sqrMagnitude < 0.01f)
            {
                Vector3 arbitrary = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f 
                    ? Vector3.right 
                    : Vector3.up;
                slashDirection = Vector3.Cross(normal, arbitrary).normalized;
            }

            return slashDirection;
        }

        private void AdjustWoundScale(GameObject wound, Collider other, Vector3 hitPoint, Vector3 normal)
        {
            float fixedWidth = 0.02f;
            float maxLength = 0.4f;
            float minLength = 0.1f;
            
            Vector3 up = wound.transform.up;

            float distUp1 = GetDistanceToEdge(hitPoint, normal, up, other);
            float distUp2 = GetDistanceToEdge(hitPoint, normal, -up, other);

            float minDist = Mathf.Min(distUp1, distUp2);
            float length = Mathf.Clamp(minDist * 2f, minLength, maxLength);

            wound.transform.localScale = new Vector3(fixedWidth, length, 1f);
        }

        private float GetDistanceToEdge(Vector3 origin, Vector3 surfaceNormal, Vector3 direction, Collider col)
        {
            float maxCheckDistance = 0.5f;
            int steps = 5;
            float stepSize = maxCheckDistance / steps;

            for (int i = 1; i <= steps; i++)
            {
                Vector3 testPoint = origin + direction * (stepSize * i);
                Vector3 closest = col.ClosestPoint(testPoint);
                
                float distFromSurface = Vector3.Distance(testPoint, closest);
                
                if (distFromSurface > 0.05f)
                {
                    return stepSize * (i - 1);
                }
            }

            return maxCheckDistance;
        }

        private IEnumerator ReturnWoundAfterDelay(GameObject wound, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnWoundToPool(wound);
        }

        private void OnDestroy()
        {
            if (_poolContainer != null)
            {
                Destroy(_poolContainer.gameObject);
            }
        }
    }

    public class WoundFollower : MonoBehaviour
    {
        private Transform _targetBone;
        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private bool _initialized;

        public void Initialize(Collider targetCollider, Vector3 surfaceNormal, float offset)
        {
            if (targetCollider == null) return;

            Transform root = targetCollider.transform;
            
            _targetBone = FindNearestBone(root, transform.position);
            
            if (_targetBone == null)
            {
                _targetBone = root;
            }

            _localPosition = _targetBone.InverseTransformPoint(transform.position);
            _localRotation = Quaternion.Inverse(_targetBone.rotation) * transform.rotation;
            
            transform.SetParent(null);
            
            _initialized = true;
        }

        private Transform FindNearestBone(Transform root, Vector3 worldPos)
        {
            var skinnedMesh = root.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh == null || skinnedMesh.bones == null || skinnedMesh.bones.Length == 0)
            {
                return null;
            }

            Transform nearest = null;
            float minDist = float.MaxValue;

            foreach (var bone in skinnedMesh.bones)
            {
                if (bone == null) continue;
                
                float dist = Vector3.Distance(bone.position, worldPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = bone;
                }
            }

            return nearest;
        }

        private void LateUpdate()
        {
            if (!_initialized) return;
            
            if (_targetBone == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = _targetBone.TransformPoint(_localPosition);
            transform.rotation = _targetBone.rotation * _localRotation;
        }

        private void OnDisable()
        {
            _initialized = false;
            _targetBone = null;
        }
    }
}

