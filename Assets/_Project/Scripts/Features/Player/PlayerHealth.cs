using UnityEngine;
using Core;
using Core.Events;

namespace Features.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        private void Start()
        {
            PublishHealthChanged();
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Max(0, CurrentHealth);

            PublishHealthChanged();

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            CurrentHealth += amount;
            CurrentHealth = Mathf.Min(CurrentHealth, _maxHealth);

            PublishHealthChanged();
        }

        private void Die()
        {
            EventBus.Publish(new PlayerDiedEvent());
            Destroy(gameObject);
        }

        private void PublishHealthChanged()
        {
            EventBus.Publish(new PlayerHealthChangedEvent
            {
                CurrentHealth = CurrentHealth,
                MaxHealth = _maxHealth
            });
        }
    }
}
