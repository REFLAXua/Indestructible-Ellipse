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
        [SerializeField] private Animator _hudAnimator; // For screen shake / damage effects

        private void Start()
        {
            // Subscribe to events
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);

            // Initialize with actual values from Player
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                var health = player.GetComponent<PlayerHealth>();
                var stamina = player.GetComponent<PlayerStamina>();

                if (health != null)
                {
                    if (_healthBar != null) _healthBar.Initialize(health.CurrentHealth, health.MaxHealth);
                    if (_healthGlobe != null) _healthGlobe.Initialize(health.CurrentHealth, health.MaxHealth);
                }
                
                if (stamina != null)
                {
                    if (_staminaBar != null) _staminaBar.Initialize(stamina.CurrentStamina, stamina.MaxStamina);
                    if (_staminaGlobe != null) _staminaGlobe.Initialize(stamina.CurrentStamina, stamina.MaxStamina);
                }
            }
            else
            {
                // Fallback if player not found yet
                if (_healthBar != null) _healthBar.Initialize(100, 100);
                if (_healthGlobe != null) _healthGlobe.Initialize(100, 100);
                if (_staminaBar != null) _staminaBar.Initialize(100, 100);
                if (_staminaGlobe != null) _staminaGlobe.Initialize(100, 100);
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt)
        {
            Debug.Log($"UI Received Health Update: {evt.CurrentHealth}/{evt.MaxHealth}");
            if (_healthBar != null) _healthBar.SetValue(evt.CurrentHealth, evt.MaxHealth);
            if (_healthGlobe != null) _healthGlobe.SetValue(evt.CurrentHealth, evt.MaxHealth);
        }

        private void OnStaminaChanged(PlayerStaminaChangedEvent evt)
        {
            if (_staminaBar != null) _staminaBar.SetValue(evt.CurrentStamina, evt.MaxStamina);
            if (_staminaGlobe != null) _staminaGlobe.SetValue(evt.CurrentStamina, evt.MaxStamina);

            // Pulse if low stamina (< 20%)
            bool isLow = (evt.CurrentStamina / evt.MaxStamina) < 0.2f;
            if (_staminaBar != null) _staminaBar.PulseLow(isLow);
            if (_staminaGlobe != null) _staminaGlobe.PulseLow(isLow);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            // Hide HUD or show Game Over screen
            // _hudAnimator.SetTrigger("Hide");
        }
    }
}
