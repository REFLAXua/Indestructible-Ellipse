using UnityEngine;
using Core;
using Core.Events;
using Features.Player.Data;

namespace Features.Player
{
    public class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private PlayerConfigSO _config;

        public float CurrentStamina { get; private set; }
        public float MaxStamina => _config != null ? _config.MaxStamina : 100f;
        public bool IsExhausted { get; private set; }

        private void Awake()
        {
            CurrentStamina = MaxStamina;
        }

        private void Start()
        {
            PublishStaminaChanged();
        }

        private void Update()
        {
            if (_config == null) return;

            if (CurrentStamina < _config.MaxStamina)
            {
                Regenerate(_config.StaminaRegenRate * Time.deltaTime);
            }
        }

        public bool CanConsume(float amount)
        {
            return CurrentStamina >= amount;
        }

        public void Consume(float amount)
        {
            if (_config == null) return;

            CurrentStamina -= amount;
            if (CurrentStamina <= 0)
            {
                CurrentStamina = 0;
                IsExhausted = true;
            }

            PublishStaminaChanged();
        }

        public void Regenerate(float amount)
        {
            if (_config == null) return;

            CurrentStamina += amount;
            if (CurrentStamina > _config.MaxStamina)
            {
                CurrentStamina = _config.MaxStamina;
            }

            if (CurrentStamina >= _config.ExhaustionRecoveryThreshold)
            {
                IsExhausted = false;
            }

            PublishStaminaChanged();
        }

        private void PublishStaminaChanged()
        {
            float max = _config != null ? _config.MaxStamina : 100f;
            EventBus.Publish(new PlayerStaminaChangedEvent
            {
                CurrentStamina = CurrentStamina,
                MaxStamina = max
            });
        }
    }
}
