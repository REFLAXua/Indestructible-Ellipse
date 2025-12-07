using UnityEngine;
using TMPro;
using Core;
using Features.NPC.Events;
using Features.NPC.Interfaces;

namespace Features.NPC.UI
{
    public class WorldSpacePromptUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private RectTransform _promptPanel;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeSpeed = 5f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobAmount = 0.1f;

        [Header("Billboard Settings")]
        [SerializeField] private bool _alwaysFaceCamera = true;

        private Transform _cameraTransform;
        private float _targetAlpha;
        private Vector3 _basePosition;
        private bool _isVisible;

        private void Awake()
        {
            ValidateReferences();
            CacheCameraTransform();
            _basePosition = transform.localPosition;
            HideImmediate();
        }

        private void ValidateReferences()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void CacheCameraTransform()
        {
            if (UnityEngine.Camera.main != null)
            {
                _cameraTransform = UnityEngine.Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            UpdateFade();
            
            if (_isVisible)
            {
                UpdateBobAnimation();
                
                if (_alwaysFaceCamera)
                {
                    UpdateBillboard();
                }
            }
        }

        private void UpdateFade()
        {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = Mathf.MoveTowards(
                _canvasGroup.alpha, 
                _targetAlpha, 
                _fadeSpeed * Time.deltaTime
            );
        }

        private void UpdateBobAnimation()
        {
            float bob = Mathf.Sin(Time.time * _bobSpeed) * _bobAmount;
            transform.localPosition = _basePosition + Vector3.up * bob;
        }

        private void UpdateBillboard()
        {
            if (_cameraTransform == null)
            {
                CacheCameraTransform();
                return;
            }

            transform.rotation = Quaternion.LookRotation(
                transform.position - _cameraTransform.position
            );
        }

        public void Show(string prompt)
        {
            _isVisible = true;
            gameObject.SetActive(true);
            _targetAlpha = 1f;

            if (_promptText != null)
            {
                _promptText.text = prompt;
            }
        }

        public void Hide()
        {
            _isVisible = false;
            _targetAlpha = 0f;
        }

        public void HideImmediate()
        {
            _isVisible = false;
            _targetAlpha = 0f;
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        public void SetPromptColor(Color color)
        {
            if (_promptText != null)
            {
                _promptText.color = color;
            }
        }
    }
}
