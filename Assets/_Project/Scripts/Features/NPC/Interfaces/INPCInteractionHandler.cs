namespace Features.NPC.Interfaces
{
    public interface INPCInteractionHandler
    {
        void Execute(NPCController npc);
        void OnInteractionStart(NPCController npc);
        void OnInteractionEnd(NPCController npc);
    }
}
