using System;
using UnityEngine;

namespace Features.Experience
{
    public class ExperienceService : IExperienceService
    {
        public float CurrentExperience { get; private set; }
        public float CurrentGold { get; private set; }

        public event Action<float> OnExperienceChanged;
        public event Action<float> OnGoldChanged;

        public void AddExperience(float amount)
        {
            if (amount <= 0) return;

            CurrentExperience += amount;
            Debug.Log($"[Experience] Added {amount} XP. Total: {CurrentExperience}");
            
            OnExperienceChanged?.Invoke(CurrentExperience);
        }

        public void AddGold(float amount)
        {
            if (amount <= 0) return;
            CurrentGold += amount;

            Debug.Log($"[Experience] Added {amount} GOLD.");

            OnGoldChanged?.Invoke(CurrentGold);
        }
    }
}
