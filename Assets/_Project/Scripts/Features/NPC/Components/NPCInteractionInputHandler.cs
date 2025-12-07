using UnityEngine;
using Core;
using Core.Input;
using Features.NPC.Events;
using Features.NPC.Interfaces;

namespace Features.NPC.Components
{
    public class NPCInteractionInputHandler : MonoBehaviour
    {
        private IInputService _inputService;
        private INPCInteractionService _interactionService;
        private INPCInteractable _currentNPC;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out IInputService inputService))
            {
                Debug.LogError("[NPCInteractionInputHandler] InputService не знайдено!");
                enabled = false;
                return;
            }
            _inputService = inputService;

            if (!ServiceLocator.TryGet(out INPCInteractionService interactionService))
            {
                Debug.LogError("[NPCInteractionInputHandler] NPCInteractionService не знайдено!");
                enabled = false;
                return;
            }
            _interactionService = interactionService;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerEnteredNPCRangeEvent>(OnPlayerEnteredRange);
            EventBus.Subscribe<PlayerExitedNPCRangeEvent>(OnPlayerExitedRange);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerEnteredNPCRangeEvent>(OnPlayerEnteredRange);
            EventBus.Unsubscribe<PlayerExitedNPCRangeEvent>(OnPlayerExitedRange);
        }

        private void Update()
        {
            if (_inputService == null) return;

            if (_inputService.IsInteractPressed)
            {
                _interactionService?.TryInteract();
            }

            if (_inputService.IsCancelPressed && _interactionService?.IsInInteraction == true)
            {
                _interactionService.EndCurrentInteraction();
            }
        }

        private void OnPlayerEnteredRange(PlayerEnteredNPCRangeEvent evt)
        {
            _currentNPC = evt.NPC;
        }

        private void OnPlayerExitedRange(PlayerExitedNPCRangeEvent evt)
        {
            if (_currentNPC == evt.NPC)
            {
                _currentNPC = null;
            }
        }
    }
}
