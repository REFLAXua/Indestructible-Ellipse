using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class GenericHandler : NPCInteractionHandlerBase
    {
        [Header("Generic Settings")]
        [SerializeField] private string _interactionId;
        
        [Header("Custom Events")]
        [SerializeField] private UnityEvent _onExecute;
        [SerializeField] private UnityEvent<NPCController> _onExecuteWithContext;

        public string InteractionId => _interactionId;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[GenericHandler] Виконання взаємодії: {_interactionId}");
            
            _onExecute?.Invoke();
            _onExecuteWithContext?.Invoke(npc);
        }
    }
}
