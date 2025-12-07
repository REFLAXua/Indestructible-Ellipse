using Features.NPC.Interfaces;

namespace Features.NPC
{
    public interface INPCInteractionService
    {
        INPCInteractable CurrentNPC { get; }
        bool IsInInteraction { get; }
        bool HasNPCInRange { get; }
        void RegisterNPCInRange(INPCInteractable npc);
        void UnregisterNPCInRange(INPCInteractable npc);
        void TryInteract();
        void EndCurrentInteraction();
    }
}
