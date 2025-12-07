using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class ScarecrowHandler : NPCInteractionHandlerBase
    {
        [Header("Scarecrow Settings")]
        [SerializeField] private string _scarecrowId = "training_dummy";
        [SerializeField] private float _maxHealth = 1000f;
        [SerializeField] private bool _isInvincible = true;
        [SerializeField] private float _healthRegenDelay = 3f;
        [SerializeField] private float _healthRegenRate = 100f;
        
        [Header("Damage Display")]
        [SerializeField] private bool _showDamageNumbers = true;
        [SerializeField] private bool _showDPS = true;
        [SerializeField] private float _dpsWindowSeconds = 5f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onTrainingStarted;
        [SerializeField] private UnityEvent _onTrainingEnded;
        [SerializeField] private UnityEvent<float> _onDamageReceived;
        [SerializeField] private UnityEvent<float> _onDPSCalculated;

        private float _currentHealth;
        private float _totalDamageInWindow;
        private float _windowStartTime;
        private float _lastDamageTime;
        private bool _isTraining;

        public string ScarecrowId => _scarecrowId;
        public float CurrentHealth => _currentHealth;
        public bool IsTraining => _isTraining;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public override void Execute(NPCController npc)
        {
            _isTraining = true;
            _windowStartTime = Time.time;
            _totalDamageInWindow = 0f;
            
            Debug.Log($"[ScarecrowHandler] Початок тренування: {_scarecrowId}");
            _onTrainingStarted?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _isTraining = false;
            _onTrainingEnded?.Invoke();
        }

        private void Update()
        {
            if (!_isTraining) return;

            if (_showDPS)
            {
                UpdateDPSCalculation();
            }

            if (_currentHealth < _maxHealth && Time.time - _lastDamageTime > _healthRegenDelay)
            {
                RegenerateHealth();
            }
        }

        public void TakeDamage(float damage)
        {
            if (!_isTraining) return;

            _lastDamageTime = Time.time;
            
            if (!_isInvincible)
            {
                _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            }

            _totalDamageInWindow += damage;
            
            if (_showDamageNumbers)
            {
                Debug.Log($"[ScarecrowHandler] Отримано {damage:F1} урону");
            }
            
            _onDamageReceived?.Invoke(damage);
        }

        private void RegenerateHealth()
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + _healthRegenRate * Time.deltaTime);
        }

        private void UpdateDPSCalculation()
        {
            float elapsed = Time.time - _windowStartTime;
            
            if (elapsed >= _dpsWindowSeconds)
            {
                float dps = _totalDamageInWindow / _dpsWindowSeconds;
                Debug.Log($"[ScarecrowHandler] DPS: {dps:F1}");
                _onDPSCalculated?.Invoke(dps);
                
                _windowStartTime = Time.time;
                _totalDamageInWindow = 0f;
            }
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _totalDamageInWindow = 0f;
            Debug.Log("[ScarecrowHandler] Здоров'я відновлено");
        }
    }
}
