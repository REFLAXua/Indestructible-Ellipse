using UnityEngine;
using System.Collections.Generic;
using Features.Enemy;

namespace Features.Player
{
    /// <summary>
    /// Attach this script to the Weapon/Sword GameObject.
    /// Requires a Collider with IsTrigger = true.
    /// </summary>
    public class SwordHitbox : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private GameObject _woundPrefab; // Prefab with red line/slash sprite
        [SerializeField] private Transform _swordTip; // Assign the tip of the sword to calculate swing velocity accurately

        private MeshRenderer rend;
        private bool _canDealDamage;
        private float _damage;
        private HashSet<GameObject> _hitEnemies = new HashSet<GameObject>();
        private Transform _owner; // The player transform for knockback calculation
        private Color _originalColor = Color.white;

        // Velocity Tracking
        private Vector3 _lastTipPosition;
        private Vector3 _swordVelocity;

        void Start()
        {
            // assign to the field (don't shadow). Try component first, then children.
            rend = GetComponent<MeshRenderer>() ?? GetComponentInChildren<MeshRenderer>();
            if (rend != null && rend.material != null)
            {
                _originalColor = rend.material.color;
            }

            // If tip is not assigned, fallback to this transform (though less accurate for swings)
            if (_swordTip == null) _swordTip = transform;
            _lastTipPosition = _swordTip.position;
        }

        private void Update()
        {
            // Calculate velocity of the sword tip to determine slash direction
            if (Time.deltaTime > 0)
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
            if (rend != null && rend.material != null)
            {
                rend.material.color = Color.red;
            }
            
            // Reset velocity tracking to avoid jump artifacts
            if (_swordTip != null) _lastTipPosition = _swordTip.position;
        }

        public void DisableDamage()
        {
            _canDealDamage = false;
            if (rend != null && rend.material != null)
            {
                rend.material.color = _originalColor;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_canDealDamage) return;
            if (!other.CompareTag("Enemy")) return;
            if (_hitEnemies.Contains(other.gameObject)) return;

            _hitEnemies.Add(other.gameObject);

            // Create Wound Effect aligned with the slash
            CreateWoundEffect(other);

            // 1. Try to use the main EnemyController (Preferred Architecture)
            var enemyController = other.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                Vector3 knockbackDir = _owner != null 
                    ? (other.transform.position - _owner.position).normalized 
                    : transform.forward;
                
                enemyController.TakeDamage(_damage, knockbackDir);
                return;
            }

            // 2. Fallback/Direct Component Access
            var health = other.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(_damage);
            }
        }

        private void CreateWoundEffect(Collider other)
        {
            if (_woundPrefab == null) return;

            // 1. Find exact hit point
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            
            // 2. Determine Surface Normal
            Vector3 attackDir = (hitPoint - transform.position).normalized;
            if (attackDir == Vector3.zero) attackDir = transform.forward;
            Vector3 normal = -attackDir; 

            // Refine normal with Raycast
            Vector3 rayOrigin = transform.position - attackDir * 0.5f;
            if (Physics.Raycast(rayOrigin, attackDir, out RaycastHit hitInfo, 2f))
            {
                if (hitInfo.collider == other)
                {
                    hitPoint = hitInfo.point;
                    normal = hitInfo.normal;
                }
            }

            // 3. Calculate Slash Direction on the Surface
            // Project the sword's velocity onto the surface plane
            Vector3 slashDirection = Vector3.ProjectOnPlane(_swordVelocity, normal).normalized;

            // If velocity is too small (stationary hit), fallback to random or sword up vector
            if (slashDirection.sqrMagnitude < 0.01f)
            {
                 slashDirection = Vector3.ProjectOnPlane(transform.up, normal).normalized;
            }

            // 4. Instantiate and Align
            Vector3 spawnPos = hitPoint + normal * 0.01f; // Offset
            
            // We want the decal's "Up" (or Right) to align with the slash direction.
            // LookRotation(forward, up):
            // - forward: The vector pointing "out" of the surface (normal).
            // - up: The vector defining the rotation around the normal (slashDirection).
            // Note: Check your prefab's orientation. Usually, a Quad faces -Z (forward). 
            // So we might need to look along the normal.
            
            // FIX: Use -normal because Unity Quads face -Z local. 
            // So to have the face point along 'normal', we point Z along '-normal'.
            Quaternion rotation = Quaternion.LookRotation(-normal, slashDirection);

            var wound = Instantiate(_woundPrefab, spawnPos, rotation);
            wound.transform.SetParent(other.transform);

            // Optional: Rotate 90 degrees if your texture is horizontal but the code aligns vertical
            // wound.transform.Rotate(0, 0, 90); 

            Destroy(wound, 10f);
        }
    }
}
