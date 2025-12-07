using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Features.UI
{
    public class StatBarView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _ghostImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _valueText;
        [SerializeField] private RectTransform _waveOverlay;

        [Header("Settings")]
        [SerializeField] private float _fillSpeed = 10f;
        [SerializeField] private float _ghostDelay = 0.5f;
        [SerializeField] private float _ghostSpeed = 5f;
        [SerializeField] private Gradient _colorGradient;
        [SerializeField] private float _pulseMinAlpha = 0.5f;
        [SerializeField] private float _pulseSpeed = 2f;

        private float _targetFill = 1f;
        private Coroutine _ghostRoutine;

        public void Initialize(float current, float max)
        {
            if (max <= 0) max = 1;

            _targetFill = current / max;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = _targetFill;
            }
            if (_ghostImage != null)
            {
                _ghostImage.fillAmount = _targetFill;
            }

            UpdateColor(_targetFill);
            UpdateValueText(current, max);
        }

        public void SetValue(float current, float max, bool instant = false)
        {
            if (max <= 0) max = 1;

            float newFill = Mathf.Clamp01(current / max);

            if (instant)
            {
                SetFillInstant(newFill);
            }
            else
            {
                HandleGhostAnimation(newFill);
            }

            _targetFill = newFill;
            UpdateValueText(current, max);
        }

        private void SetFillInstant(float fill)
        {
            if (_fillImage != null) _fillImage.fillAmount = fill;
            if (_ghostImage != null) _ghostImage.fillAmount = fill;
        }

        private void HandleGhostAnimation(float newFill)
        {
            if (_ghostImage == null) return;

            float currentFill = _fillImage != null ? _fillImage.fillAmount : 0;

            if (newFill < currentFill)
            {
                if (_ghostRoutine != null) StopCoroutine(_ghostRoutine);
                _ghostRoutine = StartCoroutine(GhostAnimation(newFill));
            }
            else if (newFill > currentFill)
            {
                _ghostImage.fillAmount = newFill;
            }
        }

        private void UpdateValueText(float current, float max)
        {
            if (_valueText != null)
            {
                _valueText.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
            }
        }

        private void Update()
        {
            AnimateFill();
            UpdateGlobePositionIfNeeded();
        }

        private void AnimateFill()
        {
            if (_fillImage == null) return;
            if (Mathf.Abs(_fillImage.fillAmount - _targetFill) <= 0.001f) return;

            _fillImage.fillAmount = Mathf.Lerp(_fillImage.fillAmount, _targetFill, Time.deltaTime * _fillSpeed);
            UpdateColor(_fillImage.fillAmount);
        }

        private void UpdateGlobePositionIfNeeded()
        {
            if (_fillImage == null) return;
            if (_fillImage.type != Image.Type.Filled) return;
            if (_fillImage.fillOrigin != 1) return;

            UpdateGlobePosition(_fillImage.fillAmount);
        }

        private void UpdateGlobePosition(float fillPercent)
        {
            RectTransform fillRect = _fillImage.rectTransform;
            float fillHeight = fillRect.rect.height;
            float offset = -fillHeight * (1f - fillPercent);

            Vector2 anchoredPos = fillRect.anchoredPosition;
            fillRect.anchoredPosition = new Vector2(anchoredPos.x, offset);
        }

        private void UpdateColor(float percent)
        {
            if (_colorGradient == null || _fillImage == null) return;
            _fillImage.color = _colorGradient.Evaluate(percent);
        }

        private IEnumerator GhostAnimation(float targetFill)
        {
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
                float alpha = Mathf.PingPong(Time.time * _pulseSpeed, 1f - _pulseMinAlpha) + _pulseMinAlpha;
                _canvasGroup.alpha = alpha;
            }
            else
            {
                _canvasGroup.alpha = 1f;
            }
        }
    }
}
