using UnityEngine;
using Core;
using Features.NPC.Interfaces;
using Features.NPC.Events;

namespace Features.NPC
{
    public sealed class NPCInteractionService : INPCInteractionService
    {
        private INPCInteractable _currentNPC;
        private bool _isInInteraction;

        public INPCInteractable CurrentNPC => _currentNPC;
        public bool IsInInteraction => _isInInteraction;
        public bool HasNPCInRange => _currentNPC != null;

        public NPCInteractionService()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<PlayerEnteredNPCRangeEvent>(OnPlayerEnteredRange);
            EventBus.Subscribe<PlayerExitedNPCRangeEvent>(OnPlayerExitedRange);
            EventBus.Subscribe<NPCInteractionStartedEvent>(OnInteractionStarted);
            EventBus.Subscribe<NPCInteractionEndedEvent>(OnInteractionEnded);
        }

        public void Unsubscribe()
        {
            EventBus.Unsubscribe<PlayerEnteredNPCRangeEvent>(OnPlayerEnteredRange);
            EventBus.Unsubscribe<PlayerExitedNPCRangeEvent>(OnPlayerExitedRange);
            EventBus.Unsubscribe<NPCInteractionStartedEvent>(OnInteractionStarted);
            EventBus.Unsubscribe<NPCInteractionEndedEvent>(OnInteractionEnded);
        }

        private void OnPlayerEnteredRange(PlayerEnteredNPCRangeEvent evt)
        {
            _currentNPC = evt.NPC;
            Debug.Log($"[NPCInteractionService] Гравець увійшов в зону NPC: {evt.NPCType}");
        }

        private void OnPlayerExitedRange(PlayerExitedNPCRangeEvent evt)
        {
            if (_currentNPC == evt.NPC)
            {
                _currentNPC = null;
                Debug.Log("[NPCInteractionService] Гравець вийшов із зони NPC");
            }
        }

        private void OnInteractionStarted(NPCInteractionStartedEvent evt)
        {
            _isInInteraction = true;
            Debug.Log($"[NPCInteractionService] Взаємодія розпочата: {evt.NPCType}");
        }

        private void OnInteractionEnded(NPCInteractionEndedEvent evt)
        {
            _isInInteraction = false;
            Debug.Log("[NPCInteractionService] Взаємодія завершена");
        }

        public void RegisterNPCInRange(INPCInteractable npc)
        {
            if (npc == null) return;

            if (_currentNPC == null || ShouldPrioritize(npc))
            {
                _currentNPC = npc;
            }
        }

        public void UnregisterNPCInRange(INPCInteractable npc)
        {
            if (_currentNPC == npc)
            {
                _currentNPC = null;
            }
        }

        public void TryInteract()
        {
            if (_currentNPC == null || _isInInteraction) return;

            if (_currentNPC.CanInteract)
            {
                _currentNPC.Interact();
            }
        }

        public void EndCurrentInteraction()
        {
            _isInInteraction = false;
            
            EventBus.Publish(new NPCInteractionEndedEvent { NPC = _currentNPC });
        }

        private bool ShouldPrioritize(INPCInteractable newNPC)
        {
            return false;
        }
    }
}
