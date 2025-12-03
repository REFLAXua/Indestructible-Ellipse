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
            EventBus.Publish(new PlayerHealthChangedEvent { CurrentHealth = CurrentHealth, MaxHealth = _maxHealth });
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
            EventBus.Publish(new PlayerHealthChangedEvent { CurrentHealth = CurrentHealth, MaxHealth = _maxHealth });

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            EventBus.Publish(new PlayerDiedEvent());
            Destroy(gameObject); // Or disable, or play animation
        }
    }
}
