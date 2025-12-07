using UnityEngine;
using UnityEngine.UI;

namespace Features.Enemy
{
    public class EnemyHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyHealth _health;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Canvas _canvas;

        [Header("Settings")]
        [SerializeField] private bool _billboard = true;

        private Transform _cameraTransform;

        private void Awake()
        {
            if (!FindHealthComponent()) return;
            FindUIComponents();
            CacheCamera();
        }

        private bool FindHealthComponent()
        {
            if (_health == null)
            {
                _health = GetComponentInParent<EnemyHealth>();
                if (_health == null) _health = GetComponent<EnemyHealth>();
            }

            if (_health == null)
            {
                Debug.LogError($"[EnemyHealthUI] Could not find EnemyHealth component on {gameObject.name} or its parents!", gameObject);
                enabled = false;
                return false;
            }

            return true;
        }

        private void FindUIComponents()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_healthSlider == null) _healthSlider = GetComponentInChildren<Slider>();
            if (_healthSlider == null) _healthSlider = GetComponentInParent<Slider>();

            if (_healthBarFill == null && _healthSlider == null)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.type == Image.Type.Filled || img.name.Contains("Fill"))
                    {
                        _healthBarFill = img;
                        break;
                    }
                }
            }

            if (_healthBarFill == null && _healthSlider == null)
            {
                Debug.LogError($"[EnemyHealthUI] Could not find a Slider OR a Fill Image on {gameObject.name}.", gameObject);
            }
        }

        private void CacheCamera()
        {
            if (UnityEngine.Camera.main != null)
            {
                _cameraTransform = UnityEngine.Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(_health.CurrentHealth, _health.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void LateUpdate()
        {
            if (!_billboard || _canvas == null) return;

            if (_cameraTransform == null)
            {
                CacheCamera();
                if (_cameraTransform == null) return;
            }

            _canvas.transform.rotation = _cameraTransform.rotation;
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (max <= 0) max = 1;

            if (_healthSlider != null)
            {
                _healthSlider.maxValue = max;
                _healthSlider.value = current;
            }
            else if (_healthBarFill != null)
            {
                float pct = Mathf.Clamp01(current / max);
                _healthBarFill.fillAmount = pct;
            }
        }
    }
}
