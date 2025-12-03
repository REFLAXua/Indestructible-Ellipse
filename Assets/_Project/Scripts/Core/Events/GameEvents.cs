namespace Core.Events
{
    public struct PlayerHealthChangedEvent
    {
        public float CurrentHealth;
        public float MaxHealth;
    }

    public struct PlayerStaminaChangedEvent
    {
        public float CurrentStamina;
        public float MaxStamina;
    }

    public struct PlayerDiedEvent { }
}
