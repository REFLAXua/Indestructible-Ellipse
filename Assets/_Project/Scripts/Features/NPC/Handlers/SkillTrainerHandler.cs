using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class SkillTrainerHandler : NPCInteractionHandlerBase
    {
        [Header("Trainer Settings")]
        [SerializeField] private string _trainerId;
        [SerializeField] private SkillCategory[] _availableSkillCategories;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onTrainingMenuOpened;
        [SerializeField] private UnityEvent _onTrainingMenuClosed;

        public string TrainerId => _trainerId;
        public SkillCategory[] AvailableSkillCategories => _availableSkillCategories;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[SkillTrainerHandler] Відкриття меню тренування: {_trainerId}");
            
            _onTrainingMenuOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onTrainingMenuClosed?.Invoke();
        }
    }

    public enum SkillCategory
    {
        Combat,
        Magic,
        Stealth,
        Crafting,
        Survival
    }
}
