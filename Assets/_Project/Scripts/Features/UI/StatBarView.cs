using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Features.UI
{
    public class StatBarView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _fillImage;      // The main colored bar
        [SerializeField] private Image _ghostImage;     // The delayed white bar (optional)
        [SerializeField] private CanvasGroup _canvasGroup; // For fading/pulsing
        [SerializeField] private Text _valueText;       // Optional text display (e.g. "100/100")
        [SerializeField] private RectTransform _waveOverlay; // Optional animated wave sprite that sits on top of liquid

        [Header("Settings")]
        [SerializeField] private float _fillSpeed = 10f;
        [SerializeField] private float _ghostDelay = 0.5f;
        [SerializeField] private float _ghostSpeed = 5f;
        [SerializeField] private Gradient _colorGradient; // Color based on percentage

        private float _targetFill = 1f;
        private Coroutine _ghostRoutine;

        public void Initialize(float current, float max)
        {
            _targetFill = current / max;
            _fillImage.fillAmount = _targetFill;
            if (_ghostImage != null) _ghostImage.fillAmount = _targetFill;
            UpdateColor(_targetFill);

            if (_valueText != null)
            {
                _valueText.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            }
        }

        public void SetValue(float current, float max, bool instant = false)
        {
            float newFill = Mathf.Clamp01(current / max);
            
            if (instant)
            {
                _fillImage.fillAmount = newFill;
                if (_ghostImage != null) _ghostImage.fillAmount = newFill;
            }
            else
            {
                // If taking damage (newFill < current), trigger ghost effect
                if (newFill < _fillImage.fillAmount && _ghostImage != null)
                {
                    // Ghost stays at old value for a moment, then shrinks
                    if (_ghostRoutine != null) StopCoroutine(_ghostRoutine);
                    _ghostRoutine = StartCoroutine(GhostAnimation(newFill));
                }
                else if (newFill > _fillImage.fillAmount && _ghostImage != null)
                {
                    // If healing, ghost snaps to new value immediately (or follows, depends on taste)
                    _ghostImage.fillAmount = newFill;
                }
            }

            _targetFill = newFill;
            
            if (_valueText != null)
            {
                _valueText.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            }
        }

        private void Update()
        {
            // Smooth fill animation
            if (Mathf.Abs(_fillImage.fillAmount - _targetFill) > 0.001f)
            {
                _fillImage.fillAmount = Mathf.Lerp(_fillImage.fillAmount, _targetFill, Time.deltaTime * _fillSpeed);
                UpdateColor(_fillImage.fillAmount);
            }

            // For globes with Fill Origin: Top, move the entire image down as it empties
            // This keeps the liquid at the bottom and the animated wave on top
            if (_fillImage != null && _fillImage.type == Image.Type.Filled && _fillImage.fillOrigin == 1) // 1 = Top for Vertical
            {
                UpdateGlobePosition(_fillImage.fillAmount);
            }
        }

        private void UpdateGlobePosition(float fillPercent)
        {
            RectTransform fillRect = _fillImage.rectTransform;
            float fillHeight = fillRect.rect.height;
            
            // Move down more aggressively: multiply by full height instead of half
            float offset = -fillHeight * (1f - fillPercent);
            
            Vector2 anchoredPos = fillRect.anchoredPosition;
            fillRect.anchoredPosition = new Vector2(anchoredPos.x, offset);
        }

        // ... (rest of the class)


        private void UpdateColor(float percent)
        {
            if (_colorGradient != null)
            {
                _fillImage.color = _colorGradient.Evaluate(percent);
            }
        }

        private IEnumerator GhostAnimation(float targetFill)
        {
            // Wait before shrinking ghost
            yield return new WaitForSeconds(_ghostDelay);

            while (Mathf.Abs(_ghostImage.fillAmount - targetFill) > 0.001f)
            {
                _ghostImage.fillAmount = Mathf.Lerp(_ghostImage.fillAmount, targetFill, Time.deltaTime * _ghostSpeed);
                yield return null;
            }
            _ghostImage.fillAmount = targetFill;
        }

        public void PulseLow(bool isLow)
        {
            if (_canvasGroup == null) return;
            
            if (isLow)
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 0.5f) + 0.5f; // Pulse between 0.5 and 1.0
                _canvasGroup.alpha = alpha;
            }
            else
            {
                _canvasGroup.alpha = 1f;
            }
        }
    }
}
