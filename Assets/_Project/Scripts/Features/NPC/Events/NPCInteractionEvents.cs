using Features.NPC.Interfaces;

namespace Features.NPC.Events
{
    public struct PlayerEnteredNPCRangeEvent
    {
        public INPCInteractable NPC;
        public string NPCName;
        public string InteractionPrompt;
        public NPCType NPCType;
    }

    public struct PlayerExitedNPCRangeEvent
    {
        public INPCInteractable NPC;
    }

    public struct NPCInteractionStartedEvent
    {
        public INPCInteractable NPC;
        public string NPCName;
        public NPCType NPCType;
    }

    public struct NPCInteractionEndedEvent
    {
        public INPCInteractable NPC;
    }

    public struct NPCDialogueRequestEvent
    {
        public string DialogueId;
        public INPCInteractable NPC;
    }
}

