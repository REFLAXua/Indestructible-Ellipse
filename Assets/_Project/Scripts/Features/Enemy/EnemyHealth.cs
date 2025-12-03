using UnityEngine;
using System;
using System.Collections;

namespace Features.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;
        private float _currentHealth;

        public event Action OnDeath;
        public event Action<Vector3?> OnHit;
        public event Action<float, float> OnHealthChanged; // Current, Max

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        private MeshRenderer[] _renderers;
        private Color[] _originalColors;
        private Coroutine _colorRestoreCoroutine;

        private void Awake()
        {
            _currentHealth = _maxHealth;

            // Get all MeshRenderers on this enemy and children
            _renderers = GetComponentsInChildren<MeshRenderer>();
            
            if (_renderers.Length > 0)
            {
                _originalColors = new Color[_renderers.Length];
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i].material != null)
                    {
                        _originalColors[i] = _renderers[i].material.color;
                    }
                }
            }
            else
            {
                Debug.LogWarning("EnemyHealth: No MeshRenderers found on enemy.");
            }
        }

        public void Initialize(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeDamage(float amount, Vector3? knockbackDir = null)
        {
            if (_currentHealth <= 0) return;

            // Stop previous color restore if it's running
            if (_colorRestoreCoroutine != null)
            {
                StopCoroutine(_colorRestoreCoroutine);
            }

            // Set all renderers to red
            foreach (var renderer in _renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = new Color(1f, 0f, 0f, 1f);
                }
            }

            // Start coroutine to restore original colors after delay
            _colorRestoreCoroutine = StartCoroutine(RestoreColorAfterDelay(0.15f));

            _currentHealth -= amount;
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnHit?.Invoke(knockbackDir);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator RestoreColorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                {
                    _renderers[i].material.color = _originalColors[i];
                }
            }
        }

        private void Die()
        {
            // Stop color restore on death
            if (_colorRestoreCoroutine != null)
            {
                StopCoroutine(_colorRestoreCoroutine);
                 _colorRestoreCoroutine = StartCoroutine(RestoreColorAfterDelay(0.15f));
            }
            
            OnDeath?.Invoke();
            GetComponent<EnemyController>().OnDeath();
        }
    }
}
