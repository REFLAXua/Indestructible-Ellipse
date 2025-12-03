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
            CurrentStamina = _config != null ? _config.MaxStamina : 100f;
        }

        private void Start()
        {
            float max = _config != null ? _config.MaxStamina : 100f;
            EventBus.Publish(new PlayerStaminaChangedEvent { CurrentStamina = CurrentStamina, MaxStamina = max });
        }

        private void Update()
        {
            // Auto regenerate if not exhausted or after some delay? 
            // For now simple regen
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
            CurrentStamina -= amount;
            if (CurrentStamina <= 0)
            {
                CurrentStamina = 0;
                IsExhausted = true;
            }
            
            EventBus.Publish(new PlayerStaminaChangedEvent { CurrentStamina = CurrentStamina, MaxStamina = _config.MaxStamina });
        }

        public void Regenerate(float amount)
        {
            CurrentStamina += amount;
            if (CurrentStamina > _config.MaxStamina)
            {
                CurrentStamina = _config.MaxStamina;
            }
            
            if (CurrentStamina > 15f) // Threshold to recover from exhaustion
            {
                IsExhausted = false;
            }

            EventBus.Publish(new PlayerStaminaChangedEvent { CurrentStamina = CurrentStamina, MaxStamina = _config.MaxStamina });
        }
    }
}
