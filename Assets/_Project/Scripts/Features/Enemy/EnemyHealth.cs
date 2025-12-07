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
        public event Action<float, float> OnHealthChanged;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;

        private MeshRenderer[] _renderers;
        private Color[] _originalColors;
        private Coroutine _colorRestoreCoroutine;
        private bool _isDead;
        private float _hitFlashDuration = 0.15f;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            CacheRenderers();
        }

        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();

            if (_renderers.Length > 0)
            {
                _originalColors = new Color[_renderers.Length];
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null && _renderers[i].material != null)
                    {
                        _originalColors[i] = _renderers[i].material.color;
                    }
                }
            }
        }

        public void Initialize(float maxHealth, float hitFlashDuration = 0.15f)
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            _hitFlashDuration = hitFlashDuration;
            _isDead = false;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeDamage(float amount, Vector3? knockbackDir = null)
        {
            if (_isDead || _currentHealth <= 0) return;

            FlashRed();

            _currentHealth -= amount;
            _currentHealth = Mathf.Max(0, _currentHealth);

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnHit?.Invoke(knockbackDir);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void FlashRed()
        {
            if (_colorRestoreCoroutine != null)
            {
                StopCoroutine(_colorRestoreCoroutine);
            }

            SetRenderersColor(Color.red);
            _colorRestoreCoroutine = StartCoroutine(RestoreColorAfterDelay(_hitFlashDuration));
        }

        private void SetRenderersColor(Color color)
        {
            if (_renderers == null) return;

            foreach (var renderer in _renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        private IEnumerator RestoreColorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_renderers == null || _originalColors == null) yield break;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null && i < _originalColors.Length)
                {
                    _renderers[i].material.color = _originalColors[i];
                }
            }
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (_colorRestoreCoroutine != null)
            {
                StopCoroutine(_colorRestoreCoroutine);
                StartCoroutine(RestoreColorAfterDelay(_hitFlashDuration));
            }

            OnDeath?.Invoke();

            var controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.OnDeath();
            }
        }

        public bool IsDead => _isDead;
    }
}
