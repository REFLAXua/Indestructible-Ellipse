using UnityEngine;
using Core;
using Core.Events;
using Features.Player;

namespace Features.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private StatBarView _healthBar;
        [SerializeField] private StatBarView _healthGlobe;

        [Header("Stamina")]
        [SerializeField] private StatBarView _staminaBar;
        [SerializeField] private StatBarView _staminaGlobe;

        [Header("Feedback")]
        [SerializeField] private Animator _hudAnimator;

        private const float LowStaminaThreshold = 0.2f;

        private void Start()
        {
            SubscribeToEvents();
            InitializeWithPlayerValues();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void InitializeWithPlayerValues()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                InitializeFromPlayer(player);
            }
            else
            {
                InitializeWithDefaults();
            }
        }

        private void InitializeFromPlayer(PlayerController player)
        {
            var health = player.GetComponent<PlayerHealth>();
            var stamina = player.GetComponent<PlayerStamina>();

            if (health != null)
            {
                InitializeHealthBars(health.CurrentHealth, health.MaxHealth);
            }

            if (stamina != null)
            {
                InitializeStaminaBars(stamina.CurrentStamina, stamina.MaxStamina);
            }
        }

        private void InitializeWithDefaults()
        {
            InitializeHealthBars(100, 100);
            InitializeStaminaBars(100, 100);
        }

        private void InitializeHealthBars(float current, float max)
        {
            _healthBar?.Initialize(current, max);
            _healthGlobe?.Initialize(current, max);
        }

        private void InitializeStaminaBars(float current, float max)
        {
            _staminaBar?.Initialize(current, max);
            _staminaGlobe?.Initialize(current, max);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt)
        {
            _healthBar?.SetValue(evt.CurrentHealth, evt.MaxHealth);
            _healthGlobe?.SetValue(evt.CurrentHealth, evt.MaxHealth);
        }

        private void OnStaminaChanged(PlayerStaminaChangedEvent evt)
        {
            _staminaBar?.SetValue(evt.CurrentStamina, evt.MaxStamina);
            _staminaGlobe?.SetValue(evt.CurrentStamina, evt.MaxStamina);

            bool isLow = (evt.CurrentStamina / evt.MaxStamina) < LowStaminaThreshold;
            _staminaBar?.PulseLow(isLow);
            _staminaGlobe?.PulseLow(isLow);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (_hudAnimator != null)
            {
                _hudAnimator.SetTrigger("Hide");
            }
        }
    }
}
