using System;

namespace Features.Experience
{
    public interface IExperienceService
    {
        float CurrentExperience { get; }
        float CurrentGold { get; }

        event Action<float> OnExperienceChanged; // current
        event Action<float> OnGoldChanged; // current

        void AddExperience(float amount);
        void AddGold(float amount);
    }
}
