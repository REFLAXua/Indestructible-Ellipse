namespace Features.NPC.Interfaces
{
    public interface INPCInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract { get; }
        NPCType NPCType { get; }
        void Interact();
        void OnPlayerEnterRange();
        void OnPlayerExitRange();
    }

    public enum NPCType
    {
        Interactive,
        ConsumablesShop,
        WeaponShop,
        MagicShop,
        SkillShop,
        StatsShop,
        Scarecrow,
        Portal,
        Customization,
        Lootbox,
        PlayerSpawn,
        Arena,
        Generic
    }
}
