using UnityEngine;
using Core;
using Features.Enemy;
using Features.Experience;

namespace Features.Experience.Components
{
    [RequireComponent(typeof(EnemyHealth))]
    public class ExperienceDropper : MonoBehaviour
    {
        [SerializeField] public float _experienceAmount = 10f;
        [SerializeField] public float _goldAmount = 1f;

        private EnemyHealth _enemyHealth;
        private ExperienceVisualisator _visualisator;

        private void Awake()
        {
            _enemyHealth = GetComponent<EnemyHealth>();
            _visualisator = GetComponent<ExperienceVisualisator>();
            
            if (_visualisator == null)
            {
                _visualisator = gameObject.AddComponent<ExperienceVisualisator>();
            }
        }

        private void OnEnable()
        {
            _enemyHealth.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _enemyHealth.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            // Show floating XP and Gold numbers using sprites with different group indices
            _visualisator.ShowFloatingXP(transform.position + Vector3.up * 1f, (int)_experienceAmount, groupIndex: 0);
            _visualisator.ShowFloatingGold(transform.position + Vector3.up * 1f, (int)_goldAmount, groupIndex: 1);

            if (ServiceLocator.TryGet(out IExperienceService experienceService))
            {
                Debug.Log($"[ExperienceDropper] Enemy died. Awarding {_experienceAmount} XP.");
                experienceService.AddExperience(_experienceAmount);
                Debug.Log($"[ExperienceDropper] Enemy died. Awarding {_goldAmount} GOLD.");
                experienceService.AddGold(_goldAmount);
            }
        }
    }
}
