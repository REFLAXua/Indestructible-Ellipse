using UnityEngine;
using UnityEngine.UI;

namespace Features.Enemy
{
    public class EnemyHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyHealth _health;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Slider _healthSlider; // Support for Slider component
        [SerializeField] private Canvas _canvas;

        [Header("Settings")]
        [SerializeField] private bool _billboard = true;
    
        private Transform _cameraTransform;

        private void Awake()
        {
            // 1. Find Health Component
            if (_health == null)
            {
                _health = GetComponentInParent<EnemyHealth>();
                if (_health == null) _health = GetComponent<EnemyHealth>();
            }

            if (_health == null)
            {
                Debug.LogError($"[EnemyHealthUI] Could not find EnemyHealth component on {gameObject.name} or its parents!", gameObject);
                enabled = false;
                return;
            }

            // 2. Find Canvas
            if (_canvas == null) _canvas = GetComponent<Canvas>();

            // 3. Find Slider (Preferred if using Slider prefab)
            if (_healthSlider == null) _healthSlider = GetComponentInChildren<Slider>();
            // Also check parent if the script is on the Fill object itself
            if (_healthSlider == null) _healthSlider = GetComponentInParent<Slider>();

            // 4. Find Fill Image (Fallback or manual assignment)
            if (_healthBarFill == null && _healthSlider == null)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    // Look for something that looks like a fill bar
                    if (img.type == Image.Type.Filled || img.name.Contains("Fill"))
                    {
                        _healthBarFill = img;
                        break;
                    }
                }
            }

            // Removed auto-fix for Sliced images. 
            // If using Sliced, a Slider component is required to handle the sizing.
            
            if (_healthBarFill == null && _healthSlider == null)
            {
                Debug.LogError($"[EnemyHealthUI] Could not find a Slider OR a Fill Image on {gameObject.name}.", gameObject);
            }

            // 5. Cache Camera
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
                _cameraTransform = UnityEngine.Camera.main?.transform;
            }

            if (_cameraTransform != null)
            {
                _canvas.transform.rotation = _cameraTransform.rotation;
            }
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (max <= 0) max = 1;

            if (_healthSlider != null)
            {
                // Sync slider range with health values
                _healthSlider.maxValue = max;
                _healthSlider.value = current;
            }
            else if (_healthBarFill != null)
            {
                // Fallback for simple Image fill
                float pct = Mathf.Clamp01(current / max);
                _healthBarFill.fillAmount = pct;
            }
        }
    }
}
