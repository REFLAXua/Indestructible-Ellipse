using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class QuestGiverHandler : NPCInteractionHandlerBase
    {
        [Header("Quest Settings")]
        [SerializeField] private string _questGiverId;
        [SerializeField] private QuestData[] _availableQuests;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onQuestMenuOpened;
        [SerializeField] private UnityEvent _onQuestMenuClosed;
        [SerializeField] private UnityEvent<string> _onQuestAccepted;
        [SerializeField] private UnityEvent<string> _onQuestCompleted;

        public string QuestGiverId => _questGiverId;
        public QuestData[] AvailableQuests => _availableQuests;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[QuestGiverHandler] Відкриття меню квестів: {_questGiverId}");
            Debug.Log($"  - Доступні квести: {_availableQuests?.Length ?? 0}");
            
            _onQuestMenuOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onQuestMenuClosed?.Invoke();
        }

        public void AcceptQuest(string questId)
        {
            Debug.Log($"[QuestGiverHandler] Квест прийнято: {questId}");
            _onQuestAccepted?.Invoke(questId);
        }

        public void CompleteQuest(string questId)
        {
            Debug.Log($"[QuestGiverHandler] Квест завершено: {questId}");
            _onQuestCompleted?.Invoke(questId);
        }
    }

    [System.Serializable]
    public struct QuestData
    {
        public string QuestId;
        public string QuestName;
        [TextArea(2, 4)]
        public string QuestDescription;
        public QuestState State;
    }

    public enum QuestState
    {
        NotStarted,
        InProgress,
        Completed,
        TurnedIn
    }
}
