using UnityEngine;
using UnityEngine.Events;
using Core;
using Features.NPC.Events;

namespace Features.NPC.Handlers
{
    public class DialogueHandler : NPCInteractionHandlerBase
    {
        [Header("Dialogue Settings")]
        [SerializeField] private string _dialogueId;
        [SerializeField] private DialogueEntry[] _dialogueEntries;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onDialogueStarted;
        [SerializeField] private UnityEvent _onDialogueEnded;
        [SerializeField] private UnityEvent<int> _onDialogueLineChanged;

        private int _currentDialogueIndex;

        public string DialogueId => _dialogueId;
        public DialogueEntry[] DialogueEntries => _dialogueEntries;
        public int CurrentDialogueIndex => _currentDialogueIndex;

        public override void Execute(NPCController npc)
        {
            _currentDialogueIndex = 0;

            Debug.Log($"[DialogueHandler] Початок діалогу: {_dialogueId}");

            EventBus.Publish(new NPCDialogueRequestEvent
            {
                DialogueId = _dialogueId,
                NPC = npc
            });

            _onDialogueStarted?.Invoke();
            
            if (_dialogueEntries != null && _dialogueEntries.Length > 0)
            {
                ShowCurrentDialogueLine();
            }
        }

        public void AdvanceDialogue(NPCController npc)
        {
            _currentDialogueIndex++;

            if (_dialogueEntries == null || _currentDialogueIndex >= _dialogueEntries.Length)
            {
                EndDialogue(npc);
                return;
            }

            ShowCurrentDialogueLine();
        }

        private void ShowCurrentDialogueLine()
        {
            if (_dialogueEntries == null || _currentDialogueIndex >= _dialogueEntries.Length) return;

            var entry = _dialogueEntries[_currentDialogueIndex];
            Debug.Log($"[DialogueHandler] {entry.SpeakerName}: {entry.DialogueText}");
            
            _onDialogueLineChanged?.Invoke(_currentDialogueIndex);
        }

        private void EndDialogue(NPCController npc)
        {
            Debug.Log($"[DialogueHandler] Завершення діалогу: {_dialogueId}");
            _onDialogueEnded?.Invoke();
            npc.EndInteraction();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _currentDialogueIndex = 0;
        }
    }

    [System.Serializable]
    public struct DialogueEntry
    {
        public string SpeakerName;
        [TextArea(2, 5)]
        public string DialogueText;
        public float DisplayDuration;
        public AudioClip VoiceClip;
    }
}
