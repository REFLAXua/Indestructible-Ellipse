using UnityEngine;
using Features.NPC.Interfaces;

namespace Features.NPC.Data
{
    [CreateAssetMenu(fileName = "TrainerConfig", menuName = "LetsRoll/NPC/Trainer Config")]
    public class TrainerConfigSO : NPCConfigSO
    {
        [Header("Trainer Specific")]
        [SerializeField] private string _trainerId;
        [SerializeField] private TrainerSpecialization _specialization;
        [SerializeField] private int _requiredPlayerLevel = 1;
        [SerializeField] private float _trainingCostMultiplier = 1f;

        public string TrainerId => _trainerId;
        public TrainerSpecialization Specialization => _specialization;
        public int RequiredPlayerLevel => _requiredPlayerLevel;
        public float TrainingCostMultiplier => _trainingCostMultiplier;

        private void OnValidate()
        {
            _requiredPlayerLevel = Mathf.Max(1, _requiredPlayerLevel);
            _trainingCostMultiplier = Mathf.Max(0.1f, _trainingCostMultiplier);
        }
    }

    public enum TrainerSpecialization
    {
        Combat,
        Magic,
        Stealth,
        Crafting,
        All
    }
}
