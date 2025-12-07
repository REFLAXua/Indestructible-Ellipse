using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class HealerHandler : NPCInteractionHandlerBase
    {
        [Header("Healer Settings")]
        [SerializeField] private string _healerId;
        [SerializeField] private float _healCostPerPoint = 1f;
        [SerializeField] private bool _canRemoveDebuffs = true;
        [SerializeField] private bool _canResurrect = false;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onHealerMenuOpened;
        [SerializeField] private UnityEvent _onHealerMenuClosed;
        [SerializeField] private UnityEvent _onPlayerHealed;

        public string HealerId => _healerId;
        public float HealCostPerPoint => _healCostPerPoint;
        public bool CanRemoveDebuffs => _canRemoveDebuffs;
        public bool CanResurrect => _canResurrect;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[HealerHandler] Відкриття меню цілителя: {_healerId}");
            
            _onHealerMenuOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onHealerMenuClosed?.Invoke();
        }

        public void HealPlayer(float amount)
        {
            Debug.Log($"[HealerHandler] Відновлення {amount} здоров'я");
            _onPlayerHealed?.Invoke();
        }
    }
}
