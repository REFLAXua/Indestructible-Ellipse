using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class SkillShopHandler : NPCInteractionHandlerBase
    {
        [Header("Shop Settings")]
        [SerializeField] private string _shopId = "skill_shop";
        
        [Header("Shop Capabilities")]
        [SerializeField] private bool _canSellSkills = true;
        [SerializeField] private bool _canUpgradeSkills = true;
        
        [Header("Available Skills")]
        [SerializeField] private SkillItem[] _availableSkills;
        
        [Header("Upgrade Settings")]
        [SerializeField] private int _maxSkillLevel = 5;
        [SerializeField] private float _upgradeCostMultiplier = 1.75f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onShopOpened;
        [SerializeField] private UnityEvent _onShopClosed;
        [SerializeField] private UnityEvent<string> _onSkillPurchased;
        [SerializeField] private UnityEvent<string, int> _onSkillUpgraded;

        public string ShopId => _shopId;
        public bool CanSellSkills => _canSellSkills;
        public bool CanUpgradeSkills => _canUpgradeSkills;
        public SkillItem[] AvailableSkills => _availableSkills;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[SkillShopHandler] Відкриття магазину умінь: {_shopId}");
            Debug.Log($"  - Продаж: {_canSellSkills}, Апгрейди: {_canUpgradeSkills}");
            
            _onShopOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onShopClosed?.Invoke();
        }

        public void PurchaseSkill(string skillId)
        {
            Debug.Log($"[SkillShopHandler] Покупка уміння: {skillId}");
            _onSkillPurchased?.Invoke(skillId);
        }

        public void UpgradeSkill(string skillId, int currentLevel)
        {
            if (currentLevel >= _maxSkillLevel)
            {
                Debug.LogWarning($"[SkillShopHandler] Уміння {skillId} вже максимального рівня");
                return;
            }

            int newLevel = currentLevel + 1;
            Debug.Log($"[SkillShopHandler] Апгрейд уміння: {skillId} до рівня {newLevel}");
            _onSkillUpgraded?.Invoke(skillId, newLevel);
        }

        public int CalculateUpgradeCost(int basePrice, int currentLevel)
        {
            return Mathf.RoundToInt(basePrice * Mathf.Pow(_upgradeCostMultiplier, currentLevel));
        }
    }

    [System.Serializable]
    public struct SkillItem
    {
        public string SkillId;
        public string SkillName;
        public SkillTreeCategory Category;
        public int BasePrice;
        public int RequiredLevel;
        public string[] Prerequisites;
        public Sprite Icon;
        [TextArea(1, 3)]
        public string Description;
    }

    public enum SkillTreeCategory
    {
        Combat,
        Defense,
        Magic,
        Stealth,
        Survival,
        Utility
    }
}
