using UnityEngine;
using Features.NPC.Data;

namespace Features.NPC.Components
{
    [RequireComponent(typeof(NPCController))]
    public class NPCPromptDisplay : MonoBehaviour
    {
        [Header("Prompt References")]
        [SerializeField] private UI.WorldSpacePromptUI _worldSpacePrompt;
        [SerializeField] private bool _useWorldSpacePrompt = true;

        private NPCController _npcController;

        private void Awake()
        {
            _npcController = GetComponent<NPCController>();
            
            if (_useWorldSpacePrompt && _worldSpacePrompt == null)
            {
                CreateWorldSpacePrompt();
            }
        }

        private void OnEnable()
        {
            if (_npcController != null)
            {
                SubscribeToNPCEvents();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromNPCEvents();
        }

        private void SubscribeToNPCEvents()
        {
            Core.EventBus.Subscribe<Events.PlayerEnteredNPCRangeEvent>(OnPlayerEntered);
            Core.EventBus.Subscribe<Events.PlayerExitedNPCRangeEvent>(OnPlayerExited);
            Core.EventBus.Subscribe<Events.NPCInteractionStartedEvent>(OnInteractionStarted);
        }

        private void UnsubscribeFromNPCEvents()
        {
            Core.EventBus.Unsubscribe<Events.PlayerEnteredNPCRangeEvent>(OnPlayerEntered);
            Core.EventBus.Unsubscribe<Events.PlayerExitedNPCRangeEvent>(OnPlayerExited);
            Core.EventBus.Unsubscribe<Events.NPCInteractionStartedEvent>(OnInteractionStarted);
        }

        private void OnPlayerEntered(Events.PlayerEnteredNPCRangeEvent evt)
        {
            if (evt.NPC != _npcController) return;

            if (_worldSpacePrompt != null)
            {
                _worldSpacePrompt.Show(evt.NPCName + "\n" + evt.InteractionPrompt);
            }
        }

        private void OnPlayerExited(Events.PlayerExitedNPCRangeEvent evt)
        {
            if (evt.NPC != _npcController) return;

            if (_worldSpacePrompt != null)
            {
                _worldSpacePrompt.Hide();
            }
        }

        private void OnInteractionStarted(Events.NPCInteractionStartedEvent evt)
        {
            if (evt.NPC != _npcController) return;

            if (_worldSpacePrompt != null)
            {
                _worldSpacePrompt.Hide();
            }
        }

        private void CreateWorldSpacePrompt()
        {
            var promptGO = new GameObject("WorldSpacePrompt");
            promptGO.transform.SetParent(transform);
            
            var config = _npcController.Config;
            var offset = config != null ? config.PromptOffset : new Vector3(0f, 2.5f, 0f);
            promptGO.transform.localPosition = offset;

            _worldSpacePrompt = promptGO.AddComponent<UI.WorldSpacePromptUI>();
        }
    }
}

