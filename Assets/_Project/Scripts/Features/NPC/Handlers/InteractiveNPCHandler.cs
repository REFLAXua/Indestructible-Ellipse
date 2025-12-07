using UnityEngine;
using UnityEngine.Events;
using Core;
using Features.NPC.Events;

namespace Features.NPC.Handlers
{
    public class InteractiveNPCHandler : NPCInteractionHandlerBase
    {
        [Header("NPC Settings")]
        [SerializeField] private string _npcId;
        [SerializeField] private InteractiveNPCRole _role = InteractiveNPCRole.Info;
        
        [Header("Dialogue")]
        [SerializeField] private DialogueEntry[] _dialogueEntries;
        
        [Header("Quest (if Quest Giver)")]
        [SerializeField] private QuestData[] _availableQuests;
        
        [Header("Tutorial (if Trainer)")]
        [SerializeField] private TutorialStep[] _tutorialSteps;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onInteractionStarted;
        [SerializeField] private UnityEvent _onInteractionEnded;
        [SerializeField] private UnityEvent<int> _onDialogueAdvanced;
        [SerializeField] private UnityEvent<string> _onQuestAccepted;
        [SerializeField] private UnityEvent<int> _onTutorialStepCompleted;

        private int _currentDialogueIndex;
        private int _currentTutorialStep;

        public string NPCId => _npcId;
        public InteractiveNPCRole Role => _role;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[InteractiveNPCHandler] Взаємодія з {_npcId}, роль: {_role}");
            
            _currentDialogueIndex = 0;
            _onInteractionStarted?.Invoke();

            switch (_role)
            {
                case InteractiveNPCRole.Info:
                    ShowDialogue();
                    break;
                case InteractiveNPCRole.QuestGiver:
                    ShowQuestMenu(npc);
                    break;
                case InteractiveNPCRole.Trainer:
                    StartTutorial(npc);
                    break;
                case InteractiveNPCRole.Combined:
                    ShowDialogue();
                    break;
            }
        }

        private void ShowDialogue()
        {
            if (_dialogueEntries == null || _dialogueEntries.Length == 0)
            {
                Debug.LogWarning("[InteractiveNPCHandler] Немає діалогів");
                return;
            }

            var entry = _dialogueEntries[_currentDialogueIndex];
            Debug.Log($"[InteractiveNPCHandler] {entry.SpeakerName}: {entry.DialogueText}");
            _onDialogueAdvanced?.Invoke(_currentDialogueIndex);
        }

        public void AdvanceDialogue(NPCController npc)
        {
            _currentDialogueIndex++;
            
            if (_currentDialogueIndex >= _dialogueEntries.Length)
            {
                if (_role == InteractiveNPCRole.Combined)
                {
                    ShowQuestMenu(npc);
                }
                else
                {
                    npc.EndInteraction();
                }
                return;
            }

            ShowDialogue();
        }

        private void ShowQuestMenu(NPCController npc)
        {
            if (_availableQuests == null || _availableQuests.Length == 0)
            {
                Debug.Log("[InteractiveNPCHandler] Немає доступних квестів");
                return;
            }

            Debug.Log($"[InteractiveNPCHandler] Доступні квести: {_availableQuests.Length}");
            foreach (var quest in _availableQuests)
            {
                Debug.Log($"  - {quest.QuestName}: {quest.State}");
            }
        }

        public void AcceptQuest(string questId)
        {
            Debug.Log($"[InteractiveNPCHandler] Прийнято квест: {questId}");
            _onQuestAccepted?.Invoke(questId);
        }

        private void StartTutorial(NPCController npc)
        {
            if (_tutorialSteps == null || _tutorialSteps.Length == 0)
            {
                Debug.LogWarning("[InteractiveNPCHandler] Немає кроків туторіалу");
                return;
            }

            _currentTutorialStep = 0;
            ShowTutorialStep();
        }

        private void ShowTutorialStep()
        {
            var step = _tutorialSteps[_currentTutorialStep];
            Debug.Log($"[InteractiveNPCHandler] Туторіал [{_currentTutorialStep + 1}/{_tutorialSteps.Length}]: {step.Title}");
            Debug.Log($"  {step.Description}");
        }

        public void CompleteTutorialStep(NPCController npc)
        {
            _onTutorialStepCompleted?.Invoke(_currentTutorialStep);
            _currentTutorialStep++;

            if (_currentTutorialStep >= _tutorialSteps.Length)
            {
                Debug.Log("[InteractiveNPCHandler] Туторіал завершено!");
                npc.EndInteraction();
                return;
            }

            ShowTutorialStep();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _currentDialogueIndex = 0;
            _currentTutorialStep = 0;
            _onInteractionEnded?.Invoke();
        }
    }

    public enum InteractiveNPCRole
    {
        Info,
        QuestGiver,
        Trainer,
        Combined
    }

    [System.Serializable]
    public struct TutorialStep
    {
        public string Title;
        [TextArea(2, 4)]
        public string Description;
        public TutorialActionType ActionType;
        public string RequiredInput;
        public float TimeoutSeconds;
    }

    public enum TutorialActionType
    {
        Read,
        PerformAction,
        DefeatEnemy,
        UseItem,
        Navigate
    }
}
