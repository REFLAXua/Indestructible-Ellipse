using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class MagicShopHandler : NPCInteractionHandlerBase
    {
        [Header("Shop Settings")]
        [SerializeField] private string _shopId = "magic_shop";
        
        [Header("Shop Capabilities")]
        [SerializeField] private bool _canSellSpells = true;
        [SerializeField] private bool _canUpgradeSpells = true;
        
        [Header("Available Spells")]
        [SerializeField] private SpellItem[] _availableSpells;
        
        [Header("Upgrade Settings")]
        [SerializeField] private int _maxSpellLevel = 5;
        [SerializeField] private float _upgradeCostMultiplier = 2f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onShopOpened;
        [SerializeField] private UnityEvent _onShopClosed;
        [SerializeField] private UnityEvent<string> _onSpellPurchased;
        [SerializeField] private UnityEvent<string, int> _onSpellUpgraded;

        public string ShopId => _shopId;
        public bool CanSellSpells => _canSellSpells;
        public bool CanUpgradeSpells => _canUpgradeSpells;
        public SpellItem[] AvailableSpells => _availableSpells;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[MagicShopHandler] Відкриття магазину магії: {_shopId}");
            Debug.Log($"  - Продаж: {_canSellSpells}, Апгрейди: {_canUpgradeSpells}");
            
            _onShopOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onShopClosed?.Invoke();
        }

        public void PurchaseSpell(string spellId)
        {
            Debug.Log($"[MagicShopHandler] Покупка заклинання: {spellId}");
            _onSpellPurchased?.Invoke(spellId);
        }

        public void UpgradeSpell(string spellId, int currentLevel)
        {
            if (currentLevel >= _maxSpellLevel)
            {
                Debug.LogWarning($"[MagicShopHandler] Заклинання {spellId} вже максимального рівня");
                return;
            }

            int newLevel = currentLevel + 1;
            Debug.Log($"[MagicShopHandler] Апгрейд заклинання: {spellId} до рівня {newLevel}");
            _onSpellUpgraded?.Invoke(spellId, newLevel);
        }

        public int CalculateUpgradeCost(int basePrice, int currentLevel)
        {
            return Mathf.RoundToInt(basePrice * Mathf.Pow(_upgradeCostMultiplier, currentLevel));
        }
    }

    [System.Serializable]
    public struct SpellItem
    {
        public string SpellId;
        public string SpellName;
        public MagicSchool School;
        public SpellType Type;
        public int BasePrice;
        public int ManaCost;
        public float Cooldown;
        public Sprite Icon;
        [TextArea(1, 3)]
        public string Description;
    }

    public enum MagicSchool
    {
        Fire,
        Ice,
        Lightning,
        Earth,
        Light,
        Dark,
        Arcane
    }

    public enum SpellType
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Summon,
        Utility
    }
}
