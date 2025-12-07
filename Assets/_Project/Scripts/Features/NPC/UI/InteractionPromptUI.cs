using UnityEngine;
using TMPro;
using Core;
using Features.NPC.Events;
using Features.NPC.Interfaces;

namespace Features.NPC.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _npcNameText;
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private GameObject _promptContainer;

        [Header("Style")]
        [SerializeField] private Color _nameColor = Color.white;
        [SerializeField] private Color _promptColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        [Header("Animation Settings")]
        [SerializeField] private float _fadeSpeed = 5f;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseAmount = 0.1f;

        private float _targetAlpha;
        private float _baseScale;
        private INPCInteractable _currentNPC;
        private string _currentNPCName;
        private bool _isVisible;

        private void Awake()
        {
            ValidateReferences();
            _baseScale = transform.localScale.x;
            Hide();
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

            if (_promptContainer == null)
            {
                _promptContainer = gameObject;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerEnteredNPCRangeEvent>(OnPlayerEnteredNPCRange);
            EventBus.Subscribe<PlayerExitedNPCRangeEvent>(OnPlayerExitedNPCRange);
            EventBus.Subscribe<NPCInteractionStartedEvent>(OnInteractionStarted);
            EventBus.Subscribe<NPCInteractionEndedEvent>(OnInteractionEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerEnteredNPCRangeEvent>(OnPlayerEnteredNPCRange);
            EventBus.Unsubscribe<PlayerExitedNPCRangeEvent>(OnPlayerExitedNPCRange);
            EventBus.Unsubscribe<NPCInteractionStartedEvent>(OnInteractionStarted);
            EventBus.Unsubscribe<NPCInteractionEndedEvent>(OnInteractionEnded);
        }

        private void Update()
        {
            UpdateFade();
            
            if (_isVisible)
            {
                UpdatePulseAnimation();
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

            if (_canvasGroup.alpha <= 0.01f && _targetAlpha == 0)
            {
                _promptContainer.SetActive(false);
            }
        }

        private void UpdatePulseAnimation()
        {
            float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
            transform.localScale = Vector3.one * (_baseScale * pulse);
        }

        private void OnPlayerEnteredNPCRange(PlayerEnteredNPCRangeEvent evt)
        {
            _currentNPC = evt.NPC;
            _currentNPCName = evt.NPCName;
            Show(evt.NPCName, evt.InteractionPrompt);
        }

        private void OnPlayerExitedNPCRange(PlayerExitedNPCRangeEvent evt)
        {
            if (_currentNPC == evt.NPC)
            {
                _currentNPC = null;
                _currentNPCName = null;
                Hide();
            }
        }

        private void OnInteractionStarted(NPCInteractionStartedEvent evt)
        {
            Hide();
        }

        private void OnInteractionEnded(NPCInteractionEndedEvent evt)
        {
            if (_currentNPC != null && _currentNPC.CanInteract)
            {
                Show(_currentNPCName, _currentNPC.InteractionPrompt);
            }
        }

        public void Show(string npcName, string prompt)
        {
            _isVisible = true;
            _promptContainer.SetActive(true);
            _targetAlpha = 1f;

            if (_npcNameText != null)
            {
                _npcNameText.text = npcName;
                _npcNameText.color = _nameColor;
            }

            if (_promptText != null)
            {
                _promptText.text = prompt;
                _promptText.color = _promptColor;
            }
        }

        public void Hide()
        {
            _isVisible = false;
            _targetAlpha = 0f;
        }
    }
}


