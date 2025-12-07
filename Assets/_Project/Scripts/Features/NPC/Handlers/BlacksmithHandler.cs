using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class BlacksmithHandler : NPCInteractionHandlerBase
    {
        [Header("Blacksmith Settings")]
        [SerializeField] private string _blacksmithId;
        [SerializeField] private bool _canRepairItems = true;
        [SerializeField] private bool _canUpgradeWeapons = true;
        [SerializeField] private bool _canCraftArmor = true;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onForgeMenuOpened;
        [SerializeField] private UnityEvent _onForgeMenuClosed;

        public string BlacksmithId => _blacksmithId;
        public bool CanRepairItems => _canRepairItems;
        public bool CanUpgradeWeapons => _canUpgradeWeapons;
        public bool CanCraftArmor => _canCraftArmor;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[BlacksmithHandler] Відкриття кузні: {_blacksmithId}");
            Debug.Log($"  - Ремонт: {_canRepairItems}, Покращення: {_canUpgradeWeapons}, Крафт: {_canCraftArmor}");
            
            _onForgeMenuOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onForgeMenuClosed?.Invoke();
        }
    }
}
