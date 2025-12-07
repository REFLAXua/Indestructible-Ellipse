using UnityEngine;
using Features.NPC.Interfaces;

namespace Features.NPC
{
    public abstract class NPCInteractionHandlerBase : MonoBehaviour, INPCInteractionHandler
    {
        [Header("Handler Settings")]
        [SerializeField] protected bool _pauseGameOnInteraction;
        [SerializeField] protected bool _disablePlayerMovement = true;

        public abstract void Execute(NPCController npc);

        public virtual void OnInteractionStart(NPCController npc)
        {
            if (_pauseGameOnInteraction)
            {
                Time.timeScale = 0f;
            }

            Debug.Log($"[NPCInteractionHandler] Початок взаємодії з {npc.Config?.NPCName ?? npc.gameObject.name}");
        }

        public virtual void OnInteractionEnd(NPCController npc)
        {
            if (_pauseGameOnInteraction)
            {
                Time.timeScale = 1f;
            }

            Debug.Log($"[NPCInteractionHandler] Завершення взаємодії з {npc.Config?.NPCName ?? npc.gameObject.name}");
        }
    }
}
